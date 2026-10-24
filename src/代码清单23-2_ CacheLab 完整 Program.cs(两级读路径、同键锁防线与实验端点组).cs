using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// 连接串口径与测试底账一致：环境变量 LAB_TEST_DATABASE_URL 优先注入，
// 缺省才回落到 appsettings 里的本地占位串（localhost + 库名 + 占位，零真实凭据）。
// 库名在此统一改写为本章隔离验证库 cachelab_ch23：只换库名，不触碰凭据字段。
var connectionString =
    Environment.GetEnvironmentVariable("LAB_TEST_DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("Default");
var csb = new NpgsqlConnectionStringBuilder(connectionString) { Database = "cachelab_ch23" };
connectionString = csb.ConnectionString;

builder.Services.AddDbContext<AppDb>(options => options.UseNpgsql(connectionString));

// L2：IDistributedCache 的 Redis 供应。InstanceName 是键前缀，
// 防止多工程共用同一 Redis 实例时互相串键。
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:Configuration"] ?? "localhost:6379";
    options.InstanceName = "cachelab:";
});

// L1：进程内存缓存，随共享框架而来，零引包。
builder.Services.AddMemoryCache();

var app = builder.Build();

// ---- 可观测状态与缓存策略参数 ----

// 回源计数器：每真正查询一次数据库就自增。击穿与否不看故事看这个数。
long dbBackfillCount = 0;

// 同键互斥锁表：每个缓存键配一把信号量（单实例进程内锁，即本章教学落点；
// 跨实例的真分布式锁需要 Redis 侧锁设施，此处不展开）。
ConcurrentDictionary<string, SemaphoreSlim> keyLocks = new();

TimeSpan l1Ttl = TimeSpan.FromSeconds(30);   // L1 短命：进程内存珍贵，也避免多实例各自为政太久
TimeSpan l2Ttl = TimeSpan.FromMinutes(2);    // L2 长命：跨请求、跨进程的共享层

// 模拟远程往返的显式延迟 300 毫秒：叠加在真实远程库往返之上，
// 用来放大击穿窗口，让"并发打库"在墙钟时间上也看得见。数据本身（计数）不受它影响。
TimeSpan simulatedRemoteLatency = TimeSpan.FromMilliseconds(300);

var memoryCache = app.Services.GetRequiredService<IMemoryCache>();
var distCache = app.Services.GetRequiredService<IDistributedCache>();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// ---- 读路径：L1 → L2 → 回源，两级都未中才碰数据库 ----

// 依次试 L1、L2。命中返回响应，双未中返回 null。
async Task<IResult?> TryFromCacheAsync(string cacheKey)
{
    // L1：进程内存，命中即返，零网络往返。
    if (memoryCache.TryGetValue(cacheKey, out Order? hot))
        return Results.Json(new { source = "L1", order = hot });

    // L2：Redis 存的是 byte[]，取出来必须反序列化成对象才能用。
    byte[]? stored = null;
    try
    {
        stored = await distCache.GetAsync(cacheKey);
    }
    catch (RedisConnectionException ex)
    {
        // Redis 连不上的降级形态：把 L2 视作未命中，链路不断，日志留痕。
        // 缓存层的信条：缓存坏了最多慢，不该把业务读挂掉。
        logger.LogWarning(ex, "L2 不可达，降级为 L1+回源（key={CacheKey}）", cacheKey);
    }

    if (stored is null) return null;
    var order = JsonSerializer.Deserialize<Order>(stored);
    if (order is null) return null;

    // L2 命中顺手回填 L1：下一请求连 Redis 都不用问。
    memoryCache.Set(cacheKey, order, l1Ttl);
    return Results.Json(new { source = "L2", order });
}

// 回源：查库 → 回填 L2 与 L1。计数器在这里自增，是击穿实验的唯一观测点。
async Task<IResult> BackfillAsync(AppDb db, string cacheKey, int id)
{
    Interlocked.Increment(ref dbBackfillCount);

    await Task.Delay(simulatedRemoteLatency);
    var order = await db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);
    if (order is null)
        return Results.NotFound(new { message = $"order {id} not found", cacheKey });

    // 对象进分布式缓存必经序列化：对象 → JSON 字节（byte[]）。
    // 回填失败同样不致命：本轮还有 L1 顶着，下轮重新回源。缓存坏只降级，不炸响应。
    try
    {
        await distCache.SetAsync(cacheKey, JsonSerializer.SerializeToUtf8Bytes(order),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = l2Ttl });
    }
    catch (RedisConnectionException ex)
    {
        logger.LogWarning(ex, "L2 回填失败（key={CacheKey}），本轮降级为 L1", cacheKey);
    }
    memoryCache.Set(cacheKey, order, l1Ttl);

    return Results.Json(new { source = "DB", order });
}

async Task<IResult> ReadOrderAsync(AppDb db, int id, bool useLock)
{
    var cacheKey = $"order:{id}";

    var hit = await TryFromCacheAsync(cacheKey);
    if (hit is not null) return hit;

    if (useLock)
    {
        // 同键互斥：热点键失效的瞬间，只放一个请求回源，其余等它补完直接取。
        var keyLock = keyLocks.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));
        await keyLock.WaitAsync();
        try
        {
            // 等锁期间缓存多半已被同键请求补好——锁内再查一次，才轮到回源。
            // 没有这个双重检查，排队请求会在锁内逐个重复回源，锁就白加了。
            hit = await TryFromCacheAsync(cacheKey);
            if (hit is not null) return hit;

            return await BackfillAsync(db, cacheKey, id);
        }
        finally { keyLock.Release(); }
    }

    // 无锁对照路径：实验用。生产读路径应当带锁。
    return await BackfillAsync(db, cacheKey, id);
}

// ---- 路由（沿 ch19 体例：MapGroup 分组）----

var experiment = app.MapGroup("/experiment");

// 击穿实验专用读端点：useLock=false 即无锁对照轮。
experiment.MapGet("/order/{id}", async (AppDb db, int id, bool useLock = true) =>
    await ReadOrderAsync(db, id, useLock));

// 清场：清指定键的两级缓存并归零计数，保证每轮实验从同一空缓存起跑。
experiment.MapPost("/reset/{id}", async (int id) =>
{
    var cacheKey = $"order:{id}";
    memoryCache.Remove(cacheKey);
    try { await distCache.RemoveAsync(cacheKey); }
    catch (RedisConnectionException ex)
    {
        logger.LogWarning(ex, "L2 清理失败（key={CacheKey}），L1 已清", cacheKey);
    }
    Interlocked.Exchange(ref dbBackfillCount, 0);
    return Results.Json(new { reset = cacheKey, backfillCount = 0 });
});

// 观测端点：回源计数回显。
experiment.MapGet("/stats", () =>
    Results.Json(new { backfillCount = Interlocked.Read(ref dbBackfillCount) }));

// 教学正脸端点：带锁防线的读路径。
app.MapGet("/orders/{id}", async (AppDb db, int id) => await ReadOrderAsync(db, id, useLock: true));

await DbSeed.EnsureSeededAsync(app.Services);
app.Run();
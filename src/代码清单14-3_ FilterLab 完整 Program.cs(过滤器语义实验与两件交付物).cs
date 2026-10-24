// 文件位置: FilterLab/Program.cs, 整体替换 dotnet new web 模板生成的那份(完整文件, 覆盖后即可运行)
// 与前几章的关系: 零第三方包, 端点过滤器机制全部框架内置;
// 端口 5800 来自清单14-2 的 Kestrel 配置; 环境分界沿用第 13 章:
// Production 才挂管道层兜底, Development 交给框架默认的开发者异常页
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// 过滤器要注入的作用域服务: 出生即领一枚 Guid 实例 Id, 与第 8 章作用域实验同一套观测手法;
// 过滤器实例自己是否每请求新建, 也用同一手法观测
builder.Services.AddScoped<ScopeIdService>();

var app = builder.Build();

// 环境分界沿用第 13 章: Production 才挂管道层兜底, Development 交给框架默认的开发者异常页;
// 这道分界正是「过滤器接住异常后管道层还看不看得见」对照实验的前提
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/oops");
}

// 管道层兜底的落脚端点: 只有异常飞出过滤器、飞出端点没人接时才会被触达,
// 它一旦出现, 就说明异常穿透了过滤器这一层
app.MapGet("/oops", (HttpContext httpContext) =>
{
    app.Logger.LogError("管道层 /oops 兜底被触达, 异常穿透了过滤器: {Path}", httpContext.Request.Path);
    return Results.Text("服务器出了意外, 请稍后重试。", statusCode: StatusCodes.Status500InternalServerError);
});

// 入口: 报出全部演示路径, 与前几章同一形态
app.MapGet("/", () => Results.Text(
    "这是 FilterLab 的入口, 演示路径: /probe 、 /chain 、 /short 、 /noshort 、 /orders/{编号} 、 /ex/missing 、 /ex/conflict 、 /ex/unexpected 。"));

// 内存订单库: 3 笔订单, 编号 1 到 3; 独立项目仓库从零起, 互不残留;
// 订单 1 特意置为已取消, 给 /ex/conflict 的冲突案备用
var orderStore = new[]
{
    new { Id = 1, Description = "云海咖啡豆 x2", Status = "已取消" },
    new { Id = 2, Description = "阿里山茶行 x1", Status = "已签收" },
    new { Id = 3, Description = "青屿生鲜 x5", Status = "配送中" }
};

// 语义实验一: 类型注册的过滤器, 实例是每请求新建还是缓存复用——
// 过滤器与它注入的作用域服务各持一枚 Guid, 两次请求对比四枚 Id 就能看出答案
app.MapGet("/probe", (HttpContext httpContext) =>
        Results.Text($"端点已执行, 请求作用域服务 Id = {httpContext.RequestServices.GetRequiredService<ScopeIdService>().Id}"))
    .AddEndpointFilter<InstanceProbeFilter>();

// 语义实验二: 链式两个过滤器挂同一端点, 执行次序看日志行的进出配对
app.MapGet("/chain", () => Results.Text("端点本体已执行, 次序看应用终端日志。"))
    .AddEndpointFilter<ChainOuterFilter>()
    .AddEndpointFilter<ChainInnerFilter>();

// 语义实验三: 短路——过滤器不调 next 直接返回值, 端点函数还执不执行、响应长什么样;
// /noshort 是不短路的对照路径, 同一个端点形态换个过滤器就能对出差异
app.MapGet("/short", () => Results.Text("端点本体: 你不该看到这句话。"))
    .AddEndpointFilter<ShortCircuitFilter>();
app.MapGet("/noshort", () => Results.Text("端点本体: 未短路, 一路放行。"))
    .AddEndpointFilter<PassthroughFilter>();

// 交付物一: 操作审计——进出各记一条日志, 进入记方法与路径, 离开记状态码;
// 任何要盯「谁在什么时候动了哪些订单」的场景, 都可以用这一层插桩
app.MapGet("/orders/{orderId:int}", (int orderId) =>
    {
        var order = orderStore.FirstOrDefault(o => o.Id == orderId);
        return order is null
            ? Results.NotFound($"订单 {orderId} 不存在, 可查的编号是 1 到 3。")
            : Results.Text($"订单{order.Id}: {order.Description}");
    })
    .AddEndpointFilter<AuditTrailFilter>();

// 交付物二: 按类接异常——查无此单映射 404, 状态冲突映射 409;
// 未登记进映射表的类型原样重抛, 交给管道层兜底, 分工与第 13 章的保险丝口径一致
app.MapGet("/ex/missing", () =>
{
    var order = orderStore.FirstOrDefault(o => o.Id == 99);
    if (order is null)
    {
        throw new OrderNotFoundException(99);
    }
    return Results.Text($"订单{order.Id}: {order.Description}");
}).AddEndpointFilter<ExceptionToStatusFilter>();

app.MapGet("/ex/conflict", () =>
{
    var order = orderStore.First(o => o.Id == 1);
    if (order.Status == "已取消")
    {
        throw new OrderConflictException(order.Id, order.Status, "发货");
    }
    return Results.Text($"订单{order.Id}: 已发货");
}).AddEndpointFilter<ExceptionToStatusFilter>();

// 意外故障: IndexOutOfRangeException 不在过滤器的映射表里, 观察它走重抛路径还是被吞掉;
// 案情沿用第 11 章与第 13 章的越界案, 订单号被当数组下标用
app.MapGet("/ex/unexpected", () =>
{
    var order = orderStore[3];
    return Results.Text($"订单{order.Id}: {order.Description}");
}).AddEndpointFilter<ExceptionToStatusFilter>();

// 兜底: 没人认领的请求统一落到这里, 双轨口径与第 5 章一字不变
app.MapFallback(() => Results.NotFound("你访问的路径不存在, 先试试 /probe 或 /orders/1 。"));

// 启动监听——端口来自 appsettings.json 的 Kestrel 配置, 这份代码里没有端口字样;
// 预期结果: dotnet run 后监听 http://localhost:5800, 验证命令与实抓输出见清单14-4、清单14-5 与清单14-6
app.Run();

// 作用域服务: 第 8 章作用域身份证手法的复用, 供过滤器构造注入, 用 Id 对账
public class ScopeIdService
{
    public Guid Id { get; } = Guid.NewGuid();
}

// 语义实验一过滤器: 实例 Id 在构造函数里领取; 若实例被缓存复用, 两次请求会打出同一枚 Id
public class InstanceProbeFilter(ScopeIdService scopeIdService) : IEndpointFilter
{
    public Guid FilterId { get; } = Guid.NewGuid();

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        Console.WriteLine($"[probe] 过滤器实例 Id = {FilterId}, 注入的作用域服务 Id = {scopeIdService.Id}");
        var result = await next(context);
        Console.WriteLine($"[probe] next 返回, 过滤器实例 Id = {FilterId}");
        return result;
    }
}

// 语义实验二过滤器: 只打日志不做任何事, 次序结论全靠进出两行的配对
public class ChainOuterFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        Console.WriteLine("[chain] 外层过滤器进入");
        var result = await next(context);
        Console.WriteLine("[chain] 外层过滤器离开");
        return result;
    }
}

public class ChainInnerFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        Console.WriteLine("[chain] 内层过滤器进入");
        var result = await next(context);
        Console.WriteLine("[chain] 内层过滤器离开");
        return result;
    }
}

// 语义实验三过滤器: 不调 next 就是短路, 返回值直接成为整个响应;
// 方法体里没有任何 await, 就不必写成 async, 直接包一枚已完成的 ValueTask 回去
public class ShortCircuitFilter : IEndpointFilter
{
    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        Console.WriteLine("[short] 短路生效, next 未被调用");
        return ValueTask.FromResult<object?>(
            Results.Text("过滤器拦截: 演示端点不对外服务。", statusCode: StatusCodes.Status418ImATeapot));
    }
}

// 对照过滤器: 原样转交, 什么都不做, 证明短路差异出在「调没调 next」
public class PassthroughFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        return await next(context);
    }
}

// 操作审计过滤器: 进入记方法与路径, 离开记状态码;
// 有一个实测过的时机坑: next 返回时端点只是把 IResult 交了回来, 状态码要等结果执行时才写上响应,
// 就在 next 后面读 StatusCode 只能读到默认的 200, 查无此单回 404 也会被记成 200;
// 所以离开这条日志挂在响应即将发出第一个字节的回调里, 拿到的才是调用方真正收到的码
public class AuditTrailFilter(ILogger<AuditTrailFilter> logger) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var http = context.HttpContext;
        logger.LogInformation("审计进入: {Method} {Path}", http.Request.Method, http.Request.Path);
        var result = await next(context);
        http.Response.OnStarting(() =>
        {
            logger.LogInformation("审计离开: {Method} {Path} -> {StatusCode}",
                http.Request.Method, http.Request.Path, http.Response.StatusCode);
            return Task.CompletedTask;
        });
        return result;
    }
}

// 按类接异常的过滤器: try/catch 包住 next, 按异常类型映射状态码;
// 映射表外的类型记一笔后原样重抛, 管道层兜底地位不变, 过滤器只管进端点这一层
public class ExceptionToStatusFilter(ILogger<ExceptionToStatusFilter> logger) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        try
        {
            return await next(context);
        }
        catch (OrderNotFoundException ex)
        {
            logger.LogWarning("过滤器按类接管: {ExceptionType} 映射为 404", ex.GetType().Name);
            return Results.Text(ex.Message, statusCode: StatusCodes.Status404NotFound);
        }
        catch (OrderConflictException ex)
        {
            logger.LogWarning("过滤器按类接管: {ExceptionType} 映射为 409", ex.GetType().Name);
            return Results.Text(ex.Message, statusCode: StatusCodes.Status409Conflict);
        }
        catch (Exception ex)
        {
            // 未登记类型: 不吞单, 记一笔错误日志后原样上抛, 让管道层兜底照常工作
            logger.LogError(ex, "过滤器映射表未登记 {ExceptionType}, 原样重抛交管道层", ex.GetType().Name);
            throw;
        }
    }
}

// 自定义异常一: 查无此单, 契约上领 404; 异常类与消息文案沿用第 13 章口径
public class OrderNotFoundException(int orderId) : Exception($"订单 {orderId} 不存在, 可查的编号是 1 到 3。");

// 自定义异常二: 业务规则冲突, 契约上领 409
public class OrderConflictException(int orderId, string currentStatus, string attemptedAction)
    : Exception($"订单 {orderId} 当前状态是「{currentStatus}」, 无法执行「{attemptedAction}」。");
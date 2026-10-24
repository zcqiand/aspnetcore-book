// 文件位置: LifetimeLab/Services/ReportCacheService.cs, 与清单8-2 同目录同命名空间
// 剧情设定: 一个缓存服务想要「当前请求」的报告快照, 它该注册为 Singleton(全应用共享一份缓存),
// 而 Scoped 服务每个请求只有一份, 捕获依赖陷阱就出在这两句话的交界处
// ILogger 与 IServiceScopeFactory 都在 Web SDK 的隐式 using 名单里, 本文件无需额外引入
namespace LifetimeLab.Services;

// ===== 错误写法(仅对照病灶, 不要写入项目) =====
// Singleton 的构造函数直接声明 Scoped 依赖, 注册行写成:
//   builder.Services.AddSingleton<ReportCacheService>();
// 容器创建这个单例的那一刻就要解析 ScopedReportService, 而单例一生只创建一次,
// 这等于把某一个请求的 Scoped 实例永久焊死在单例身上, 之后所有请求都共用这个已作废的实例
public sealed class ReportCacheService
{
    // 病灶就在这个构造函数参数上: ScopedReportService 在清单8-3 已按 Scoped 注册,
    // 作用域校验器顺着注册表一查, 发现单例抱着作用域服务, 当场拦下
    public ReportCacheService(ScopedReportService reporter)
    {
        Snapshot = reporter.Describe();
    }

    public string Snapshot { get; }
}
// 作用域校验只认注册表上的真实身份: 若把构造参数声明成未按 Scoped 注册的类型
// (比如只按 Transient 注册的 TransientReportService), 单例抱 Transient 天经地义合法, 校验不触发, 应用照常启动
// Development 环境默认开启构建期校验, builder.Build() 当场抛异常——外层是 AggregateException 包着
// InvalidOperationException, 异常消息格式见清单8-7; 换到 Production 默认不校验,
// 启动不报错、单例静默焊死请求级实例, 反而更危险

// ===== 正解(要写就写这一版, 与错误写法二选一) =====
// 为什么注入 IServiceScopeFactory 而不是 ScopedReportService: IServiceScopeFactory 本身是单例,
// Singleton 持有它天经地义; 真要用 Scoped 服务时再手动开一个作用域, 开与关都攥在自己手里,
// 既过得了作用域校验, 也不把任何请求级实例焊死在应用级对象上
public sealed class ReportCacheService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReportCacheService> _logger;

    // 两个参数都是单例, Singleton 的构造函数注入它们完全合法
    public ReportCacheService(IServiceScopeFactory scopeFactory, ILogger<ReportCacheService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public string CaptureSnapshot()
    {
        try
        {
            // using 声明: 方法结束即使中途抛异常, scope 也保证被释放, 里面的 Scoped 实例随之作废
            using var scope = _scopeFactory.CreateScope();
            var reporter = scope.ServiceProvider.GetRequiredService<ScopedReportService>();
            return reporter.Describe();
        }
        catch (Exception error)
        {
            // 记录证据后原样上抛: 快照拿不到属于调用方必须知道的故障, 吞掉异常只会制造静默错误
            _logger.LogError(error, "ReportCacheService 生成请求快照失败");
            throw;
        }
    }
}
// 注册行加在清单8-3 的三条注册之后: builder.Services.AddSingleton<ReportCacheService>();
// ScopedReportService 已在清单8-3 按 Scoped 注册过, 无需重复注册, 手动作用域直接复用那一条;
// 想亲手复现清单8-7, 把正解的构造函数整体换回错误写法那一份即可
// 文件位置: LifetimeLab/Program.cs, 整体替换清单8-3 那一份(完整文件, 可直接覆盖后运行)
// 与第 8 章的关系: 三行生命周期注册按第 8 章定稿原样保留(三个实现各按具体类注册, Transient 用双参数位各解析一次);
// /report 端点骨架沿用第 8 章, 两处本章自有调整: 手动作用域改解析 TransientReportService(每解析必新实例),
// Describe 输出行序重排为 单例/作用域/暂时两次/单例回访/作用域回访, 与清单8-5 不再逐行对应, 不影响本章实验;
// 本章新增四样: Report 选项绑定与启动校验, /options 端点, Scrutor 批量装配导出器家族, /export 端点
// 端口口径与第 8 章一致: 代码不写死端口, 由清单9-5 的 dotnet run --urls=http://localhost:5400 指定

// 相比清单8-3 新增两个 using, 各管一件事:
// Microsoft.Extensions.Options 提供 IOptions/IOptionsSnapshot/IOptionsMonitor 与 OptionsValidationException,
//   三者都不在 Web SDK 的隐式 using 名单内, 缺了它 /options 端点的参数表编译不过;
// Scrutor 提供 Scan 等批量装配扩展方法, 本书不钉版本号, 以安装时的最新稳定版为准
using LifetimeLab.Services;
using Microsoft.Extensions.Options;
using Scrutor;

var builder = WebApplication.CreateBuilder(args);

// ===== 第 8 章既有内容 1: 三行生命周期注册, 按第 8 章定稿形态原样保留 =====
// 三行注册仍只差方法名, 三种命运: 三个实现各按具体类登记(不再同接口注册三次), 容器各按各的生命周期供给实例
builder.Services.AddSingleton<SingletonReportService>();
builder.Services.AddScoped<ScopedReportService>();
builder.Services.AddTransient<TransientReportService>();

// ===== 本章新增 1: Report 选项绑定与启动校验 =====
// AddOptions 返回 OptionsBuilder, Bind 把 Report 节挂上绑定器, 校验调用链在同一条 builder 上:
// ValidateDataAnnotations 执行清单9-2 里声明的全部特性([Required]/[Range]/[RealTitle]);
// Validate 补一条特性表达不了的跨字段规则; ValidateOnStart 把校验时点提前到启动那一刻
builder.Services.AddOptions<ReportOptions>()
    .Bind(builder.Configuration.GetSection("Report"))
    .ValidateDataAnnotations()
    .Validate(report => report.MaxRows >= report.RetryCount,
        "Report:MaxRows 不得小于 Report:RetryCount, 重试次数超过可导出行数上限没有意义。")
    .ValidateOnStart();
// 为什么 ValidateOnStart 是选项模式的兜底之一: 没有它, 错配置要等第一个请求碰 .Value 才炸, 炸在用户面前;
// 有了它, dotnet run 阶段就失败, 问题停在开发者自己的终端里, 排查半径最小

// ===== 本章新增 2: Scrutor 批量装配 IReportExporter 家族 =====
// 等价的手写版是下面三行, 每加一种导出格式就多一行, 漏写一行该格式就静默缺失:
//   builder.Services.AddTransient<IReportExporter, CsvReportExporter>();
//   builder.Services.AddTransient<IReportExporter, JsonReportExporter>();
//   builder.Services.AddTransient<IReportExporter, TextReportExporter>();
// 扫描装配把「增」的动作从注册表挪到「新写一个实现类」上: 实现写出来即注册, 这三行永远不用再碰
builder.Services.Scan(scan => scan
    .FromAssemblyOf<IReportExporter>()                               // 以接口所在程序集为扫描边界, 不越出本项目
    .AddClasses(classes => classes.AssignableTo<IReportExporter>())  // 圈定程序集里全部实现类
    .AsImplementedInterfaces()                                       // 按各自实现的接口登记
    .WithTransientLifetime());                                       // 无状态算力按第 8 章决策表选 Transient
// Scan 等扩展方法住在 Scrutor 命名空间, 顶部缺 using Scrutor 时这条语句编译不过

var app = builder.Build();

// ===== 第 8 章既有内容 2: /report 实验端点, 骨架沿用第 8 章(手动作用域与行序两处调整见文件头部说明) =====
// 注册按具体类登记后, 参数表也按具体类声明依赖; Transient 用两个参数位各解析一次,
// 同一请求里两行暂时的 ID 互不相同, 「每次索要都新建」从此有直接的实证
app.MapGet("/report", (SingletonReportService singletonReporter,
                       ScopedReportService scopedReporter,
                       TransientReportService transientReporter1,
                       TransientReportService transientReporter2,
                       IServiceScopeFactory scopeFactory) =>
{
    app.Logger.LogInformation("/report 请求进入, 开始生成生命周期报告");

    var lines = new List<string>();
    try
    {
        lines.Add(singletonReporter.Describe());
        lines.Add(scopedReporter.Describe());
        lines.Add(transientReporter1.Describe());
        lines.Add(transientReporter2.Describe());
        lines.Add(singletonReporter.Describe());
        lines.Add(scopedReporter.Describe());

        using var manualScope = scopeFactory.CreateScope();
        var firstInManualScope = manualScope.ServiceProvider.GetRequiredService<TransientReportService>();
        var secondInManualScope = manualScope.ServiceProvider.GetRequiredService<TransientReportService>();
        lines.Add($"手动作用域第 1 次解析: {firstInManualScope.Describe()}");
        lines.Add($"手动作用域第 2 次解析: {secondInManualScope.Describe()}");

        return Results.Text(string.Join(Environment.NewLine, lines));
    }
    catch (Exception error)
    {
        app.Logger.LogError(error, "生成生命周期报告失败");
        return Results.Text("生成生命周期报告失败, 请查看服务端日志。",
            statusCode: StatusCodes.Status500InternalServerError);
    }
});

// ===== 本章新增 3: /options 端点, 三个选项接口同台对照 =====
// 三个参数是同一份 ReportOptions 的三扇窗口, 差别只在「什么时候重新读配置」:
// IOptions 全程一份(单例视角), IOptionsSnapshot 每请求开头重读一次, IOptionsMonitor 现读现给且带变更推送
app.MapGet("/options", (IOptions<ReportOptions> optionsOnce,
                        IOptionsSnapshot<ReportOptions> optionsPerRequest,
                        IOptionsMonitor<ReportOptions> optionsLive) =>
{
    app.Logger.LogInformation("/options 请求进入, 输出三个接口各自读到的 Report 配置");

    try
    {
        var lines = new List<string>
        {
            $"IOptions          => {optionsOnce.Value}",
            $"IOptionsSnapshot  => {optionsPerRequest.Value}",
            $"IOptionsMonitor   => {optionsLive.CurrentValue}"
        };
        return Results.Text(string.Join(Environment.NewLine, lines));
    }
    catch (OptionsValidationException validationError)
    {
        // 第一道门(ValidateOnStart)拦启动, 这层拦运行期: 若读者删掉 ValidateOnStart 做对照实验,
        // 错配置会延迟到第一次 .Value 访问才抛 OptionsValidationException, 就由这里接住回 500
        app.Logger.LogError(validationError, "Report 选项未通过校验");
        return Results.Text($"Report 选项未通过校验: {string.Join("; ", validationError.Failures)}",
            statusCode: StatusCodes.Status500InternalServerError);
    }
    catch (Exception error)
    {
        // 兜底体例与 /report 一致: 记下异常证据, 回 500 与一句人话, 不把堆栈泼给调用方
        app.Logger.LogError(error, "/options 读取 Report 配置失败");
        return Results.Text("读取 Report 配置失败, 请查看服务端日志。",
            statusCode: StatusCodes.Status500InternalServerError);
    }
});

// ===== 本章新增 4: /export 端点, 消费整个导出器家族 =====
// 参数 IEnumerable<IReportExporter> 一次性拿到全部已注册导出器, 这是批量装配的消费端红利:
// 日后再加 XML 导出器, 本端点与注册行一行不改, 新格式自动进入分发
app.MapGet("/export", (IEnumerable<IReportExporter> exporters,
                       IOptionsSnapshot<ReportOptions> reportOptions,
                       SingletonReportService singletonReporter,
                       ScopedReportService scopedReporter,
                       TransientReportService transientReporter,
                       string? format) =>
{
    app.Logger.LogInformation("/export 请求进入, format={Format}", format);

    try
    {
        // 没带 format 参数时退回 Report:DefaultFormat 配置键, 清单9-2 里属性默认值在这里兑现
        var requestedFormat = string.IsNullOrWhiteSpace(format)
            ? reportOptions.Value.DefaultFormat
            : format.Trim().ToLowerInvariant();

        var exporter = exporters.FirstOrDefault(candidate => candidate.FormatName == requestedFormat);
        if (exporter is null)
        {
            // 未知格式是调用方的错, 归 4xx: 把支持格式一并告知, 让一次报错就够改对参数
            var supportedFormats = string.Join("/",
                exporters.Select(candidate => candidate.FormatName).OrderBy(name => name));
            app.Logger.LogWarning("未知导出格式 {RequestedFormat}, 当前支持 {SupportedFormats}",
                requestedFormat, supportedFormats);
            return Results.Text($"未知导出格式「{requestedFormat}」, 当前支持: {supportedFormats}。",
                statusCode: StatusCodes.Status400BadRequest);
        }

        // 报表行与 /report 的前六行同源: 三种生命周期各打一条 Describe
        var rows = new List<string>
        {
            singletonReporter.Describe(),
            scopedReporter.Describe(),
            transientReporter.Describe()
        };
        return Results.Text(exporter.Export(rows));
    }
    catch (Exception error)
    {
        // 导出器拼装失败(如序列化异常)时兜底, 体例仍与 /report 一致
        app.Logger.LogError(error, "/export 导出失败, format={Format}", format);
        return Results.Text("导出报表失败, 请查看服务端日志。",
            statusCode: StatusCodes.Status500InternalServerError);
    }
});

// 兜底: 没人认领的请求统一落到这里, 规矩与第 8 章相同
app.MapFallback(() => Results.NotFound("你访问的路径不存在, 先试试根路径 / 。"));

app.Run();
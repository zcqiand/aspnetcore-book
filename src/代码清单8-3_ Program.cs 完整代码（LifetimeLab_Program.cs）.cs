// 本章与第 5-7 章 FirstWebApp 的 Program.cs 有三处关系, 先交代清楚再读代码:
// 1. 这是 LifetimeLab 自己的 Program.cs, 由 dotnet new web 模板生成后整体替换,
//    FirstWebApp 那份一字不动; 两份五行骨架(WebApplication.CreateBuilder + Build + MapGet/MapFallback + Run)同源;
// 2. 本章新增的只有两样: 三条服务注册, 与一条 /report 实验端点;
// 3. 端口不在代码里写死: 清单8-4 用 dotnet run --urls=http://localhost:5400 从命令行指定监听地址,
//    命令行参数是 URL 配置中优先级最高的一档, 能压过 launchSettings.json 模板随机分配的端口,
//    本章所有清单的端口号因此固定为 5400, 不随机器漂移。

// 三个实现与其基类住在 Services 子目录(清单8-2), 命名空间是 LifetimeLab.Services,
// 缺了这一行, 端点参数表里的三个类型编译期就会报「未能找到类型或命名空间名」
using LifetimeLab.Services;

var builder = WebApplication.CreateBuilder(args);

// 三条注册只差方法名, 三种命运: 三个具体类型, 各按各的生命周期供给实例
// 为什么按具体类各自注册、而不是把三个实现都挂到同一个接口上:
// Minimal API 端点参数按参数类型逐个解析, 同一个服务类型注册多次时, 解析到的永远是最后一条注册——
// 三个参数若全声明成同一类型, 就会全部落到同一个实现上, 三种身份的对照实验整体作废;
// 只有各走各的类型, Singleton/Scoped/Transient 三种身份才能各解析到各的实现,
// ILifetimeReporter 在本章只当公共报告契约(统一 Describe 输出格式), 不参与注册与解析
builder.Services.AddSingleton<SingletonReportService>();
builder.Services.AddScoped<ScopedReportService>();
builder.Services.AddTransient<TransientReportService>();

var app = builder.Build();

// 实验端点: 依赖直接写在参数表里, 由容器按各自生命周期解析后递进来(第 6 章 GetRequiredService 的正名版:
// 那是「从宿主自带的服务里取出」, 这里是容器把服务送上门, 背后都是同一个 IoC 容器在工作)
// Transient 刻意放两个同类型参数: 端点参数每个请求只解析一次, 想看「解析两次」的效果,
// 就得给容器两次解析机会——两个参数位各解析一次, 才能得到两枚互不相同的实例
app.MapGet("/report", (SingletonReportService singletonReporter,
                       ScopedReportService scopedReporter,
                       TransientReportService transientReporter1,
                       TransientReportService transientReporter2,
                       IServiceScopeFactory scopeFactory) =>
{
    // 进入行日志: 每个请求都打一行, 用来在控制台把「一次请求」和「一段输出」对齐
    app.Logger.LogInformation("/report 请求进入, 开始生成生命周期报告");

    // Singleton/Scoped 是对同一实例连打两次 Describe(验证同实例两次调用返回同一 ID),
    // Transient 则是两个参数位各解析一次(真·解析两次, 两枚实例各打一行),
    // 三种生命周期的差异全藏在 8 行输出的 ID 异同里
    var lines = new List<string>();
    try
    {
        lines.Add(singletonReporter.Describe());
        lines.Add(singletonReporter.Describe());
        lines.Add(scopedReporter.Describe());
        lines.Add(scopedReporter.Describe());
        lines.Add(transientReporter1.Describe());
        lines.Add(transientReporter2.Describe());

        // 对照实验: 手动划一个新作用域, 证明「每个请求一个作用域」这条边界可以手动划,
        // 新作用域里解析 ScopedReportService 会得到与请求作用域不同的另一个实例;
        // using 声明保证 scope 无论中途是否抛异常都会被释放, 里面的 Scoped 实例随之作废, 不留悬空对象
        using var manualScope = scopeFactory.CreateScope();
        var firstInManualScope = manualScope.ServiceProvider.GetRequiredService<ScopedReportService>();
        var secondInManualScope = manualScope.ServiceProvider.GetRequiredService<ScopedReportService>();
        lines.Add($"手动作用域第 1 次解析: {firstInManualScope.Describe()}");
        lines.Add($"手动作用域第 2 次解析: {secondInManualScope.Describe()}");

        return Results.Text(string.Join(Environment.NewLine, lines));
    }
    catch (Exception error)
    {
        // 演示端点也要兜底: 记下异常证据后回 500 与一句人话, 体例与第 7 章异常兜底中间件一致
        app.Logger.LogError(error, "生成生命周期报告失败");
        return Results.Text("生成生命周期报告失败, 请查看服务端日志。",
            statusCode: StatusCodes.Status500InternalServerError);
    }
});

// 兜底: 没人认领的请求统一落到这里, 规矩与第 5 章相同
app.MapFallback(() => Results.NotFound("你访问的路径不存在, 先试试根路径 / 。"));

app.Run();
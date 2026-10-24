// 本章与第 6 章版本的 Program.cs 有三处差异, 先交代清楚再读代码:
// 1. 摘除第 6 章的三处检查点日志与停机钩子——启动顺序那一章已经验明, 本章的主角换成请求进来之后的处理链;
// 2. 新增三个 app.Use 中间件(日志、计时、异常兜底), 按注册顺序组成请求的去程通道;
// 3. 新增一条 /boom 演示端点, 专门抛异常, 用来亲眼看异常兜底中间件如何接住它。
// appsettings.json 不做任何改动, 第 6 章清单6-2 配置的 Kestrel 监听 http://localhost:5300 仍然生效。

// Stopwatch 住在 System.Diagnostics 命名空间, 不在 Web SDK 的隐式 using 名单里,
// 缺了这一行, 计时中间件那几行编译期就会报「当前上下文中不存在名称 Stopwatch」
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// 中间件一: 日志——await next() 之前是去程, 之后是回溯,
// 这一写法本身就是「进出各打一行」的对称结构: 进入行打在去程, 离开行打在回溯,
// 两行之间夹着的, 正是下游所有环节处理这一单的总时长
app.Use(async (context, next) =>
{
    app.Logger.LogInformation("日志中间件: 进入, {Method} {Path}", context.Request.Method, context.Request.Path);
    await next();
    app.Logger.LogInformation("日志中间件: 离开, 响应状态码 {StatusCode}", context.Response.StatusCode);
});

// 中间件二: 计时——停表与报数放进 finally 而不是顺手写在 await next() 之后,
// 用 finally 是为了让异常路径也计时: 下游就算抛了异常, 耗时这一行照样打印, 观测不因故障失明
app.Use(async (context, next) =>
{
    var stopwatch = Stopwatch.StartNew();
    try
    {
        await next();
    }
    finally
    {
        app.Logger.LogInformation("计时中间件: 本段管道耗时 {ElapsedMilliseconds} ms", stopwatch.ElapsedMilliseconds);
    }
});

// 中间件三: 异常兜底——catch 接住下游抛上来的异常, 记日志并改写响应,
// 接住之后不再上抛(不写 throw), 回溯通道对上游两个中间件保持「正常返回」,
// 它们的回溯半程照常执行, 不会因一个深层异常被拦腰斩断
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception error)
    {
        app.Logger.LogError(error, "异常兜底中间件: 管道内发生未处理异常, 已接管响应");
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsync("服务器内部错误, 请稍后重试。");
    }
});

// 登记根路径约定——应答文本与第 5 章一字不差
app.MapGet("/", () => "这是来自 FirstWebApp 的问候: 你的第一个 ASP.NET Core 应用正在运行。");

// 演示端点——故意抛异常: 异常从最内环出发, 沿回溯通道向外飞, 中间件三的 catch 正在出口处等着它。
// 注意这里必须写成块形式: throw 表达式作 lambda 体时没有类型, lambda 拿不到自然委托类型, MapGet 处编译不过
app.MapGet("/boom", () => { throw new InvalidOperationException("演示用异常: 订单数据越界了。"); });

// 兜底——没人认领的请求统一落到这里, 规矩与第 5 章相同
app.MapFallback(() => Results.NotFound("你访问的路径不存在, 先试试根路径 / 。"));

// 启动监听——注册顺序即去程顺序: 请求按日志、计时、异常兜底、端点的次序向内穿,
// 响应与异常按相反的次序向外回, 这份文件的全部意义, 都兑现在每一单请求的穿越与回溯里
app.Run();
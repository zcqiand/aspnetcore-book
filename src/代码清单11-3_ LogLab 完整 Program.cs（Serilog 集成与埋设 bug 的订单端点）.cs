// 文件位置: LogLab/Program.cs, 整体替换 dotnet new web 模板生成的那份(完整文件, 覆盖后即可运行)
// 与前几章的关系: 业务侧日志写法与第 10 章完全一致(照常用 app.Logger), 新增的只有
// 宿主桥接一处与请求日志中间件一行; 端口 5500 来自清单11-2 的 Kestrel 配置

using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 桥接注册: 启动时从 appsettings.json 的 Serilog 节读一遍级别与输出端,
// 此后 app.Logger 的每一条日志都汇入 Serilog 管道, 自动携带结构化字段
builder.Host.UseSerilog((context, loggerConfiguration) =>
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext());

var app = builder.Build();

// 请求日志中间件: 每个请求自动产出一条完成事件,
// 请求方法、路径、状态码、耗时全在完成事件里, 第 7 章手写的日志与计时两个中间件的活由它一条行包办
app.UseSerilogRequestLogging();

// 内存订单库: 3 笔订单, 订单号 1 到 3
// 数组下标从 0 起而订单号从 1 起, 这组错位是 11.5 节排案实验埋下的「案发现场」, 先照抄
var orderStore = new[]
{
    new { Id = 1, Description = "云海咖啡豆 x2" },
    new { Id = 2, Description = "阿里山茶行 x1" },
    new { Id = 3, Description = "青屿生鲜 x5" }
};

app.MapGet("/orders/{orderId}", (int orderId) =>
{
    // 进入行: OrderId 是命名字段, 值同时进文本与事件, 不用插值把值拼死在文本里
    app.Logger.LogInformation("收到订单查询: OrderId={OrderId}", orderId);

    try
    {
        // ===== 埋设的 bug: 拿订单号当数组下标用 =====
        // 合法下标是 0 到 2, 查 3 号订单时下标 3 越界, 炸出 IndexOutOfRangeException;
        // 查 1、2 号时不炸, 但取到的是隔壁订单, 属于没炸也错了的暗伤, 事后用 {@Order} 字段对账可发现
        var order = orderStore[orderId];

        // 成功行: {@Order} 解构占位符把对象拆成字段树, 能按 Id 字段核对是不是货不对板
        app.Logger.LogInformation("订单查询成功: {@Order}", order);

        return Results.Text($"订单{order.Id}: {order.Description}");
    }
    catch (Exception error)
    {
        // 错误行: 异常对象作第一个参数, 类型、消息与堆栈随事件全文入账,
        // OrderId 字段照带, 不翻代码就能对上「哪一笔请求出的错」; 回 500 与一句人话, 不把堆栈泼给调用方
        app.Logger.LogError(error, "订单查询失败: OrderId={OrderId}", orderId);
        return Results.Text("查询订单失败, 请稍后重试。",
            statusCode: StatusCodes.Status500InternalServerError);
    }
});

// 兜底: 没人认领的请求统一落到这里, 规矩与第 7/10 章相同
app.MapFallback(() => Results.NotFound("你访问的路径不存在, 先试试 /orders/1 。"));

app.Run();
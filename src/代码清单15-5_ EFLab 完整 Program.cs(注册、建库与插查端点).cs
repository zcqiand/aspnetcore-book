// 文件位置: EFLab/Program.cs, 整体替换 dotnet new web 模板生成的那份(完整文件, 覆盖后即可运行)
// 与前几章的关系: 端口不在代码里, 来自清单15-4 的 Kestrel 配置; 端点登记与兜底端点的写法沿用第 12 章口径
// 本章新增的家当三样: 一条 DbContext 注册、一段启动建库块、两个走数据库的端点
using EFLab;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// DbContext 按 Scoped 注册: 每个请求一个实例, 请求内共享、请求间隔离
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

// 建库: 数据库不存在时连库带表一起按实体类生成; 已存在则原样返回 false, 不动已有数据
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var created = db.Database.EnsureCreated();
    app.Logger.LogInformation("数据库就绪(本次启动新建: {Created})", created);
}

// 入口指引: 报出其余路径
app.MapGet("/", () => Results.Text("这是 EFLab 的入口, 可用的路径: /orders/add 、 /orders/1 。"));

// 插入端点: 新增一笔订单, 主键 Id 由数据库自增分配
app.MapGet("/orders/add", (AppDbContext db) =>
{
    try
    {
        var order = new Order
        {
            OrderNumber = $"ORD-{DateTime.Now:HHmmss}",
            Customer = "青屿生鲜",
            Amount = 2100m
        };
        db.Orders.Add(order);
        db.SaveChanges();
        return Results.Text($"已写入订单: Id={order.Id} {order.OrderNumber} {order.Customer} {order.Amount} 元。");
    }
    catch (Exception error)
    {
        app.Logger.LogError(error, "写入订单失败");
        return Results.Text("写入订单失败, 请查看服务端日志。",
            statusCode: StatusCodes.Status500InternalServerError);
    }
});

// 查询端点: 按主键查一笔订单; {orderId:int} 约束沿用第 12 章口径, 非整数进不了端点
app.MapGet("/orders/{orderId:int}", (int orderId, AppDbContext db) =>
{
    try
    {
        // Find 对键值做严格类型匹配: 主键是 long, 路由参数是 int, 不强转会当场抛异常
        var order = db.Orders.Find((long)orderId);
        return order is null
            ? Results.NotFound($"订单 {orderId} 不存在, 先访问 /orders/add 写入一笔。")
            : Results.Text($"订单{order.Id}: {order.OrderNumber} {order.Customer} {order.Amount} 元。");
    }
    catch (Exception error)
    {
        app.Logger.LogError(error, "查询订单失败");
        return Results.Text("查询订单失败, 请查看服务端日志。",
            statusCode: StatusCodes.Status500InternalServerError);
    }
});

// 兜底: 谁都没认领的请求统一落到这里
app.MapFallback(() => Results.NotFound("你访问的路径不存在, 先试试 /orders/add 。"));

app.Run();

// 预期结果: dotnet run 后监听 http://localhost:5900, 启动与请求的输出形态见清单15-6
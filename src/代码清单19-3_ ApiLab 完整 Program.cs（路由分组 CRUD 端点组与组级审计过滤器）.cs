using ApiLab;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// DbContext 按 Scoped 注册, 连接串来自配置(真机跑时经环境变量注入口令)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

// 路由分组: /orders 前缀只写一次, 组内端点共用; 组级过滤器一次挂载, 组内全部端点生效
var orders = app.MapGroup("/orders")
    .AddEndpointFilter<AuditTrailFilter>();

// 列表: 全部订单
orders.MapGet("/", (AppDbContext db) => Results.Ok(db.Orders.OrderBy(o => o.Id).ToList()));

// 详情: 按主键查一笔; {id:int} 约束沿用第 12 章口径, 查无回 404 人话
orders.MapGet("/{id:int}", (int id, AppDbContext db) =>
{
    var order = db.Orders.Find(id);
    return order is null
        ? Results.NotFound($"订单 {id} 不存在, 可先 POST /orders 新建一笔。")
        : Results.Ok(order);
});

// 新建: JSON 体进来一笔订单, 主键 Id 由数据库自增分配, 回 201 与 Location
orders.MapPost("/", (Order order, AppDbContext db) =>
{
    db.Orders.Add(order);
    db.SaveChanges();
    return Results.Created($"/orders/{order.Id}", order);
});

// 更新: 找得到才改, 查无回 404 人话
orders.MapPut("/{id:int}", (int id, Order input, AppDbContext db) =>
{
    var order = db.Orders.Find(id);
    if (order is null)
    {
        return Results.NotFound($"订单 {id} 不存在, 无从更新。");
    }

    order.Customer = input.Customer;
    order.Amount = input.Amount;
    order.Status = input.Status;
    db.SaveChanges();
    return Results.Ok(order);
});

// 删除: 找得到才删, 查无回 404 人话
orders.MapDelete("/{id:int}", (int id, AppDbContext db) =>
{
    var order = db.Orders.Find(id);
    if (order is null)
    {
        return Results.NotFound($"订单 {id} 不存在, 无从删除。");
    }

    db.Orders.Remove(order);
    db.SaveChanges();
    return Results.Ok(new { deleted = id });
});

// 兜底: 谁都没认领的路径统一落到这里, 404 人话(第 5 章双轨口径)
app.MapFallback(() => Results.NotFound("你访问的路径不存在, 订单相关操作从 /orders 进。"));

app.Run();

// 操作审计过滤器: 进入记方法与路径, 离开经 Response.OnStarting 记真码(时机坑与第 14 章同款:
// next 返回时状态码还没写上响应, 所以离开这条挂在响应即将发出第一个字节的回调里)
public class AuditTrailFilter(ILogger<AuditTrailFilter> logger) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var http = context.HttpContext;
        logger.LogInformation("审计进入: {Method} {Path}", http.Request.Method, http.Request.Path);
        http.Response.OnStarting(() =>
        {
            logger.LogInformation("审计离开: {Method} {Path} -> {StatusCode}",
                http.Request.Method, http.Request.Path, http.Response.StatusCode);
            return Task.CompletedTask;
        });
        return await next(context);
    }
}
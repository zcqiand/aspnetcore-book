// 文件位置: ErrorLab/Program.cs, 整体替换 dotnet new web 模板生成的那份(完整文件, 覆盖后即可运行)
// 与前几章的关系: 零第三方包, 契约服务与异常处理中间件都是框架内置;
// 端口 5700 来自清单13-2 的 Kestrel 配置; 全局异常处理只在 Production 挂载,
// Development 交给框架默认的开发者异常页, 两个环境各管一摊, 对照实验就建立在这道分界上

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// 契约服务登记: 错误响应的 JSON 正文与 Content-Type 都由它按问题详情契约统一产出
builder.Services.AddProblemDetails();

// 契约处理器登记: 按异常类型裁决状态码的实现写在文件末尾的 GlobalExceptionContractHandler 里;
// 后文不带参数的 UseExceptionHandler 重载依赖这两条登记: 处理器由 AddExceptionHandler 供应,
// 契约出单能力由 AddProblemDetails 供应; 其中 AddProblemDetails 少不得, 契约处理器的构造函数要注入契约服务, 只摘它启动时就会被框架点名(见 13.6 节练习 3)
builder.Services.AddExceptionHandler<GlobalExceptionContractHandler>();

var app = builder.Build();

// 环境分界: Production 才挂异常处理中间件, 异常被它接住后按表13-1 定状态码并产出契约响应;
// Development 分支刻意留空, 开发者异常页保持接管; 因登记了契约服务, 它的出单也走问题详情格式,
// 异常全文住进 exception 扩展字段, 实抓见清单13-5
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
}

// 入口: 报出三条演示路径, 让新来的请求有处可去
app.MapGet("/", () => Results.Text("这是 ErrorLab 的入口, 三条演示路径: /demo/unexpected 、 /demo/missing 、 /demo/conflict 。"));

// 内存订单库: 3 笔订单, 编号 1 到 3; 语境沿用第 11 章但整体重新登记, ErrorLab 是独立项目, 互不残留;
// 订单 1 特意置为已取消, 给 /demo/conflict 的冲突案备用
var orderStore = new[]
{
    new { Id = 1, Description = "云海咖啡豆 x2", Status = "已取消" },
    new { Id = 2, Description = "阿里山茶行 x1", Status = "已签收" },
    new { Id = 3, Description = "青屿生鲜 x5", Status = "配送中" }
};

// 演示一: 意外故障——案情延续第 11 章的越界案, 订单号被当数组下标用, 访问即炸 IndexOutOfRangeException,
// 契约上领 500: 客户端帮不上忙, 换任何编号重试都无济于事
app.MapGet("/demo/unexpected", () =>
{
    // 下标合法范围是 0 到 2, 字面 3 越界; 保留第 11 章原样的错位写法, 本章不修
    var order = orderStore[3];
    return Results.Text($"订单{order.Id}: {order.Description}");
});

// 演示二: 查无此单——先说清划界: 端点内判空早返回回 404 是首选(第 11 章练习 1 的裁定),
// 这里演示的是漏网之鱼的保险丝形态: 判空发现无货时不自己回 404, 而是抛出自定义异常,
// 把它映射成 404 的活交给管道层; 访问即查 99 号, 库里没有, 必抛 OrderNotFoundException
app.MapGet("/demo/missing", () =>
{
    var order = orderStore.FirstOrDefault(o => o.Id == 99);
    if (order is null)
    {
        // 保险丝形态: 状态码的裁决在管道层, 端点只负责把「查无此单」这个事实用异常说出来
        throw new OrderNotFoundException(99);
    }
    return Results.Text($"订单{order.Id}: {order.Description}");
});

// 演示三: 业务规则冲突——订单 1 已取消, 再对它发货属于状态矛盾, 契约上领 409:
// 客户端先核对订单状态, 修正操作再发, 与意外故障的「无事可做」划开
app.MapGet("/demo/conflict", () =>
{
    var order = orderStore.First(o => o.Id == 1);
    if (order.Status == "已取消")
    {
        throw new OrderConflictException(order.Id, order.Status, "发货");
    }
    return Results.Text($"订单{order.Id}: 已发货");
});

// 兜底: 没人认领的请求统一落到这里, 双轨口径与第 5 章一字不变, 本章不动它
app.MapFallback(() => Results.NotFound("你访问的路径不存在, 先试试 /demo/unexpected 。"));

// 启动监听——端口由清单13-2 的 Kestrel 配置决定, 这份代码里没有端口字样;
// 预期结果: dotnet run 后监听 http://localhost:5700, 三条演示路径待命,
// Development 与 Production 两组验证见清单13-4, 输出见清单13-5 与清单13-6
app.Run();

// 自定义异常一: 查无此单——语义上属于客户端能修正的错误, 契约上领 404;
// 订单号进属性, 日志与契约都能按字段取用
public class OrderNotFoundException : Exception
{
    public OrderNotFoundException(int orderId)
        : base($"订单 {orderId} 不存在, 可查的编号是 1 到 3。")
    {
        OrderId = orderId;
    }

    public int OrderId { get; }
}

// 自定义异常二: 业务规则冲突——订单当前状态与要执行的操作矛盾, 契约上领 409;
// 当前状态与想做的动作都进属性, 排障时不用翻代码就知道矛盾的双方是谁
public class OrderConflictException : Exception
{
    public OrderConflictException(int orderId, string currentStatus, string attemptedAction)
        : base($"订单 {orderId} 当前状态是「{currentStatus}」, 无法执行「{attemptedAction}」。")
    {
        OrderId = orderId;
        CurrentStatus = currentStatus;
        AttemptedAction = attemptedAction;
    }

    public int OrderId { get; }
    public string CurrentStatus { get; }
    public string AttemptedAction { get; }
}

// 全局异常契约处理器: Production 下异常处理中间件接住异常后, 裁决权交给这里;
// 实现的是框架内置的 IExceptionHandler 接口, 按异常类型定状态码, 正文交回契约服务写出
public class GlobalExceptionContractHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionContractHandler> _logger;
    private readonly IProblemDetailsService _problemDetailsService;

    // 两个依赖都由容器经构造函数注入, 第 2 章以来的老相识
    public GlobalExceptionContractHandler(ILogger<GlobalExceptionContractHandler> logger, IProblemDetailsService problemDetailsService)
    {
        _logger = logger;
        _problemDetailsService = problemDetailsService;
    }

    // 返回 true 表示异常已被本处理器接管, 不再向上飞; 签名由 IExceptionHandler 接口约定:
    // 返回 ValueTask<bool>, 参数表 (HttpContext, Exception, CancellationToken), 真机编译已过
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // 契约映射: 逐类型领状态码, 未登记的类型一律兜底 500;
        // 口径与表13-1 一致: 客户端能修正的给 4xx, 服务端意外留 500
        var (statusCode, title, detail) = exception switch
        {
            OrderNotFoundException => (StatusCodes.Status404NotFound, "查无此单", exception.Message),
            OrderConflictException => (StatusCodes.Status409Conflict, "订单状态冲突", exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "服务器内部错误", "服务器出了意外, 请稍后重试。")
        };

        // 异常被接住不等于没发生: 日志照记, 错误级行不缺席, 与第 11 章同一口径; 实际形态见清单13-6
        _logger.LogError(exception, "全局异常处理接管: {ExceptionType} 按错误契约映射为 {StatusCode}",
            exception.GetType().Name, statusCode);

        // 状态码写上响应, 正文交回契约服务: Content-Type 的 application/problem+json 由它定,
        // type 链接也由它按状态码补齐
        httpContext.Response.StatusCode = statusCode;
        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail
            }
        });
    }
}
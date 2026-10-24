// 【机制示意, 非运行代码】特性路由(attribute routing)的标注形态长这样:
// 路径不再集中登记在一处, 而是拆成特性贴在各自的类与方法上, 模板与实现长在同一处。
// 刻意说明为何在 RouteLab 里只能示意: 这套标注要经 AddControllers() 与 MapControllers()
// 两行注册进管道才会应答请求, RouteLab 没有这两行注册——
// 这段代码即使原样粘进项目且编译通过, /orders/2 也会落空给 404, 因为没有端点被登记,
// 这正是「标注在案」与「登记生效」两回事的直接证据。
// 形态对照: 类级 [Route("orders")] 管前缀, 动作级 [HttpGet(...)] 管剩余段, 两级拼接成完整路径;
// {orderId:int} 约束与 RouteLab 模板同款写法, 约束词汇是两套风格共享的

[Route("orders")]
public class OrdersController : ControllerBase
{
    // GET /orders -> 列表, 与 RouteLab 路由表第 2 行同一条路径
    [HttpGet]
    public IActionResult ListOrders()
    {
        return Ok("订单列表(示意文案)。");
    }

    // GET /orders/{orderId:int} -> 详情, 与 RouteLab 路由表第 3 行同一条路径
    [HttpGet("{orderId:int}")]
    public IActionResult GetOrder(int orderId)
    {
        return Ok($"订单 {orderId} 详情(示意文案)。");
    }
}
// 注册前提(示意注释, 勿粘进 RouteLab):
//   builder.Services.AddControllers(); 与 app.MapControllers();
// 预期结果: 本清单为机制示意, 在 RouteLab 中不产生任何可命中的端点, 不参与清单12-5 的验证
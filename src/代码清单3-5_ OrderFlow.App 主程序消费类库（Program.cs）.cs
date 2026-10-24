using OrderFlow.Domain;

// App 只负责构造示范数据与输出结果, 不做任何业务校验:
// 校验职责已收拢在类库边界（清单3-4）, 两处重复校验只会在日后改规则时失同步
var pricer = new OrderPricer();

// 合法订单: 单价 128.00 元 × 数量 26, 延续第 2 章订单主题的字段口径
Order paidOrder = new(1001, "阿里山茶行", 128.00m, 26);

// 两条故意非法的订单, 各自命中类库抛出的一种异常:
// 负单价命中 ArgumentOutOfRangeException, 空白客户名命中 ArgumentException
Order negativeAmountOrder = new(1002, "云海咖啡豆", -640.00m, 3);
Order blankCustomerOrder = new(1003, "   ", 210.00m, 2);

int exitCode = 0;

foreach (Order order in new[] { paidOrder, negativeAmountOrder, blankCustomerOrder })
{
    try
    {
        decimal total = pricer.CalculateTotal(order);
        Console.WriteLine($"订单 ORD-{order.Id} 合计金额: {total:F2} 元");
    }
    catch (ArgumentOutOfRangeException rangeError)
    {
        // 这个类型派生自 ArgumentException, catch 顺序必须放在前面,
        // 否则非法取值会被更宽的 ArgumentException 分支抢先吞掉, 错误提示张冠李戴
        Console.WriteLine($"金额或数量非法: {rangeError.Message}");
        exitCode = 1;
    }
    catch (ArgumentException argumentError)
    {
        Console.WriteLine($"订单数据非法: {argumentError.Message}");
        exitCode = 1;
    }
}

// 退出码区分 0 与 1: 控制台程序自己不说哪里失败,
// 脚本与持续集成只能靠退出码判断本次运行是否健康, 这是第 1 章自检小工具口径的延续
return exitCode;
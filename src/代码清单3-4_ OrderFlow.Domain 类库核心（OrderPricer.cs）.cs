namespace OrderFlow.Domain;

// 延续第 2 章的订单主题, 但这是新项目里的新 Order:
// 字段按计价场景裁剪, 比第 2 章的版本少了 OrderNumber 与 Status, 多了计价必需的 Quantity
public record Order(long Id, string Customer, decimal Amount, int Quantity);

public sealed class OrderPricer
{
    // 校验全部挡在类库边界而非散落在调用方:
    // 非法数据在入口处就被抛出, 能走到计算这一行的订单一定合法, 调用方拿到的一定是可信结果
    public decimal CalculateTotal(Order order)
    {
        if (order is null)
        {
            // null 是编程错误而非业务输入错误, 用 ArgumentNullException 单独点名参数为 null 的情形
            throw new ArgumentNullException(nameof(order), "订单不能为 null。");
        }

        if (string.IsNullOrWhiteSpace(order.Customer))
        {
            // 用 IsNullOrWhiteSpace 而非只判空串: 纯空格的客户名同样没有业务含义, 一并挡下
            throw new ArgumentException("订单客户名不能为空白。", nameof(order));
        }

        if (order.Amount < 0)
        {
            // 金额为负是取值超出合理范围, 归 ArgumentOutOfRangeException; 错误消息带上订单号便于定位
            throw new ArgumentOutOfRangeException(nameof(order),
                $"订单 {order.Id} 的金额不能为负数, 实际为 {order.Amount}。");
        }

        if (order.Quantity <= 0)
        {
            // 数量为零与为负都算非法: 零件订单乘出来恒为零, 对计价毫无意义, 一并拒绝
            throw new ArgumentOutOfRangeException(nameof(order),
                $"订单 {order.Id} 的数量必须大于零, 实际为 {order.Quantity}。");
        }

        // 用 decimal 承载金额而不是 double: decimal 是十进制浮点, 不会产生 0.1 加不精确那类二进制误差, 金额场景零容忍
        return order.Amount * order.Quantity;
    }
}
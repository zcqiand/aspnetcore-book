// 与代码清单2-1（Java）、2-2（Python）完全同构：同一份订单数据、同一套筛选-排序-汇总。
// LINQ 方法链自上而下读正是数据流动的顺序，转轨者按 Java Stream / Python 推导式的经验逐段对读即可

long threshold = 500;

List<Order> orders = new List<Order>
{
    new Order("ORD-1001", "阿里山茶行", 1280, OrderStatus.Paid),
    new Order("ORD-1002", "云海咖啡豆", 640, OrderStatus.Paid),
    new Order("ORD-1003", "沧海书屋", -150, OrderStatus.Paid),      // 脏数据: 负数金额, 会被阈值挡下
    new Order("ORD-1004", "拾光文具", 890, OrderStatus.Cancelled),  // 脏数据: 已取消, 不算有效订单
    new Order("ORD-1005", "青屿生鲜", 2100, OrderStatus.Paid),
    new Order("ORD-1006", "山间粮铺", 450, OrderStatus.Paid)        // 金额未过阈值, 同样被挡下
};

// Where 筛 -> OrderByDescending 排 -> Take 截 -> ToList 落地：
// 与 Java 的 filter/sorted/limit、Python 的推导式/sorted/切片一一对应
List<Order> top3 = orders
    .Where(order => order.Status == OrderStatus.Paid)
    .Where(order => order.Amount > threshold)
    .OrderByDescending(order => order.Amount)
    .Take(3)
    .ToList();

// Sum 接受委托直接取字段求和，对应 Java 的 mapToLong(...).sum()、Python 的 sum(生成器)
long totalAmount = top3.Sum(order => order.Amount);

Console.WriteLine($"== Top 3 已支付大额订单（金额超过 {threshold} 元）==");
for (int i = 0; i < top3.Count; i++)
{
    Console.WriteLine($"{i + 1}. {top3[i].OrderNumber} {top3[i].Customer} {top3[i].Amount} 元");
}
Console.WriteLine($"Top 3 订单总金额: {totalAmount} 元");

// 顶级语句文件的类型声明统一放在语句之后：record 一行顶 Java/Python 的多行样板，
// 构造器、相等比较、格式化输出等成员全部由编译器代劳
public record Order(string OrderNumber, string Customer, long Amount, OrderStatus Status);

public enum OrderStatus
{
    Paid,
    Cancelled
}
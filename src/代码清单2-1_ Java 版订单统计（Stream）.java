import java.util.Comparator;
import java.util.List;

public class OrderDemo {

    enum OrderStatus { PAID, CANCELLED }

    // record 等价于 C# 的 record、Python 的 @dataclass：
    // 一行声明只读字段，构造器、equals、hashCode、toString 全部由编译器代劳
    record Order(String orderNumber, String customer, long amount, OrderStatus status) {}

    public static void main(String[] args) {
        List<Order> orders = List.of(
            new Order("ORD-1001", "阿里山茶行", 1280, OrderStatus.PAID),
            new Order("ORD-1002", "云海咖啡豆", 640, OrderStatus.PAID),
            new Order("ORD-1003", "沧海书屋", -150, OrderStatus.PAID),      // 脏数据: 负数金额, 会被阈值挡下
            new Order("ORD-1004", "拾光文具", 890, OrderStatus.CANCELLED),  // 脏数据: 已取消, 不算有效订单
            new Order("ORD-1005", "青屿生鲜", 2100, OrderStatus.PAID),
            new Order("ORD-1006", "山间粮铺", 450, OrderStatus.PAID)        // 金额未过阈值, 同样被挡下
        );

        long threshold = 500;

        // Stream 链路对应 C# 的 LINQ 方法链、Python 的推导式加 sorted：
        // filter 筛选 -> sorted 倒序 -> limit 截断，三段与另外两份清单逐段对得上
        List<Order> top3 = orders.stream()
            .filter(order -> order.status() == OrderStatus.PAID)
            .filter(order -> order.amount() > threshold)
            .sorted(Comparator.comparingLong(Order::amount).reversed())
            .limit(3)
            .toList();

        // mapToLong 显式取出 amount 再求和，对应 C# 的 Sum(o => o.Amount)、Python 的 sum(生成器)
        long totalAmount = top3.stream().mapToLong(Order::amount).sum();

        System.out.println("== Top 3 已支付大额订单（金额超过 " + threshold + " 元）==");
        for (int i = 0; i < top3.size(); i++) {
            Order order = top3.get(i);
            System.out.println((i + 1) + ". " + order.orderNumber() + " "
                + order.customer() + " " + order.amount() + " 元");
        }
        System.out.println("Top 3 订单总金额: " + totalAmount + " 元");
    }
}
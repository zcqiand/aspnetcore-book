# 与代码清单2-1（Java）、2-4（C#）同构：同一份订单数据、同一套筛选-排序-汇总流程
from dataclasses import dataclass

# 用模块级常量模拟 Java/C# 的枚举：生产代码可换 enum.Enum，
# 这里求与另外两份清单同构对读，不引入额外语法负担
PAID = "PAID"
CANCELLED = "CANCELLED"


# @dataclass 自动生成 __init__/__repr__/__eq__，等价于 Java 的 record、C# 的 record
@dataclass
class Order:
    order_number: str
    customer: str
    amount: int
    status: str


def main() -> None:
    orders = [
        Order("ORD-1001", "阿里山茶行", 1280, PAID),
        Order("ORD-1002", "云海咖啡豆", 640, PAID),
        Order("ORD-1003", "沧海书屋", -150, PAID),      # 脏数据: 负数金额, 会被阈值挡下
        Order("ORD-1004", "拾光文具", 890, CANCELLED),  # 脏数据: 已取消, 不算有效订单
        Order("ORD-1005", "青屿生鲜", 2100, PAID),
        Order("ORD-1006", "山间粮铺", 450, PAID),        # 金额未过阈值, 同样被挡下
    ]

    threshold = 500

    # 列表推导式把「状态有效 + 金额过阈值」两道筛选压进一个表达式，
    # 对应 Java 的两次 filter、C# 的两次 Where
    eligible_orders = [
        order for order in orders
        if order.status == PAID and order.amount > threshold
    ]

    # sorted 加 reverse=True 对应 Java 的 sorted(comparingLong(...).reversed())；
    # 切片 [:3] 对应 C# 的 Take(3)，切片越界安全，候选不足 3 条时也不会报错
    top3 = sorted(eligible_orders, key=lambda order: order.amount, reverse=True)[:3]

    # 生成器表达式直接喂给 sum：不落地中间列表，订单量大时比先建列表再求和更省内存
    total_amount = sum(order.amount for order in top3)

    print(f"== Top 3 已支付大额订单（金额超过 {threshold} 元）==")
    for index, order in enumerate(top3, start=1):
        print(f"{index}. {order.order_number} {order.customer} {order.amount} 元")
    print(f"Top 3 订单总金额: {total_amount} 元")


if __name__ == "__main__":
    main()
// Program.cs —— 与代码清单2-5 的 OrderRepository.cs 同项目；顶级语句入口从第一行开始，
// 两个 static 辅助函数按惯例沉到文件末尾，主流程一眼读完

var orderRepository = new InMemoryOrderRepository();
List<Order> seedOrders = SeedOrders();

// 逐条走 Add 入库而不是构造时一次性塞字典：
// 让所有写入都经过唯一入口，主键冲突才可能被统一拦截
foreach (Order order in seedOrders)
{
    orderRepository.Add(order);
}

// 业务查询复用代码清单2-4 的同一套 LINQ 逻辑，数据源换成仓储返回的快照，
// 演示「查询逻辑与数据存放位置解耦」：将来数据搬进数据库，这段链路原样保留
long threshold = 500;
List<Order> top3 = orderRepository.All()
    .Where(order => order.Status == OrderStatus.Paid)
    .Where(order => order.Amount > threshold)
    .OrderByDescending(order => order.Amount)
    .Take(3)
    .ToList();

long totalAmount = top3.Sum(order => order.Amount);

Console.WriteLine($"== Top 3 已支付大额订单（金额超过 {threshold} 元）==");
for (int i = 0; i < top3.Count; i++)
{
    Console.WriteLine($"{i + 1}. {top3[i].OrderNumber} {top3[i].Customer} {top3[i].Amount} 元");
}
Console.WriteLine($"Top 3 订单总金额: {totalAmount} 元");

// 故意把第一条订单再入库一次触发主键冲突：第一类 catch 分态。
// 这类冲突属于程序逻辑错误，报错退出比静默吞掉安全，退出码置 1 便于脚本感知
int exitCode = 0;
try
{
    orderRepository.Add(seedOrders[0]);
}
catch (InvalidOperationException conflictException)
{
    Console.WriteLine($"操作被拒绝: {conflictException.Message}");
    exitCode = 1;
}

// 命令行传入订单 Id 时做按主键查询；不传参数则到此结束，保证无参数运行也有完整输出
if (args.Length > 0)
{
    try
    {
        long requestedId = long.Parse(args[0]);
        // FindCustomerName 返回 string?：类型层面就声明「可能拿不到客户名」，
        // 等价于 Java 方法签名里的 Optional<String>，编译器逼着调用方先判空再使用
        string? customerName = FindCustomerName(orderRepository, requestedId);
        if (customerName is null)
        {
            // 查无此单走判空路径而非异常：查询落空是正常业务结果，不算程序出错
            Console.WriteLine($"未找到 Id 为 {requestedId} 的订单, 请核对后重试。");
        }
        else
        {
            Console.WriteLine($"查询结果: Id 为 {requestedId} 的订单客户是 {customerName}。");
        }
    }
    catch (FormatException)
    {
        // 第二类 catch 分态：输入不是整数，属可预期的用户输入错误，与逻辑错误分开处理便于定位
        Console.WriteLine($"输入错误: \"{args[0]}\" 不是有效的订单 Id, 应为整数。");
        exitCode = 1;
    }
}

return exitCode;

static List<Order> SeedOrders() => new List<Order>
{
    new Order(1, "ORD-1001", "阿里山茶行", 1280, OrderStatus.Paid),
    new Order(2, "ORD-1002", "云海咖啡豆", 640, OrderStatus.Paid),
    new Order(3, "ORD-1003", "沧海书屋", -150, OrderStatus.Paid),      // 脏数据: 负数金额
    new Order(4, "ORD-1004", "拾光文具", 890, OrderStatus.Cancelled),  // 脏数据: 已取消
    new Order(5, "ORD-1005", "青屿生鲜", 2100, OrderStatus.Paid),
    new Order(6, "ORD-1006", "山间粮铺", 450, OrderStatus.Paid)        // 金额未过阈值
};

static string? FindCustomerName(IRepository<Order> repository, long orderId)
{
    // GetById 的返回类型是 Order?（可空引用），查无此单时得到 null 而非异常
    Order? order = repository.GetById(orderId);
    return order?.Customer;
}
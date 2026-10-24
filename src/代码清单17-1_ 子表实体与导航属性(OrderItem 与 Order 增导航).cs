// 文件位置: EFLab/OrderItem.cs, 手动新建
// 子表实体: 一个订单有多条明细, 类对表、属性对列的约定与主表实体完全同款;
// OrderId 这个名字不是随便起的: EF 按约定拿「导航名 + Id」认外键列,
// 有它在, 明细和订单的关联不需要写任何建表语句, 表结构照旧归第 16 章的迁移管
public class OrderItem
{
    public long Id { get; set; }
    public string Product { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    // 外键列: 明细挂在哪笔订单下
    public long OrderId { get; set; }

    // 引用导航: 从明细走到它的订单; 声明成 virtual 是给懒加载留的门(见清单17-9),
    // 不开懒加载时这扇门没有代价, 引用不加载时它的值是 null
    public virtual Order? Order { get; set; }
}

// 文件位置: EFLab/Order.cs, 在第 15 章的四属性上追加一条集合导航
public class Order
{
    public long Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Customer { get; set; } = string.Empty;
    public decimal Amount { get; set; }

    // 集合导航: 从订单走到它的全部明细; 同样 virtual, 理由同上;
    // 初始化成空集合是有意的: 不加载时读它不报错, 只是空的
    public virtual List<OrderItem> Items { get; set; } = new();
}

// 文件位置: EFLab/AppDbContext.cs, DbSet 家当多一行, 一张新表一个属性
public DbSet<OrderItem> OrderItems => Set<OrderItem>();
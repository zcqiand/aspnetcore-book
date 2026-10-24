// OrderRepository.cs —— 与代码清单2-6 的 Program.cs 同属 OrderMiniApp 项目，两份文件合起来才可运行

// 实体约定接口：想被仓储管理的实体必须暴露只读主键 Id。
// 有了这层约定，下面的仓储接口只需一个泛型参数就能覆盖所有实体，
// 这正是 Spring Data JPA 的 JpaRepository<T, ID> 在教学尺度上的最小对应物
public interface IIdentified
{
    long Id { get; }
}

// 泛型接口把「仓储能做什么」与「仓储里放的是什么」解耦：
// 将来换成 EF Core 读写数据库时只换实现类，调用方代码一行不改
public interface IRepository<TEntity> where TEntity : IIdentified
{
    void Add(TEntity entity);
    TEntity? GetById(long id);
    IReadOnlyList<TEntity> All();
}

// record 实现接口：位置参数 Id 直接满足 IIdentified 的只读主键约定，
// 注意本实体比代码清单2-4 的版本多一个自增主键 Id，业务单号 OrderNumber 单独一个字段
public record Order(
    long Id,
    string OrderNumber,
    string Customer,
    long Amount,
    OrderStatus Status) : IIdentified;

public enum OrderStatus
{
    Paid,
    Cancelled
}

public sealed class InMemoryOrderRepository : IRepository<Order>
{
    // Dictionary 的主键到实体映射提供均摊 O(1) 的按主键查找，
    // 等价于 Java 的 HashMap、Python 的 dict——三种语言在容器选型上殊途同归
    private readonly Dictionary<long, Order> _ordersById = new();

    public void Add(Order order)
    {
        // 主键重复几乎必然是上游缺陷：宁可当场抛异常把问题暴露在发生地，
        // 也不让脏数据悄悄覆盖既有订单，这是把校验收拢到唯一写入入口的好处
        if (_ordersById.ContainsKey(order.Id))
        {
            throw new InvalidOperationException($"订单 Id {order.Id} 已存在, 不允许重复添加。");
        }
        _ordersById[order.Id] = order;
    }

    // TryGetValue 一次查找同时完成「取值 + 存在性判断」；查不到时返回 null 而非抛异常，
    // 把「可能没有」写进返回类型 Order?，等价于 Java 里返回 Optional<Order>
    public Order? GetById(long id)
    {
        return _ordersById.TryGetValue(id, out Order? order) ? order : null;
    }

    // 返回快照副本而非字典内部视图：调用方对结果再怎么增删，都影响不到仓储内部状态
    public IReadOnlyList<Order> All()
    {
        return _ordersById.Values.ToList();
    }
}
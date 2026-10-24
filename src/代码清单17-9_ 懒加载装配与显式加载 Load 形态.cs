// (1) 懒加载装配: 两件套, 缺一不可
// 装包: dotnet add package Microsoft.EntityFrameworkCore.Proxies --version "8.0.*"
// 注册处多挂一行 UseLazyLoadingProxies, 其余不动:
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
           .UseLazyLoadingProxies());
// 前提: 全部导航属性声明 virtual(清单17-1 已埋) —— 代理靠派生类重写属性挂钩,
// 漏一个 virtual, 访问到那个导航时当场报 InvalidOperationException, 不是静默失灵

// (2) 显式加载: 零包依赖, 不需要代理, 对已查入内存的实体手动补载
// 集合导航版: Entry 配 Collection 配 Load, 一发只补这一笔订单的明细
app.MapGet("/orders/load/{orderId:int}", (int orderId, AppDbContext db) =>
{
    try
    {
        var order = db.Orders.Find((long)orderId);
        if (order is null)
        {
            return Results.NotFound($"订单 {orderId} 不存在, 先访问 /orders/add 写入一笔。");
        }
        // Load 之前: 导航是空集合(0 条), 库里有明细但它们还没进内存, 也没有任何查询发出
        var before = order.Items.Count;
        db.Entry(order).Collection(o => o.Items).Load(); // 此刻才补发那一条 SELECT
        var after = order.Items.Count;
        return Results.Text($"显式加载: Load 之前明细导航 {before} 条, Load 之后 {after} 条。");
    }
    catch (Exception error)
    {
        app.Logger.LogError(error, "显式加载实验失败");
        return Results.Text("显式加载实验失败, 请查看服务端日志。",
            statusCode: StatusCodes.Status500InternalServerError);
    }
});

// 引用导航版: Reference 配 Load; 引用不加载时是 null(对照集合导航的空集合),
// 这个 null 正是「忘了加载就 NullReferenceException」的出处
app.MapGet("/orders/loadref/{itemId:int}", (int itemId, AppDbContext db) =>
{
    try
    {
        var item = db.OrderItems.Find((long)itemId);
        if (item is null)
        {
            return Results.NotFound($"明细 {itemId} 不存在, 先访问 /orders/items/seed/3 备料。");
        }
        var before = item.Order is null ? "null" : item.Order.OrderNumber; // 此刻必是 null
        db.Entry(item).Reference(i => i.Order).Load();                     // 手动补载
        var after = item.Order is null ? "null" : item.Order.OrderNumber;
        return Results.Text($"显式加载引用: Load 之前导航是 {before} , Load 之后是 {after} 。");
    }
    catch (Exception error)
    {
        app.Logger.LogError(error, "显式引用加载实验失败");
        return Results.Text("显式引用加载实验失败, 请查看服务端日志。",
            statusCode: StatusCodes.Status500InternalServerError);
    }
});

// 真机实抓返回原文两行:
// 显式加载: Load 之前明细导航 0 条, Load 之后 3 条。
// 显式加载引用: Load 之前导航是 null , Load 之后是 ORD-201331-01 。
// 显式加载发出的 SQL 与懒加载逐单查询逐字同款, 分界只在谁触发: 懒加载是访问属性自动触发,
// 显式加载是代码手动调 Load 入口 —— 决策表的分界行就在这两个字: 自动, 还是手动。
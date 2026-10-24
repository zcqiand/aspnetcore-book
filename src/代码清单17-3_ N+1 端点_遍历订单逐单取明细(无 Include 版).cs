// 文件位置: EFLab/Program.cs, 端点登记沿用清单15-5 的体例, 逐条 MapGet
// 两个端点都不带 Include: 查询行为由装配开关决定(清单17-9), 这正是本章要对比的

// 端点一: 默认行为观察哨 —— 主查询之后直接访问导航, 看拿到什么
app.MapGet("/orders/nav/{orderId:int}", (int orderId, AppDbContext db) =>
{
    try
    {
        var order = db.Orders.Find((long)orderId);
        // 空集处置: 查无此单时导航根本不存在, 先把 404 递出去
        if (order is null)
        {
            return Results.NotFound($"订单 {orderId} 不存在, 先访问 /orders/add 写入一笔。");
        }
        // 不带 Include 也不开懒加载时, 这一行拿到的是空集合: 不报错, 但明细是 0 条;
        // 代码照常往下走, 只是明细没跟着来; 不翻日志, 谁也看不出这一单少了货
        var itemCount = order.Items.Count;
        return Results.Text($"订单{order.Id} 主查询完成; 不带 Include 直接看明细导航, 拿到 {itemCount} 条。");
    }
    catch (Exception error)
    {
        app.Logger.LogError(error, "导航访问失败");
        return Results.Text("导航访问失败, 请查看服务端日志。",
            statusCode: StatusCodes.Status500InternalServerError);
    }
});

// 端点二: N+1 复现 —— 主查询拿全部订单, foreach 里逐单访问明细导航
app.MapGet("/orders/lazy", (AppDbContext db) =>
{
    try
    {
        var watch = Stopwatch.StartNew();
        var orders = db.Orders.OrderBy(o => o.Id).ToList();
        var lines = new List<string>();
        int total = 0;
        foreach (var order in orders)
        {
            var items = order.Items; // 懒加载开着时, 每次走到这里都可能补发一条查询
            total += items.Count;
            lines.Add($"订单{order.Id}({order.Customer}) 明细 {items.Count} 条");
        }
        watch.Stop();
        return Results.Text($"遍历 {orders.Count} 笔订单取明细, 明细合计 {total} 条, 耗时 {watch.ElapsedMilliseconds} 毫秒。\n"
            + string.Join("\n", lines));
    }
    catch (Exception error)
    {
        app.Logger.LogError(error, "N+1 实验失败");
        return Results.Text("N+1 实验失败, 请查看服务端日志。",
            statusCode: StatusCodes.Status500InternalServerError);
    }
});
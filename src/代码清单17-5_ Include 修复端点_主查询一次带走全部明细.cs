// 文件位置: EFLab/Program.cs
// 修复思路: 主查询时一次把明细全带走, 循环仍只读 Items.Count, 循环里没有 SQL 可发
// Include(o => o.Items) 就是「主查询顺路把 Items 一起查回来」的入口
app.MapGet("/orders/eager", (AppDbContext db) =>
{
    try
    {
        var watch = Stopwatch.StartNew();
        // 只比清单17-3 的遍历端点多一个 Include, 读明细的方式不变, 手上的数据已经齐了
        var orders = db.Orders.OrderBy(o => o.Id).Include(o => o.Items).ToList();
        int total = 0;
        foreach (var order in orders)
        {
            total += order.Items.Count; // 明细已在内存里, 这一行不再触发任何查询
        }
        watch.Stop();
        return Results.Text($"Include 预加载 {orders.Count} 笔订单, 明细合计 {total} 条, 耗时 {watch.ElapsedMilliseconds} 毫秒。");
    }
    catch (Exception error)
    {
        app.Logger.LogError(error, "Include 实验失败");
        return Results.Text("Include 实验失败, 请查看服务端日志。",
            statusCode: StatusCodes.Status500InternalServerError);
    }
});
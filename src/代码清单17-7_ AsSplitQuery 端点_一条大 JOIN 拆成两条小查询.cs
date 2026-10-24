// 文件位置: EFLab/Program.cs
// 拆分的动机: 单条大 JOIN 会把行数乘起来(10 笔订单 × 3 条明细 = 30 行主表列全部重复),
// 关联再多几层, 这份重复是平方级长法; AsSplitQuery 换成主表一条、明细一条, 行数不乘
app.MapGet("/orders/split", (AppDbContext db) =>
{
    try
    {
        var watch = Stopwatch.StartNew();
        // Include 照旧, 后面多挂一个 AsSplitQuery, 查询从 1 条变 2 条, 组装归 EF
        var orders = db.Orders.OrderBy(o => o.Id).Include(o => o.Items).AsSplitQuery().ToList();
        int total = orders.Sum(o => o.Items.Count);
        watch.Stop();
        return Results.Text($"AsSplitQuery 拆分加载 {orders.Count} 笔订单, 明细合计 {total} 条, 耗时 {watch.ElapsedMilliseconds} 毫秒。");
    }
    catch (Exception error)
    {
        app.Logger.LogError(error, "拆分查询实验失败");
        return Results.Text("拆分查询实验失败, 请查看服务端日志。",
            statusCode: StatusCodes.Status500InternalServerError);
    }
});
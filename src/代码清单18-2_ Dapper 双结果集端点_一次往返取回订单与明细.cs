// 文件位置: EFLab/Program.cs, 端点登记沿用清单15-5 的体例, 逐条 MapGet
// 文件头新增两行 using: Dapper 提供执行入口, Npgsql 提供连接
using Dapper;
using Npgsql;
// 计时用的 Stopwatch 住在 System.Diagnostics 命名空间, 第 7 章交代过的那行显式 using 第 17 章起就在文件头, 此处沿用不重出

// Dapper 侧连接串: 与 EF 同一份配置口径, 同一个库, 现开现用, 用完即还连接池
var dapperConnectionString = builder.Configuration.GetConnectionString("Default");

// 双结果集端点: 打开一条连接, QueryMultiple 一次往返发两条 SELECT,
// 内存里按 OrderId 组装成 List<Order>(每单挂上 Items), 计时对照 Include
app.MapGet("/orders/dapper/all", () =>
{
    try
    {
        var watch = Stopwatch.StartNew();
        using var connection = new NpgsqlConnection(dapperConnectionString);
        connection.Open();
        using var multi = connection.QueryMultiple(@"
SELECT ""Id"", ""OrderNumber"", ""Customer"", ""Amount"" FROM ""Orders"" ORDER BY ""Id"";
SELECT ""Id"", ""OrderId"", ""Product"", ""Quantity"", ""UnitPrice"" FROM ""OrderItems"" ORDER BY ""Id"";");
        var orders = multi.Read<Order>().ToList();
        var items = multi.Read<OrderItem>().ToList();
        foreach (var group in items.GroupBy(i => i.OrderId))
        {
            orders.First(o => o.Id == group.Key).Items = group.ToList();
        }
        watch.Stop();
        var lines = orders.Select(o => $"订单{o.Id}({o.Customer}) 明细 {o.Items.Count} 条");
        return Results.Text($"Dapper 双结果集读取 {orders.Count} 笔订单, 明细合计 {items.Count} 条, 耗时 {watch.ElapsedMilliseconds} 毫秒。\n"
            + string.Join("\n", lines));
    }
    catch (Exception error)
    {
        app.Logger.LogError(error, "Dapper 多结果集实验失败");
        return Results.Text("Dapper 读取失败, 请查看服务端日志。",
            statusCode: StatusCodes.Status500InternalServerError);
    }
});
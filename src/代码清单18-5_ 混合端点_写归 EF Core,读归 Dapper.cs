// 文件位置: EFLab/Program.cs
// 混合持久化的代码实物: 同一端点内, 写与读各走各的通道, 落在同一座库上
app.MapGet("/orders/hybrid", (AppDbContext db) =>
{
    try
    {
        var watch = Stopwatch.StartNew();
        var order = new Order
        {
            OrderNumber = $"HYB-{DateTime.Now:HHmmssfff}",
            Customer = "混合直采",
            Amount = 168m
        };
        order.Items.Add(new OrderItem { Product = "赣南脐橙", Quantity = 2, UnitPrice = 39.9m });
        order.Items.Add(new OrderItem { Product = "智利车厘子", Quantity = 1, UnitPrice = 129m });
        db.Orders.Add(order);
        db.SaveChanges(); // 变更追踪替你排好写入顺序: 先插订单拿回 Id, 再插明细填外键
        var writeMs = watch.ElapsedMilliseconds;

        // 写完换 Dapper 读回: 两条手写 SQL, 全部参数化
        using var connection = new NpgsqlConnection(dapperConnectionString);
        var readBack = connection.QueryFirstOrDefault<Order>(
            @"SELECT ""Id"", ""OrderNumber"", ""Customer"", ""Amount""
              FROM ""Orders"" WHERE ""OrderNumber"" = @Number",
            new { Number = order.OrderNumber });
        var readItems = readBack is null
            ? new List<OrderItem>()
            : connection.Query<OrderItem>(
            @"SELECT ""Id"", ""OrderId"", ""Product"", ""Quantity"", ""UnitPrice""
              FROM ""OrderItems"" WHERE ""OrderId"" = @Id ORDER BY ""Id""",
            new { Id = readBack.Id }).ToList();
        watch.Stop();
        var itemText = string.Join("、", readItems.Select(i => $"{i.Product}x{i.Quantity}({i.UnitPrice} 元)"));
        if (readBack is null)
        {
            return Results.Text("混合持久化: EF 写入成功, 但 Dapper 没读回这笔订单, 检查两条通道是否指向同一个库。",
                statusCode: StatusCodes.Status500InternalServerError);
        }
        return Results.Text($"混合持久化: EF 写入订单 {readBack.OrderNumber}({readBack.Customer}, {readBack.Amount} 元, 写入耗时 {writeMs} 毫秒), "
            + $"Dapper 读回明细 {readItems.Count} 条: {itemText}, 全程耗时 {watch.ElapsedMilliseconds} 毫秒。");
    }
    catch (Exception error)
    {
        app.Logger.LogError(error, "混合持久化实验失败");
        return Results.Text("混合持久化实验失败, 请查看服务端日志。",
            statusCode: StatusCodes.Status500InternalServerError);
    }
});

// 真机实抓返回原文两发:
// 混合持久化: EF 写入订单 HYB-213546665(混合直采, 168 元, 写入耗时 501 毫秒), Dapper 读回明细 2 条: 赣南脐橙x2(39.9 元)、智利车厘子x1(129 元), 全程耗时 662 毫秒。
// 混合持久化: EF 写入订单 HYB-213558602(混合直采, 168 元, 写入耗时 675 毫秒), Dapper 读回明细 2 条: 赣南脐橙x2(39.9 元)、智利车厘子x1(129 元), 全程耗时 908 毫秒。
// 写入侧日志实抓: SaveChanges 拆成 1 条订单 INSERT 与逐条明细 INSERT, 均带 RETURNING 取回自增 Id:
// INSERT INTO "Orders" ("Amount", "Customer", "OrderNumber") VALUES (@p0, @p1, @p2) RETURNING "Id";
// INSERT INTO "OrderItems" ("OrderId", "Product", "Quantity", "UnitPrice") VALUES (@p0, @p1, @p2, @p3) RETURNING "Id";
// 先拿订单 Id 才填明细外键, 这个顺序是 EF Core 变更追踪替你排的
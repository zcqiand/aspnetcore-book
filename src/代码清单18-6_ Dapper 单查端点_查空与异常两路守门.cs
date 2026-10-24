// 文件位置: EFLab/Program.cs
// 单查端点: 按订单号参数化查询, 查空与连不上两路形态都在这个端点上拍
app.MapGet("/orders/dapper/find/{number}", (string number) =>
{
    try
    {
        using var connection = new NpgsqlConnection(dapperConnectionString);
        var order = connection.QueryFirstOrDefault<Order>(
            @"SELECT ""Id"", ""OrderNumber"", ""Customer"", ""Amount""
              FROM ""Orders"" WHERE ""OrderNumber"" = @Number",
            new { Number = number });
        return order is null
            ? Results.NotFound($"订单号 {number} 在表里不存在, 先访问 /orders/hybrid 写一笔。")
            : Results.Text($"订单{order.Id}: {order.OrderNumber} {order.Customer} {order.Amount} 元。");
    }
    catch (Exception error)
    {
        app.Logger.LogError(error, "Dapper 单查失败");
        return Results.Text("Dapper 单查失败, 请查看服务端日志。",
            statusCode: StatusCodes.Status500InternalServerError);
    }
});
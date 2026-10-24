// 文件位置: EFLab/Program.cs, 端点登记沿用清单15-5 的体例, 逐条 MapGet, 不设分组
app.MapGet("/orders/conflict/{orderId:int}", (int orderId, AppDbContext db,
    IServiceScopeFactory scopeFactory) =>
{
    try
    {
        // 第一步: 甲做一次追踪读, 上下文把此刻的行值记成原始值
        var mine = db.Orders.Find(orderId);
        if (mine is null)
        {
            return Results.NotFound($"订单 {orderId} 不存在, 先访问 /orders/add 写入一笔。");
        }

        // 第二步: 乙经 IServiceScopeFactory 划新作用域, 拿第二个上下文先写同一行;
        // 乙的值带时间戳, 保证每发都不同, 同值写入 EF 判「属性未变」不发 UPDATE
        string theirsValue;
        using (var scope = scopeFactory.CreateScope())
        {
            var otherDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var theirs = otherDb.Orders.Find(orderId);
            theirs.Customer = $"乙-长乐杂货-{DateTime.Now:HHmmss.fff}";
            otherDb.SaveChanges();
            theirsValue = theirs.Customer;
        }

        // 第三步: 甲改自己的值后写, 无令牌时这一笔静默覆盖乙
        mine.Customer = $"甲-青屿生鲜-{DateTime.Now:HHmmss.fff}";
        db.SaveChanges();

        return Results.Text($"甲后写完成, 无异常。乙改成了 {theirsValue}, 现在库里是 {mine.Customer} 。");
    }
    catch (Exception error)
    {
        app.Logger.LogError(error, "并发实验失败");
        return Results.Text("并发实验失败, 请查看服务端日志。",
            statusCode: StatusCodes.Status500InternalServerError);
    }
});
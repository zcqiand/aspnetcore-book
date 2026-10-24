// (1) xmin 挂法三件套
// Order.cs 新增一行:
public uint Version { get; set; }

// AppDbContext.cs 的 OnModelCreating 里加一行映射:
// xmin 系统列挂为并发令牌: uint 属性 + IsRowVersion, 提供程序识别后挂到隐藏系统列
modelBuilder.Entity<Order>().Property(order => order.Version).IsRowVersion();
// 生成的迁移文件里虽有 AddColumn xmin 一项, 但提供程序认得 xmin 是系统列,
// 实际执行段一条 DDL 都不发, 账照记, 表不动

// (2) 修复路一: 客户端获胜, catch 之后先对齐令牌, 再套回甲的值重试
catch (DbUpdateConcurrencyException error)
{
    var entry = error.Entries.Single();
    entry.Reload();  // 把库里的现值连令牌一起刷进来, 甲的原始令牌与库对齐
    mine.Customer = $"甲-坚持覆盖-{DateTime.Now:HHmmss.fff}";
    db.SaveChanges();
}

// (3) 修复路二: 库获胜, Reload 之后不套值不保存, 拿库里现值给人话提示
// return Results.Text($"冲突后听数据库的: 甲的修改作废, 这条订单别人刚改成 {mine.Customer}, 您看到的页面已刷新。");
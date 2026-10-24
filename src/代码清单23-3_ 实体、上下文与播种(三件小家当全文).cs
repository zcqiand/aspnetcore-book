// 文件位置: CacheLab/Order.cs, 手动新建
// 回源慢源表的实体: 与隔离验证库的 orders 表一一对应(EnsureCreated 建表, 零迁移)
public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = "";
    public decimal Amount { get; set; }
}

// 文件位置: CacheLab/AppDb.cs, 手动新建
using Microsoft.EntityFrameworkCore;
// 数据面上下文: 表结构手写显式映射, 不依赖约定推断; 只读实验, 零迁移文件
public class AppDb(DbContextOptions<AppDb> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(o => o.Id);
            entity.Property(o => o.CustomerName).HasColumnName("customer_name").IsRequired();
            entity.Property(o => o.Amount).HasColumnName("amount");
            entity.Property(o => o.Id).HasColumnName("id").UseIdentityAlwaysColumn();
        });
    }
}

// 文件位置: CacheLab/DbSeed.cs, 手动新建
using Microsoft.EntityFrameworkCore;
// 验证期播种: EnsureCreated 直接建隔离库与表(验证毕整库删除), 空表时补 5 行订单
public static class DbSeed
{
    public static async Task EnsureSeededAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDb>();
        await db.Database.EnsureCreatedAsync();
        if (!await db.Orders.AnyAsync())
        {
            db.Orders.AddRange(Enumerable.Range(1, 5).Select(i =>
                new Order { CustomerName = $"客户-{i}", Amount = 100m * i }));
            await db.SaveChangesAsync();
        }
    }
}
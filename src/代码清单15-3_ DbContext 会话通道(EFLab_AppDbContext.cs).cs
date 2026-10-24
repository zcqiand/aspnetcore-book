using Microsoft.EntityFrameworkCore;

namespace EFLab;

// DbContext 是程序与数据库之间的会话通道: 其中的 DbSet 属性各对应一张表,
// 对 DbSet 的增查动作由 EF Core 翻译成 SQL 发给数据库
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
}
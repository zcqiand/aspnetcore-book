dotnet new web -o ApiLab
cd ApiLab
# 数据访问两包即第 16 章起随迁移工具链装齐的那一对: Npgsql 的 EF Core 供应包与迁移设计包,
# 属既有依赖在新仓的复装, 零新依赖; 命令不钉版本号, 装到的是 EF Core 8.x 一线的最新稳定版
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Design
# 接下来按第 15 章体例手动新建两个类文件: Order.cs 与 AppDbContext.cs,
# 再生成迁移并建库(见清单19-2); 启动命令统一用 dotnet run --urls=http://localhost:6000
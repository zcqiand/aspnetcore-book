dotnet new web -n CacheLab -f net8.0
cd CacheLab
# EF 数据访问两包: 既有依赖复装; 坎: 不钉版本的命令已装不上,
#     解析到远超 net8.0 的大版本, 当场报 NU1202, 与第 15 章装 EF 包那次同族;
#     解法同款: 显式钉带, 钉带不钉死补丁号
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL -v "8.*"
dotnet add package Microsoft.EntityFrameworkCore.Design -v "8.*"
# Redis 供应包: IDistributedCache 的实现包; 命令不钉版本, 真机解析顺利,
#     该包多目标兼容 net8.0, 解析结果记档不入正文
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
# 启动命令统一用 dotnet run --urls=http://localhost:6200
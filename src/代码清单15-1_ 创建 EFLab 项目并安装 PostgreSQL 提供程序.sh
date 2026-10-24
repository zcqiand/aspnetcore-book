dotnet new web -o EFLab
cd EFLab
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version "8.0.*"

# EFLab 是与前面各章并列的独立实验项目, 不在任何旧项目上追加一行代码, 各仓互不污染;
# 这是第 4 章装包之后本书的第一个 EF 包, 装包口径仍是第 3 章立下的 add package 记账命令
# 显式钉 "8.0.*" 的原因: 实测不带版本号执行时, 工具会解析到高于 8.x 的大版本,
# 与项目的 net8.0 目标框架不兼容, 当场报 NU1202 错误, 包根本写不进 csproj;
# 钉在 8.0.* 后解析到 EF Core 8.x 的兼容版, 与本书的 EF Core 8.x 口径一致
# 装完 csproj 多出一行 PackageReference 条目, 形态与第 4 章的装包条目同款
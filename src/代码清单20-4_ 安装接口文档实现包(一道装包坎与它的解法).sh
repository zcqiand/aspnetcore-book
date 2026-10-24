dotnet add package Swashbuckle.AspNetCore
# 坎: 不钉版本号时按当下最新稳定版解析, 装进 csproj 的版本落在 10.x 一线,
#     远超本书钉定的兼容带; 编译运行不报错, 不对照就发现不了
dotnet remove package Swashbuckle.AspNetCore
# 解法: 搬回第 15 章清单15-1 的钉带体例, 钉带不钉死补丁号, 解析落回带内
dotnet add package Swashbuckle.AspNetCore --version "6.6.*"
# 装完 csproj 多出一行 PackageReference, Version="6.6.*" 与第 15 章 8.0.* 那行同款
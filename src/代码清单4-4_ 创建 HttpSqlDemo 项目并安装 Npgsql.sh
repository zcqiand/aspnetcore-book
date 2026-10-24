dotnet new console -o HttpSqlDemo
cd HttpSqlDemo
dotnet add package Npgsql

# dotnet new console 与第 1 章清单1-2 创建第一个项目是同一条命令, 此处不再展开
# add package 即第 3 章 3.2 节立的装包口径, 执行后 csproj 会多出一行 Npgsql 条目
# 安装时不追加版本号, 由工具解析当前兼容稳定版, 与本书的版本口径管理一致
dotnet add package Dapper

# EFLab 第四连: 仍是第 15 章那一仓、5900 端口、eflab 库, 实体、迁移与既有端点零改动;
# 本章唯一的装包动作, 命令不带 --version: EF 系钉 "8.0.*" 是为对齐运行时大版本,
# Dapper 是独立版本线的包, 无此顾虑, 交由工具解析最新稳定版, 解析结果随工程记档;
# 装完 csproj 多出一行 Dapper 的 PackageReference 条目, 形态与第 15 章的装包条目同款
dotnet new web -o LogLab
cd LogLab
dotnet add package Serilog.AspNetCore
# 版本口径: 本书不钉 Serilog 系包的版本号, dotnet add package 不带 --version 时引入当前最新稳定版,
# 案例仓回填时按 version-lock 的 pending 口径记录实测版本
# Serilog.AspNetCore 是伞包: Serilog 本体、控制台输出端、配置读取支持都随依赖一并进来,
# 本章用到的 Serilog 能力全在这一个包里, 不再单独引包
# LogLab 是与 FirstWebApp(第 5-7 章)、LifetimeLab(第 8-9 章)、ConfigLab(第 10 章) 并列的独立实验项目,
# 互不污染; 本章所有代码与配置都以 LogLab 目录为根
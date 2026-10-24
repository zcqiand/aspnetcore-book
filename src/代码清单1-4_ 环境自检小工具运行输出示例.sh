PS D:\Code\FirstDotNetApp> dotnet run -- 100 4
== .NET 环境自检 ==
CLR 版本  : 8.0.0
操作系统  : Microsoft Windows 10.0.22631
目标框架  : .NETCoreApp,Version=v8.0
100 ÷ 4 = 25

PS D:\Code\FirstDotNetApp> dotnet run -- 100 abc
== .NET 环境自检 ==
CLR 版本  : 8.0.0
操作系统  : Microsoft Windows 10.0.22631
目标框架  : .NETCoreApp,Version=v8.0
输入错误: "100" 或 "abc" 不是有效整数。

PS D:\Code\FirstDotNetApp> dotnet run -- 100 0
== .NET 环境自检 ==
CLR 版本  : 8.0.0
操作系统  : Microsoft Windows 10.0.22631
目标框架  : .NETCoreApp,Version=v8.0
计算错误: 除数 0 为零, 除法无法进行。
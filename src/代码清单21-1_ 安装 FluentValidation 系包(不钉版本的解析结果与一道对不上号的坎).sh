dotnet add package FluentValidation
dotnet add package FluentValidation.AspNetCore
dotnet add package FluentValidation.DependencyInjectionExtensions
# 坎: FluentValidation 的版本线独立成系, 不随 .NET 对带,
#     序列里的 8.x 一带是多年前的旧线, 钉上去就远离当下主线,
#     第 15 章 8.0.* 与第 20 章 6.6.* 的钉带体例在此对不上号;
#     不钉版本即按当下最新稳定版解析, 真机解析、编译、运行三关全过
# 注: 自动验证包 FluentValidation.AspNetCore 在官方弃用名单上, 升级维护已停,
#     弃用说的是不再背书, 装不上跑不动倒也未必; 采不采, 交给下面的三轮探测
dotnet new sln -n OrderFlow
dotnet new classlib -o OrderFlow.Domain
dotnet new console -o OrderFlow.App
dotnet new xunit -o OrderFlow.Tests
dotnet sln add OrderFlow.Domain OrderFlow.App OrderFlow.Tests

# -o 指定项目落在同名子目录, 三个项目平铺在解决方案文件旁, 互不覆盖
# dotnet sln add 一条命令可同时挂载多个项目, 免去逐条添加的重复劳动
# xunit 模板自带测试 SDK 与测试框架引用, 测试项目无需额外安装任何包
# classlib 与 console 模板生成的 Class1.cs/Program.cs 稍后逐一替换, 结构见下一份清单
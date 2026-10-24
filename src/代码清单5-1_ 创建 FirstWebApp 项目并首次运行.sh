dotnet new web -o FirstWebApp
cd FirstWebApp
dotnet run

# web 是 .NET 8 自带的空 Web 项目模板, 与第 1 章创建控制台项目的 new console 同一条命令家族:
# 模板引擎换一份料而已, 命令用法一字不差, -o 指定项目落在同名子目录
# dotnet run 首次运行先编译再启动, 这与第 1 章相同; 不同的是 Web 程序启动后不会返回命令行,
# 而是停在原地等请求上门, 想停止它按 Ctrl+C
# 启动成功的标志是控制台出现「Now listening on: http://localhost:端口」一行,
# 端口号由模板在生成时分配, 以你机器上这一行显示的为准
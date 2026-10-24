# 第 1 步: 在 LifetimeLab 目录内引入 Scrutor
dotnet add package Scrutor
# 命令不带 --version: 本书不钉版本号, 以安装时的最新稳定版为准
# 安装完成只是让清单9-4 顶部的 using Scrutor 编译得过, 全章代码不出现版本号

# 第 2 步: 启动应用, 端口口径与第 8 章一致(命令行 --urls 优先级最高, 压过 launchSettings.json)
dotnet run --urls=http://localhost:5400
# 观察点 1: 启动即跑 ValidateOnStart, Report 配置不合法时应用在这里就失败退出,
#           异常消息指向具体失败的规则, 根本轮不到下面的请求发出
# 观察点 2: 控制台出现 Now listening on: http://localhost:5400 才算就绪

# 第 3 步: 另开一个终端, 先打改配置前的第一次 /options
curl http://localhost:5400/options
# 观察点: 三行输出的四个字段完全相同, 此刻三接口同源, 记下 MaxRows 的旧值

# 第 4 步: 保持应用运行不要重启, 用编辑器把 appsettings.json 里 Report 节的
#          MaxRows 从 100 改成 200 并保存(ASP.NET Core 默认开启配置热重载, 保存即生效),
#          随后立刻打第二次 /options
curl http://localhost:5400/options
# 观察点: IOptions 一行仍是旧值 100, IOptionsSnapshot 与 IOptionsMonitor 两行已变成 200

# 第 5 步: 连发三次 /export, 三种结果各验一个
curl "http://localhost:5400/export?format=csv"
# 观察点: 正文首行是表头 index,content, 之后 3 行数据(Windows PowerShell 里带 ? 的 URL 要加引号)
curl "http://localhost:5400/export?format=json"
# 观察点: 正文是缩进的 JSON 数组, 行内容与 csv 语义相同、显示形态不同(中文显示为 \uXXXX 转义序列)
curl "http://localhost:5400/export?format=xml"
# 观察点: 400 响应, 正文是错误提示加支持格式清单, 不含任何报表数据
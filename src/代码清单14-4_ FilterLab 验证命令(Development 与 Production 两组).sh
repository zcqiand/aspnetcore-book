# 口径沿用第 5 章的 -i; Windows PowerShell 5.1 的 curl 是 Invoke-WebRequest 别名、不认 -i,
# 请在 CMD, PowerShell 7 或 Git Bash 里执行, 第 5 章的提醒原样有效

# ===== 第一组: Development =====
# dotnet new web 模板的 launchSettings.json 默认登记 Development, 直接启动即入此环境
dotnet run
# 启动成功后另开一个终端; 核对凭据: 启动行 Hosting environment 必须显示 Development
# 语义实验: /probe 打两次对比 Id, /chain 与 /short 看终端日志与响应
curl -i http://localhost:5800/probe
curl -i http://localhost:5800/probe
curl -i http://localhost:5800/chain
curl -i http://localhost:5800/short
curl -i http://localhost:5800/noshort
# 审计与按类接异常: 404 案给审计的对账用
curl -i http://localhost:5800/orders/2
curl -i http://localhost:5800/orders/99

# ===== 第二组: Production =====
# 实测有效的切法: 环境变量 + 不用启动配置文件的组合, PowerShell 写法:
$env:ASPNETCORE_ENVIRONMENT = "Production"
dotnet run --no-launch-profile
# Git Bash 可一行替代: ASPNETCORE_ENVIRONMENT=Production dotnet run --no-launch-profile
# 核对凭据: 启动行 Hosting environment 显示 Production, 监听行仍为清单14-2 的 5800 端口;
# 加了 --no-launch-profile, 模板登记的随机端口不再生效, Overriding address(es) warn 随之消失
# 异常三发 + 审计一发, 预期 404, 409 过滤器映射, /ex/unexpected 走重抛回 500
curl -i http://localhost:5800/ex/missing
curl -i http://localhost:5800/ex/conflict
curl -i http://localhost:5800/ex/unexpected
curl -i http://localhost:5800/orders/2
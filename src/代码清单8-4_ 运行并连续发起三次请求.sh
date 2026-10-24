dotnet run --urls=http://localhost:5400
# --urls 从命令行指定监听地址, 与清单8-3 顶部注释的口径一致:
# 命令行参数在 URL 配置里优先级最高, 压过 launchSettings.json 里模板随机分配的端口, 本章端口固定为 5400
# 启动成功后另开一个终端, 连打三次请求, 每段输出观察两处:
# 1. Scoped 行的 ID 是否换了新的(每个请求一个实例);
# 2. 同一次响应里 Transient 两行(来自两个参数位)的 ID 是否互不相同(两个参数位各解析一次, 每次解析都新建)
curl http://localhost:5400/report
curl http://localhost:5400/report
curl http://localhost:5400/report
# 预期: 三段输出的完整格式见清单8-5, Singleton 三次请求全程同一个 ID,
# Scoped 每请求换新且同请求内两次打印相同, Transient 同请求内两行(两枚 ID)互不相同
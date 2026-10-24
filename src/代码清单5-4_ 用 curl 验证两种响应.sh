dotnet run
# 启动成功后另开一个终端, 依 4.2 节的 curl 口径验证; -i 让响应状态行与头部一起显示
# 端口 5247 是示例, 换成你机器「Now listening on」一行显示的端口
curl -i http://localhost:5247/
curl -i http://localhost:5247/no-such-path
# 第一条命中清单5-3 登记的根路径约定, 得 200;
# 第二条没有任何地址认领, 落进 MapFallback 兜底, 得 404
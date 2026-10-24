dotnet run
# 启动成功后另开一个终端执行下面的命令; 端口 5600 来自清单12-3 的 Kestrel 配置

# 路由表第 1 行: 入口指引
curl -i http://localhost:5600/

# 路由表第 2 行: 订单列表
curl -i http://localhost:5600/orders

# 路由表第 3 行: 订单详情, 有货、无货、非整数, 一行三态
curl -i http://localhost:5600/orders/2
curl -i http://localhost:5600/orders/99
curl -i http://localhost:5600/orders/abc

# 路由表第 4 行: 进行中的订单
curl -i http://localhost:5600/orders/active

# 路由表第 5 行: 分页查询, 缺省与显式各打一发
curl -i http://localhost:5600/orders/page
curl -i http://localhost:5600/orders/page/2

# 兜底(路由表之外): 谁都没认领的路径
curl -i http://localhost:5600/whatever

# 预期: 六个请求得 200; /orders/99 、 /orders/abc 、 /whatever 得 404,
# 三种 404 状态码一致, 人话只分两路: 第一发的出自详情端点, 后两发同出兜底,
# 这正是辨认 404 出处的线索, 形态见清单12-7
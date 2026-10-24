dotnet run --urls=http://localhost:6000
# 启动成功后另开一个终端执行; 四个体文件均存为 UTF-8:
# order1.json 与 order2.json 是两笔新建订单, update1.json 是更新内容,
# bad-order.json 是截断残体, 内容只有半截: {"customer":
curl -i -X POST -H "Content-Type: application/json" --data-binary @order1.json http://localhost:6000/orders
curl -i -X POST -H "Content-Type: application/json" --data-binary @order2.json http://localhost:6000/orders
curl -i http://localhost:6000/orders
curl -i http://localhost:6000/orders/1
curl -i http://localhost:6000/orders/99
curl -i -X PUT -H "Content-Type: application/json" --data-binary @update1.json http://localhost:6000/orders/1
curl -i -X PUT -H "Content-Type: application/json" --data-binary @update1.json http://localhost:6000/orders/99
curl -i -X DELETE http://localhost:6000/orders/2
curl -i -X DELETE http://localhost:6000/orders/99
curl -i -X POST -H "Content-Type: application/json" --data-binary @bad-order.json http://localhost:6000/orders
# 路由表之外: 谁都没认领的路径
curl -i http://localhost:6000/whatever
# 预期: 两发 POST 得 201, Location 分别为 /orders/1 与 /orders/2; 列表与详情得 200;
# /orders/99 三发与 /whatever 得 404, 人话各归各的出处; 非法 JSON 一发得 400(形态见清单19-6)
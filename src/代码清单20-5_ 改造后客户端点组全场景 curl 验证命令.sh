dotnet run --urls=http://localhost:6000
# 启动成功后另开一个终端执行; 体文件均预存 UTF-8 后 --data-binary 注入(第 19 章码页坎的解法)
printf '{"name":"青屿生鲜","email":"service@qingyu.example"}' > /tmp/cust1.json
printf '{"name":"南山果铺","email":"fruit@nanshan.example"}' > /tmp/cust2.json
printf '{"name":"青屿生鲜·旗舰店","email":"flagship@qingyu.example"}' > /tmp/cust1-update.json
printf '{"name":"望江茶行","email":"tea@wangjiang.example"}' > /tmp/cust50.json
printf '{"email":"ghost@nowhere.example"}' > /tmp/cust-bad.json
# 新建双发: 合法两笔, 各得 201 与各自 Location(POST 不幂等的证据)
curl -s -i -X POST -H "Content-Type: application/json" --data-binary @/tmp/cust1.json http://localhost:6000/api/v1/customers
curl -s -i -X POST -H "Content-Type: application/json" --data-binary @/tmp/cust2.json http://localhost:6000/api/v1/customers
# 重名再发: 得 409 与契约正文
curl -s -i -X POST -H "Content-Type: application/json" --data-binary @/tmp/cust1.json http://localhost:6000/api/v1/customers
# 列表与详情: 200; 查无: 404
curl -s -i http://localhost:6000/api/v1/customers
curl -s -i http://localhost:6000/api/v1/customers/1
curl -s -i http://localhost:6000/api/v1/customers/99
# 更新双发加状态核对: 两发同响应(PUT 幂等的证据); 查无建档双发: 两发同响应
curl -s -i -X PUT -H "Content-Type: application/json" --data-binary @/tmp/cust1-update.json http://localhost:6000/api/v1/customers/1
curl -s -i -X PUT -H "Content-Type: application/json" --data-binary @/tmp/cust1-update.json http://localhost:6000/api/v1/customers/1
curl -s http://localhost:6000/api/v1/customers/1
curl -s -i -X PUT -H "Content-Type: application/json" --data-binary @/tmp/cust50.json http://localhost:6000/api/v1/customers/50
# 删除三连: 204, 删后查询 404, 删后再删 404
curl -s -i -X DELETE http://localhost:6000/api/v1/customers/2
curl -s -i http://localhost:6000/api/v1/customers/2
curl -s -i -X DELETE http://localhost:6000/api/v1/customers/2
# 缺字段新建: 400 与契约正文([ApiController] 自动验证)
curl -s -i -X POST -H "Content-Type: application/json" --data-binary @/tmp/cust-bad.json http://localhost:6000/api/v1/customers
# 旧组冒烟: 命中第 19 章旧端点, 验证库已删故 500(旧端点在册的证据)
curl -s -i http://localhost:6000/orders/99
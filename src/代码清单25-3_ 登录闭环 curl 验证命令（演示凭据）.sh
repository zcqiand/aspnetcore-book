dotnet run --urls=http://localhost:6400
# 启动成功后另开一个终端执行
# 拍① 未登录打受保护端点：401
curl -s -i http://localhost:6400/me
# 拍② 登录：凭据为代码里的虚构演示值，cookie 存进 jar
curl -s -i -c cookies.txt -X POST -d 'username=demo_user&password=demo-pass-123456' http://localhost:6400/login
# 拍③ 带 cookie 再打：200
curl -s -i -b cookies.txt http://localhost:6400/me
# 对照一笔：错误凭据登录，401
curl -s -i -X POST -d 'username=demo_user&password=wrong-demo-pass' http://localhost:6400/login
# 对照二笔：登出，再打受保护端点
curl -s -i -b cookies.txt -c cookies.txt -X POST http://localhost:6400/logout
curl -s -o /dev/null -w 'http_code=%{http_code}\n' -b cookies.txt http://localhost:6400/me
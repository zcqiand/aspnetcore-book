# 第一步：查清线上现在跑的是哪个版本的镜像
docker ps --format "{{.Names}}  {{.Image}}"

# 第二步：停掉出毛病的容器，把上一个好版本的镜像重新拉起，换回服务
docker stop lab-app
docker rm lab-app
docker run -d --name lab-app registry.example.com/lab-app:<上一版镜像标签>

# 第三步：确认流量已经回到旧版，先恢复服务，再回头排查新版本的问题
docker ps --format "{{.Names}}  {{.Image}}"
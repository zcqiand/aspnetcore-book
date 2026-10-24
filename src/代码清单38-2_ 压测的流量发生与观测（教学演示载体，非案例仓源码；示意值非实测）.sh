# 教学示意：20 路并发、共 200 发，逐发记录响应耗时（time_total，单位秒，乘 1000 折毫秒），收尾汇总两样档位
url="http://localhost:5000/api/summary/stats"

# 流量这一手：并发数 20 路同时发，发满 200 发为止
seq 1 200 | xargs -P 20 -I{} curl -s -o /dev/null \
  -w "%{time_total}\n" "$url" > times.txt

# 观测这一手：times.txt 里是 200 个逐发耗时，
# 求平均后乘 1000 得平均响应耗时（毫秒），总发数除以总秒数得每秒请求数
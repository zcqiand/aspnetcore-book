// 教学示意：三类打底的计数器，一类一行；只看名目与结构，读数留白
// 攒下的数值统一从一个端点报出去，端点名用示意名（/stats）

var requestTotal = new Counter("requests_total");         // 请求来了多少：来一个，记一笔
var elapsedTotal = new Counter("handled_seconds_total");  // 每单耗时多长：干完一单，往总量上累一笔
var errorTotal   = new Counter("errors_total");           // 出过几次错：错一回，记一回
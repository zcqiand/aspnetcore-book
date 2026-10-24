// 教学示意：同一个聚合端点的三种优化形态，计时数字均为教学档位
// 基准态：三段串行聚合，各查各的，236 毫秒档，每请求查询 3 条
var card   = LoadCard(tid);      // 约 70 毫秒
var funnel = LoadFunnel(tid);    // 约 78 毫秒
var misc   = LoadSummary(tid);   // 约 88 毫秒

// 查询路：三段并成一条聚合查询，形状收紧，132 毫秒档，每请求查询 1 条
var all = LoadAggregated(tid);

// 缓存路：聚合结果预存，命中态直接回，6 毫秒档，查询 0 条
var hit = cache.Get($"summary:{tid}");   // 未命中回落基准态，照老路走

// 并行路：三段同时发，总耗时取最慢一段，88 毫秒档，不再相加
var cardTask    = LoadCardAsync(tid);
var funnelTask  = LoadFunnelAsync(tid);
var summaryTask = LoadSummaryAsync(tid);
await Task.WhenAll(cardTask, funnelTask, summaryTask);
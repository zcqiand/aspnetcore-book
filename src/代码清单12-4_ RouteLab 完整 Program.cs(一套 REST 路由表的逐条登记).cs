// 文件位置: RouteLab/Program.cs, 整体替换 dotnet new web 模板生成的那份(完整文件, 覆盖后即可运行)
// 与前几章的关系: 不动管道不加包, 专攻路由登记; 端口 5600 来自清单12-3 的 Kestrel 配置
// 路由表(本章交付物)在代码里的对应关系, 六行各归各行:
//   GET /                          -> 入口指引
//   GET /orders                    -> 订单列表
//   GET /orders/{orderId:int}      -> 订单详情(int 约束)
//   GET /orders/active             -> 进行中的订单(条件查询)
//   GET /orders/page/{page:int=1}  -> 分页查询(默认第 1 页)
//   其余一切路径                    -> MapFallback 兜底 404

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// 路由表第 1 行: 入口指引——报出表中其余路径, 让新来的请求有处可去
app.MapGet("/", () => Results.Text("这是 RouteLab 的入口, 路由表上的路径: /orders 、 /orders/1 、 /orders/active 、 /orders/page 。"));

// 内存订单库: 3 笔订单, 编号 1 到 3; 数据沿用第 11 章的订单语境但整体重新登记——
// RouteLab 是独立项目, 与 LogLab 的订单库互不共享;
// 这里按 Id 查找, 不再有第 11 章拿订单号当数组下标的错位问题
var orderStore = new[]
{
    new { Id = 1, Description = "云海咖啡豆 x2", Status = "已发货" },
    new { Id = 2, Description = "阿里山茶行 x1", Status = "已签收" },
    new { Id = 3, Description = "青屿生鲜 x5", Status = "配送中" }
};

// 路由表第 2 行: 订单列表——整库列出, 分页的账归第 5 行管, 一行只认领一条路径
app.MapGet("/orders", () => Results.Text($"订单列表: 当前共 {orderStore.Length} 笔在库。"));

// 路由表第 3 行: 订单详情。
// 模板形态沿用第 11 章: 花括号包参数名, lambda 参数用 int 接;
// 在此之上追加了 :int 路由约束——订单号必须能解析成整数才允许命中,
// 拼错成 /orders/abc 的请求进不了端点函数, 直接落空给 404,
// 错误请求被挡在门外, 端点里就不必再防一手非法编号;
// MapGet 模板写法此处先照第 11 章的形态借用, 端点风格的归类口径第 19 章正式展开
app.MapGet("/orders/{orderId:int}", (int orderId) =>
{
    var order = orderStore.FirstOrDefault(o => o.Id == orderId);
    // 命中路由不代表有货: 编号超界(/orders/99)是业务层的无货, 由端点自己回 404 说清缘由,
    // 它与约束不匹配的 404 出处不同——一个出自端点, 一个出自框架加兜底, 清单12-7 里对照可见
    return order is null
        ? Results.NotFound($"订单 {orderId} 不存在, 可查的编号是 1 到 {orderStore.Length}。")
        : Results.Text($"订单{order.Id}: {order.Description} ({order.Status})");
});

// 路由表第 4 行: 进行中的订单——登记次序排在参数模板之后照样各归各行,
// active 不是整数, 进不了带 {orderId:int} 约束的详情端点, 约束在这里就是隔离带
app.MapGet("/orders/active", () =>
{
    var active = orderStore.Where(o => o.Status == "配送中")
        .Select(o => $"订单{o.Id}: {o.Description}");
    return Results.Text($"进行中的订单: {string.Join("、", active)}。");
});

// 路由表第 5 行: 分页查询——{page:int=1} 在约束之外又添了内联默认值,
// 等号后的 1 是缺省值: /orders/page 与 /orders/page/1 等效, URL 缺的那段由默认值顶上
app.MapGet("/orders/page/{page:int=1}", (int page) =>
    Results.Text($"订单列表: 第 {page} 页, 当前共 {orderStore.Length} 笔在库。"));

// 兜底: 谁都没认领的请求统一落到这里, 双轨口径与第 5 章一致, 12.5 节展开;
// 约束不匹配(/orders/abc)也走这条路: 端点没被命中, 请求落进兜底
app.MapFallback(() => Results.NotFound("你访问的路径不存在, 先试试 /orders/1 。"));

// 启动监听——端口由清单12-3 的 Kestrel 配置决定, 这份代码里没有端口字样:
// 换端口只动配置不动代码, 第 6 章宿主域的分界结论在路由实验里继续成立
app.Run();

// 预期结果: dotnet run 后监听 http://localhost:5600;
// 清单12-5 的 curl 集按路由表逐行打点, 命中与落空的完整对照见清单12-6 与清单12-7
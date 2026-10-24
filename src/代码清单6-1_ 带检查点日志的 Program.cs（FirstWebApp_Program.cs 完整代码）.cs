// 第 1 行: 备料——CreateBuilder 读齐默认配置源、预置日志、备好服务容器,
// 返回的 builder 就是本章主角 WebApplicationBuilder, 后续配置都先记在它身上
var builder = WebApplication.CreateBuilder(args);

// 第 2 行: 动工——Build() 把备好的原料组装成宿主, 应用实例 app 从这一刻才存在
var app = builder.Build();

// 检查点 1: 刻意放在 Build 之后而非更早, 因为 app.Logger 要等宿主组装完才拿得到,
// Build 之前既没有 app 也没有日志器; 这行日志能打印出来, 就证明 Build 确实先于监听完成
app.Logger.LogInformation("检查点 1: Build 完成, 宿主已就位, 监听尚未开始");

// 第 3 行: 登记根路径约定——GET / 由这个函数应答, 文本与第 5 章完全一致
app.MapGet("/", () => "这是来自 FirstWebApp 的问候: 你的第一个 ASP.NET Core 应用正在运行。");

// 第 4 行: 兜底——没人认领的请求统一落到这里, 回 404 加一句指引, 规矩与第 5 章相同
app.MapFallback(() => Results.NotFound("你访问的路径不存在, 先试试根路径 / 。"));

// 检查点 2: 端点数不写死在文案里, 而是用 {Count} 占位符把数字交给日志框架填入,
// 这套「模板加参数」的写法第 11 章还会正式遇到, 此处先眼熟;
// 数字 2 正对应上面登记的 MapGet 与 MapFallback 两条端点
app.Logger.LogInformation("检查点 2: 处理链登记完毕, 共 {Count} 条端点", 2);

// 停机兜底: 从宿主自带的服务里取出生命周期服务, 向 ApplicationStopping 钩子挂一个回调,
// 按 Ctrl+C 停止时它会执行——开门有日志, 打烊也要有日志, 日后排查「应用何时被谁停掉」全靠它;
// 这是框架内置能力, 不需要安装任何额外的包
var applicationLifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
applicationLifetime.ApplicationStopping.Register(() =>
{
    app.Logger.LogInformation("停机日志: 收到停止信号, FirstWebApp 正在打烊");
});

// 检查点 3: Run() 一执行程序就停在这一行等请求, 之后的一切日志都出自宿主之手,
// 这行日志出现在框架四组日志之前, 正是「登记先于监听」的直接证据
app.Logger.LogInformation("检查点 3: 准备开门营业, 即将启动监听");

// 第 5 行: 启动监听——组装处理链, 拉起 Kestrel, 打出 Now listening on, 然后停驻;
// 端口被占用这类启动失败第 5 章已预演: 框架会带着原因直接退出, 不必在这里额外捕获
app.Run();

// 预期结果: dotnet run 后, 控制台先出现三行检查点日志(检查点 1/2/3, 全部出自这份代码),
// 随后才是框架四组日志(Now listening on, Application started, Hosting environment, Content root path);
// 按 Ctrl+C 停止时, 还会多出停机日志一行, 证明生命周期钩子已生效
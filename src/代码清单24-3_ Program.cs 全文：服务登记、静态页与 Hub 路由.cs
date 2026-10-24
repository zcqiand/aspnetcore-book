using ChatLab;

var builder = WebApplication.CreateBuilder(args);

// SignalR 的服务端全部随 ASP.NET Core 共享框架而来：AddSignalR 注册路由与
// 协议协商所需的整套服务，本工程零 PackageReference，没有任何装包一步。
builder.Services.AddSignalR();

var app = builder.Build();

// wwwroot 下的静态页（聊天室前端）与 js/signalr.min.js 都从这里出。
// UseDefaultFiles 让访问根路径时直接回 index.html，不必手敲文件名。
app.UseDefaultFiles();
app.UseStaticFiles();

// 路由分组沿用第 19 章的 MapGroup 体例：/hubs 前缀下集中登记实时通道，
// 以后若加第二条 Hub，往同一组里添一行即可。
var hubs = app.MapGroup("/hubs");
hubs.MapHub<ChatHub>("/chat");

app.Run();
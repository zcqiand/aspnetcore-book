// 下面这 5 个有效行就是 web 模板替你写好的全部内容, 一个字都不多
// 本章将清空它自己重写, 所以此刻看清它即可, 不必背它

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
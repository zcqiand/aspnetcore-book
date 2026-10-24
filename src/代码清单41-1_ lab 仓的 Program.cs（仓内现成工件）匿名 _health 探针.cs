// 仓里现成的一支匿名探针，照实引用；应用自己开端点作答，活没活着从猜变成问；与容器层 HEALTHCHECK 一里一外，两层各答各的

// 匿名 /health 探针（deploy 脚本 wget 探 200；与 saas-aspnetcore MapGet("/health") 同模式）
app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();
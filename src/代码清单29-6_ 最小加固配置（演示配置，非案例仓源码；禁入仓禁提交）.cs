// ==== 服务侧：数据保护持久化 + 管理面授权策略（RequireClaim 一级轮廓）====
// 演示路径，生产按 29.7 清单第三项落位到受保护的位置
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "demo-keys")));
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminClientsOnly", p => p.RequireClaim("role", "platform_admin"));
});

// ==== 管道侧：CSP 响应头 + HTTPS 重定向 + HSTS ====
app.Use(async (ctx, next) =>
{
    ctx.Response.OnStarting(() =>
    {
        ctx.Response.Headers["Content-Security-Policy"] = "default-src 'self'";
        return Task.CompletedTask;
    });
    await next();
});
app.UseHttpsRedirection();
app.UseHsts();
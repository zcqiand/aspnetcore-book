// 建库与对齐: 库不存在时按迁移建库, 已存在时补齐尚未应用的迁移, 两头都归 Migrate 管
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    app.Logger.LogInformation("数据库就绪(启动迁移已对齐)");
}
// 示教形，非仓内实物，逐行现写；一张清单声明多服务与依赖；一条命令整套起起来，系机制面示意；示意项目名示意资源名，零版本字面

var builder = DistributedApplication.CreateBuilder(args);

var db    = builder.AddPostgres("lab-db");      // 示意资源名
var cache = builder.AddRedis("lab-cache");      // 示意资源名

builder.AddProject<Projects.CatalogService>("catalog-service")   // 示意项目名
       .WithReference(db);
builder.AddProject<Projects.ReportService>("report-service")     // 示意项目名
       .WithReference(db)
       .WithReference(cache);

builder.Build().Run();
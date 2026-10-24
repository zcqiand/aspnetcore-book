// M10.Database — EF Core + Npgsql + snake_case 命名（ADR-0010）
// shared SQL 是 SSOT；EF Model 镜像；启动时**不调** Database.Migrate()（避免与 shared SQL 重复执行）。
// 启动期校验：open connection + information_schema.tables 验证 expected tables 存在。
//
// Npgsql 8 起 Dictionary<string,object?> ↔ jsonb 动态映射需显式 EnableDynamicJson()（不再默认开启）。
// 不开就报：Reading as 'Dictionary`2' is not supported for fields having DataTypeName 'jsonb'。
// 同样 ToSettingsDto 里 Str() / maxUsers switch 仍是必要的——System.Text.Json 反序列化原语值仍是 JsonElement。
// DATABASE_URL 是全家族统一 key（2026-08-28 接线；deploy 脚本只写它，不再依赖
// appsettings.json 内嵌的 dev 连接串）。ConnectionStrings:Postgres 仍作 fallback。
var pgConn = builder.Configuration["DATABASE_URL"]
    ?? builder.Configuration.GetConnectionString("Postgres");
// 2026-09-12 live 跑批 53300 修复：Npgsql 默认 Max Pool Size=100，四方 contract-test
// 全量并发跑批时把共享 PG（max_connections=100，同实例还住着 lab/saas_prod 等 4 个库）
// 打满 → "sorry, too many clients already" → login 500 级联（59 failed 的主因）。
// 显式压到 10，与 springboot Hikari / nextjs pg-pool 的默认档对齐——多后端共库是
// 「前端不可区分」的物理基础，单后端不得独占连接预算。
var dataSourceBuilder = new NpgsqlDataSourceBuilder(
    new NpgsqlConnectionStringBuilder(pgConn) { MaxPoolSize = 10 }.ConnectionString);
dataSourceBuilder.EnableDynamicJson();
// v0.5.0：所有 PG native enum 已废（api_key_status / audit_action / user_status /
// membership_status / tenant_status / app_status / menu_status / menu_type /
// oauth_grant_type），对应表已 DROP 或 status 列改 smallint（9/7 重组）。
// Scaffold DbContext 自动按 smallint 读写，无需 MapEnum。
// 若后续需要重新引入 PG native enum，对应 entity 也需 Domain/Entities/*.cs 复活
// —— 目前 Generated/* 都是 smallint 映射。
var dataSource = dataSourceBuilder.Build();
builder.Services.AddSingleton(dataSource);
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseNpgsql(dataSource));
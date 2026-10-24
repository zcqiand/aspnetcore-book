    public sealed class Factory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Program.cs fail-fast 三件套（ADR-0019 / 2026-08-30）必须给足：
            // JWT_SIGNING_KEY ≥32B、SAAS_CORS_ALLOWED_ORIGINS、DATABASE_URL（InMemory 替换后不真连）。
            builder.UseSetting("JWT_SIGNING_KEY", "unit-test-signing-key-0123456789abcdef-32b!");
            builder.UseSetting("JWT_ISSUER", "https://unit-test-534.local");
            builder.UseSetting("JWT_AUDIENCE", "unit-test-534");
            builder.UseSetting("SAAS_CORS_ALLOWED_ORIGINS", "http://localhost:5173");
            builder.UseSetting("DATABASE_URL", "Host=localhost;Port=5432;Database=unused_534;Username=test;Password=test");
            builder.ConfigureServices(services =>
            {
                // InMemory 替身（csproj 既有 8.0.10 依赖，PartialUpdateSemanticsTests 同源）：
                // 替换 Program 的 Npgsql DbContextOptions，不碰 PG。
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase("err-semantics-534"));
            });
        }
    }
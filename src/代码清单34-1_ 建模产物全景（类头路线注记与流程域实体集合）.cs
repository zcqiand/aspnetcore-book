/// <summary>
/// EF Core 映射共享 PG 库——DB-First（ADR-0025/ADR-0033）：schema 真源 =
/// shared src/db/schema.ts（db:migrate 物化到 lab_dev/lab_test），本仓只读不写、
/// 禁 EF Migrations；漂移防线 = tests/Harness/LabDbContextSchemaTest.cs（模型 ↔ 库逐列对照）
/// + scripts/sync-db.sh（ADR-0026 marker）。
///
/// 实体直接复用 NSwag 生成的 DTO 类（stores/services 的既有类型，换装零转换层）：
/// - 列名走 UseSnakeCaseNamingConvention（PascalCase -&gt; snake_case，与 SSOT DDL 全量一致；
///   表名用 ToTable 显式给）
/// - 枚举列全是 text，值 = 契约小写串（[EnumMember]），走 Wire 转换器 --
///   与线上 JSON 同一套值（net8 EF 默认 HasConversion&lt;string&gt; 用成员名，会分叉）
/// - jsonb 列：List&lt;string&gt; / List&lt;POCO&gt; / IDictionary（object 值需数据源
///   EnableDynamicJson，见 Program.cs）
/// - issuedAt 是 timestamptz 而 DTO 是 string：ISO 转换器（服务层从不写入，仅留位）
/// - AdditionalProperties（JsonExtensionData）逐实体 Ignore
/// </summary>
public class LabDbContext(DbContextOptions<LabDbContext> options) : DbContext(options)
{
    // B3 流程域
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<SampleReceipt> SampleReceipts => Set<SampleReceipt>();
    public DbSet<Sample> Samples => Set<Sample>();
    public DbSet<TestRecord> TestRecords => Set<TestRecord>();
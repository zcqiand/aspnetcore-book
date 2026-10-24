/// <summary>
/// JwtIssuer — HS256 签名 JWT（Phase 6 OAuth + Phase 5 password login 共用）。
///
/// 从 AuthController.IssueAccessToken 抽出（v0.4.0 之前 v0.1.10 加 HS256 时塞在
/// AuthController 内, 现在 OauthController 也要用, 提到独立 service）。
///
/// Claims:
///   sub (userId) — JWT 标准 sub claim（mapInboundClaims=false 让名字保留）
///   tenant_id     — saas 域 tenant claim（lab-aspnetcore JwtBearer 用同一 key 校验）
///   jti           — 防重放随机 id
///   iat / nbf / exp — 标准时间字段
///
/// 配置:
///   Jwt:SigningKey ≥32B (HS256), Jwt:Issuer, Jwt:Audience。
///   3 个 saas 后端（nextjs / aspnetcore / springboot）+ 3 个 lab 后端的 JWT 签名 key
///   单一共享 —— 见 stateful-cuddling-cherny.md 决策 §1。
/// </summary>
public class JwtIssuer
{
    private readonly string _signingKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _ttlSeconds;

    public JwtIssuer(IConfiguration config)
    {
        // 统一 flat 命名: env JWT_SIGNING_KEY 直接被 ASP.NET 默认 env provider 当作 flat key 读
        // (不通过 `:` 段映射)。Phase 4 env 对称化后所有 6 个 saas/lab 后端 + 2 个 msw + 2 个 nextjs-self
        // 共 9 仓 + 6 个前端仓都用 JWT_SIGNING_KEY 这一名字, 跨 dev 环境互签互验。
        _signingKey = config["JWT_SIGNING_KEY"]
            ?? throw new InvalidOperationException("JWT_SIGNING_KEY not configured");
        _issuer = config["JWT_ISSUER"]
            ?? throw new InvalidOperationException("JWT_ISSUER not configured");
        _audience = config["JWT_AUDIENCE"]
            ?? throw new InvalidOperationException("JWT_AUDIENCE not configured");

        // HS256 ≥32 字节（256 bits），不足直接抛 — 防止 prod 用弱 dev 默认 key
        if (Encoding.UTF8.GetByteCount(_signingKey) < 32)
            throw new InvalidOperationException($"JWT_SIGNING_KEY must be >=32 bytes for HS256 (got {Encoding.UTF8.GetByteCount(_signingKey)})");

        _ttlSeconds = int.TryParse(config["JWT_TTL_SECONDS"], out var t) ? t : 3600;
    }
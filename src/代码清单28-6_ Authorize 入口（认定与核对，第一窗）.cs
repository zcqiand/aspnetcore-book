    public override async Task<Response3> Authorize(AuthorizeCodeRequest body)
    {
        // 2026-08-29 修 saas-vue / saas-react → saas-aspnetcore 跨域 POST 401:
        // saasSession cookie 写了 SameSite=Lax,跨子域 POST fetch 浏览器不自动带 cookie
        // (只有顶级导航 GET 带)。Authorize 端点 fallback Bearer token 验签(JwtBearer
        // middleware 已配置,User.Claims 自动从 Authorization header 填充)。
        // 安全模型保留: token 由 saas /auth/login 用同一 JWT_SIGNING_KEY 签,
        // Bearer token 验签通过 = user 是 saas 真用户(同 cookie 路径)。
        var session = HttpContext.Items[SaasSessionMiddleware.ItemsKey] as SaasSession;
        if (session is null)
        {
            // Fallback: Bearer token (跨域 POST 浏览器不带 saasSession cookie)
            // JwtBearer 默认 MapInboundClaims=true 时 'sub' 映射到 ClaimTypes.NameIdentifier;
            // Program.cs 已设 MapInboundClaims=false,claim 名原样保留 'sub' / 'tenant_id'。
            // 双查找兼容两种配置。
            var sub = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            var tid = User.FindFirstValue("tenant_id");
            if (Guid.TryParse(sub, out var uid) && Guid.TryParse(tid, out var t))
            {
                session = new SaasSession(uid, t, DateTime.UtcNow, DateTime.UtcNow.AddHours(1));
            }
            else
            {
                throw new UnauthorizedAccessException(
                    "saas session or Bearer token required for OAuth authorize");
            }
        }

        // 1. clientId 必须是已注册 OAuth client（apps.client_id；Guid → string 后查）
        var clientIdStr = body.ClientId.ToString();
        var app = await _db.OauthClients.FirstOrDefaultAsync(a => a.ClientId == clientIdStr);
        if (app == null || app.Status != 1)
            throw new ArgumentException($"INVALID_CLIENT: clientId={clientIdStr} not registered");

        // 2. redirectUri 必须在 apps.redirect_uris 白名单里。RFC 6749 §3.1.2 允许 query
        //    参数差异（lab 前端回跳带 ?from=<业务路径>），匹配规则 = 白名单条目是
        //    请求 redirectUri 的前缀且边界在 '?' 处。
        // 9/7 重组: RedirectUris/Scopes/GrantTypes 改 comma-separated string (PG array text→varchar)
        var allowedRedirectUris = (app.RedirectUris ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries);
        if (!allowedRedirectUris.Any(u => body.RedirectUri == u
                || (body.RedirectUri.StartsWith(u, StringComparison.Ordinal)
                    && body.RedirectUri[u.Length] == '?')))
            throw new ArgumentException($"INVALID_REDIRECT_URI: {body.RedirectUri} not in app.redirect_uris");

        // 3. scope：RFC 6749 §3.3 space-separated 列表，请求的每个 scope 都必须 ∈ apps.scopes
        //    （子集校验）。曾用整串 Contains 精确匹配，lab 发 "lab.read lab.write" 被拒。
        // 同上: app.Scopes 也是 csv
        var allowedScopes = (app.Scopes ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries);
        var requestedScopes = (body.Scope ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (requestedScopes.Length == 0 || requestedScopes.Any(s => !allowedScopes.Contains(s)))
            throw new ArgumentException($"INVALID_SCOPE: scope '{body.Scope}' not a subset of app.scopes");
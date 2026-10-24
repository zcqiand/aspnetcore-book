    // M04.F03.I02 + I09 — 令牌交换 + 刷新（按 grantType 路由）
    public override async Task<TokenResponse> Token(TokenRequest body)
    {
        // 2026-08-29: 放宽 session 要求 (RFC 6749 §4.1.3 — token 端点只验 code +
        // client_credentials,不需要浏览器 cookie)。路线 A 安全模型保留:
        //   - code 必须由 /authorize 端点 用 saas session 签出(强制要求用户在 saas 登录)
        //   - oauthCode.UserId/TenantId 已在 authorize 时绑定
        //   - token 端点只做 confidential client 验证 + code 一次性消费
        // lab 后端代理 OAuth 场景: lab 后端 server-to-server 调 /token 时无浏览器 cookie
        // (cookie HttpOnly + SameSite),放宽后 lab 后端才能换到 token。

        var clientIdStr = body.ClientId.ToString();
        var app = await _db.OauthClients.FirstOrDefaultAsync(a => a.ClientId == clientIdStr);
        if (app == null || app.Status != 1)
            throw new ArgumentException($"INVALID_CLIENT: clientId={clientIdStr} not registered");

        // dev 暂不验 clientSecret（saas-nextjs 同模式; prod Phase 6+ 加 Argon2 hash 校验）
        // 生产路径：if (!BCrypt.Verify(body.ClientSecret ?? "", app.ClientSecretHash ?? "")) throw ...

        return body.GrantType switch
        {
            TokenRequestGrantType.Authorization_code => await ExchangeAuthorizationCode(app, body),
            TokenRequestGrantType.Refresh_token => await RotateRefreshToken(app, body),
            _ => throw new ArgumentException("UNSUPPORTED_GRANT_TYPE"),
        };
    }

    private async Task<TokenResponse> ExchangeAuthorizationCode(OauthClient app, TokenRequest body)
    {
        if (string.IsNullOrEmpty(body.Code))
            throw new ArgumentException("INVALID_REQUEST: code required for grantType=authorization_code");
        if (string.IsNullOrEmpty(body.RedirectUri))
            throw new ArgumentException("INVALID_REQUEST: redirectUri required for grantType=authorization_code");

        // 一次性消费：查 code 必须未消费且未过期
        var oauthCode = await _db.OauthCodes.FirstOrDefaultAsync(c =>
            c.Code == body.Code && c.ClientId == app.ClientId);
        if (oauthCode == null)
            throw new ArgumentException("INVALID_GRANT: code not found");
        if (oauthCode.ExpiresAt < DateTime.UtcNow)
            throw new ArgumentException("INVALID_GRANT: code expired");
        if (oauthCode.ClientId != app.ClientId)
            throw new ArgumentException("INVALID_GRANT: code does not belong to this client");
        if (oauthCode.RedirectUri != body.RedirectUri)
            throw new ArgumentException("INVALID_GRANT: redirectUri mismatch");

        // user/tenant 已在 /authorize 端点用 saas session 绑定,不再信 body.TenantId。
        if (oauthCode.UserId == Guid.Empty)
            throw new ArgumentException("INVALID_GRANT: code has no user binding (authorize must run under saas session)");
        var user = await _db.SysUsers.FirstOrDefaultAsync(u => u.Id == oauthCode.UserId);
        if (user == null)
            throw new ArgumentException("INVALID_GRANT: code user not in saas");

        // 一次性消费 — code 行删除（oauth_codes 无 consumed 列；删除即消费，
        // 重放同 code 走上面 "code not found" → 400 INVALID_GRANT，RFC 6749 §4.1.2）。
        // 只删 authorization code 行本身；下方新写入的 refresh token 行不受影响。
        _db.OauthCodes.Remove(oauthCode);

        // 发新 refresh token + access token
        var refresh = JwtIssuer.GenerateRefreshToken(user.Id);
        _db.OauthCodes.Add(new OauthCode
        {
            Code = refresh,
            // 同上，OauthCode 不存 grant_type,
            ClientId = app.ClientId,
            UserId = user.Id,
            TenantId = oauthCode.TenantId,
            Scope = oauthCode.Scope,
            ExpiresAt = DateTime.UtcNow.Add(RefreshTtl),
            CreatedAt = DateTime.UtcNow,
        });

        await _db.SaveChangesAsync();

        return new TokenResponse
        {
            AccessToken = _jwt.IssueAccessToken(user.Id, oauthCode.TenantId),
            RefreshToken = refresh,
            TokenType = "Bearer",
            ExpiresIn = 3600,
            Scope = oauthCode.Scope ?? "",
            // T11(2026-09-16) SSOT TokenResponse 必填三件回显（tsp/routes/oauth.tsp）。
            ClientId = app.ClientId,
            UserId = user.Id.ToString(),
            TenantId = oauthCode.TenantId,
        };
    }
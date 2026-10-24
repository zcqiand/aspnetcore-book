        // 4. 生成 code 格式: saas-code-{ts-ms}-{rand-base64}（与 saas-nextjs 同款便于跨 IdP 排障）
        var code = GenerateCode();

        // 绑定 session user/tenant 到 oauth_code(token 端点不再要 session,要从 code 取 user_id 拿 token)。
        // 2026-08-29 修 prod 401: 之前漏绑 session.UserId,导致 token 端点放宽后所有 test 失败
        // ('INVALID_GRANT: code has no user binding')。路线 A 安全模型核心: code 必须绑定真 saas
        // session 里的 user,不允许 body 任意传 tenantId 领 token。
        var oauthCode = new OauthCode
        {
            Code = code,
            // 9/7 重组：OauthCode.GrantType 列 DROP（grant_type 走 token request body）
            ClientId = app.ClientId,
            UserId = session.UserId,
            TenantId = session.TenantId,
            RedirectUri = body.RedirectUri,
            Scope = body.Scope,
            ExpiresAt = DateTime.UtcNow.Add(CodeTtl),
            CreatedAt = DateTime.UtcNow,
        };
        _db.OauthCodes.Add(oauthCode);
        await _db.SaveChangesAsync();

        return new Response3
        {
            Code = code,
            State = body.State,
        };
    }
    // === M01.F05.I04 刷新 token ===

    public LoginResponse Refresh(RefreshTokenRequest body)
    {
        if (body?.RefreshToken is null)
        {
            throw new AuthenticationException("missing refresh_token");
        }
        Dictionary<string, object> claims;
        try
        {
            claims = _jwt.Verify(body.RefreshToken);
        }
        catch (ArgumentException e)
        {
            throw new AuthenticationException("invalid refresh_token: " + e.Message);
        }
        if (claims.GetValueOrDefault("typ")?.ToString() != "refresh")
        {
            throw new AuthenticationException("invalid refresh_token: not a refresh token");
        }
        var tenantId = claims.GetValueOrDefault("tenant_id")?.ToString();
        var saasRefresh = claims.GetValueOrDefault("saas_refresh_token")?.ToString();
        if (string.IsNullOrEmpty(saasRefresh))
        {
            throw new AuthenticationException("invalid refresh_token: missing saas_refresh_token claim");
        }
        var t = _saasAuth.TokenAsync("refresh_token", null, saasRefresh, null).GetAwaiter().GetResult();
        var saasUser = _saasMe.WhoamiAsync(t.AccessToken).GetAwaiter().GetResult();
        var memberships = _saasMe.ListMyTenantsAsync(t.AccessToken).GetAwaiter().GetResult();
        // 2026-08-29 修 prod: Refresh 端点漏 Upsert(对比 SsoCallback line 207-211)。
        // 新部署容器内用户目录是空的,FindByEmail 必返 null → 抛 401 →
        // hydrateAuth catch → doRefresh 失败 → 退 ANON → LoginPage 阶段 2 不会触发
        // (用户卡在 "检查登录态...")。修: 跟 SsoCallback 同款 Upsert。
        var labUser = _directory.FindByEmail(saasUser.Email)
            ?? _directory.Upsert(saasUser.Id, saasUser.Email, saasUser.DisplayName ?? "", "viewer");
        // 2026-08-29: Refresh 端点也要更新 saas refresh_token (rotate-once 语义,
        // saas /token 返回新 refresh_token) + CacheMenus 重填 cache。
        _directory.SetSaasRefreshToken(labUser.Id, t.RefreshToken ?? "");
        CacheMenus(labUser.Id, t.AccessToken);
        // 2026-09-15 租户显示名：同 SsoCallback，瞬时持 token 拉平台租户列表注入真名
        var tenantNameById = FetchTenantNames(labUser.Id, t.AccessToken);
        _membershipCache.Put(labUser.Id, TenantsFrom(memberships, tenantNameById));
        return Session(labUser, tenantId, TenantsFrom(memberships, tenantNameById), t.RefreshToken);
    }
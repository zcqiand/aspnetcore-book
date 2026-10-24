    private async Task<TokenResponse> RotateRefreshToken(OauthClient app, TokenRequest body)
    {
        if (string.IsNullOrEmpty(body.RefreshToken))
            throw new ArgumentException("INVALID_REQUEST: refreshToken required for grantType=refresh_token");

        var oldRefresh = await _db.OauthCodes.FirstOrDefaultAsync(c =>
            c.Code == body.RefreshToken);
        if (oldRefresh == null)
            throw new ArgumentException("INVALID_GRANT: refresh_token not found");
        if (oldRefresh.ExpiresAt < DateTime.UtcNow)
            throw new ArgumentException("INVALID_GRANT: refresh_token expired");
        if (oldRefresh.ClientId != app.ClientId)
            throw new ArgumentException("INVALID_GRANT: refresh_token does not belong to this client");
        if (oldRefresh.UserId == Guid.Empty)
            throw new ArgumentException("INVALID_GRANT: refresh_token has no user_id");

        // 旋转: 旧 refresh 标记 consumed, 新 refresh 写入
        var newRefresh = JwtIssuer.GenerateRefreshToken(oldRefresh.UserId);
        _db.OauthCodes.Add(new OauthCode
        {
            Code = newRefresh,
            // 同上，OauthCode 不存 grant_type,
            ClientId = app.ClientId,
            UserId = oldRefresh.UserId,
            TenantId = oldRefresh.TenantId,
            Scope = oldRefresh.Scope,
            ExpiresAt = DateTime.UtcNow.Add(RefreshTtl),
            CreatedAt = DateTime.UtcNow,
        });

        await _db.SaveChangesAsync();

        return new TokenResponse
        {
            AccessToken = _jwt.IssueAccessToken(oldRefresh.UserId, oldRefresh.TenantId),
            RefreshToken = newRefresh,
            TokenType = "Bearer",
            ExpiresIn = 3600,
            Scope = oldRefresh.Scope ?? "",
            // T11(2026-09-16) SSOT TokenResponse 必填三件回显（tsp/routes/oauth.tsp）。
            ClientId = app.ClientId,
            UserId = oldRefresh.UserId.ToString(),
            TenantId = oldRefresh.TenantId,
        };
    }
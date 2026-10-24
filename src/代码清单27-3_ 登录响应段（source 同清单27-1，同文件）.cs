        var sid = _sessions.GenerateId();
        var session = new SaasSession(
            UserId: user.Id,
            TenantId: currentTenantId,
            CreatedAt: DateTime.UtcNow,
            ExpiresAt: DateTime.UtcNow.Add(_sessions.DefaultTtl));
        _sessions.Put(session with { Id = sid });

        // 写 Set-Cookie 头（HttpOnly + SameSite=Lax；Secure 由反向代理加）
        Response.Cookies.Append(
            SaasSessionMiddleware.CookieName,
            sid,
            new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Secure = Request.IsHttps,
                Path = "/api",
                Expires = session.ExpiresAt,
            });

        return new LoginResponse
        {
            // ADR-0032 重 emit：LoginResponse = { user: SysUser, availableTenants: TenantMembership[],
            // userId(required), currentTenantId?, accessToken, refreshToken, tokenType, expiresIn, clientId }。
            User = new DtoSysUser
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Mobile = user.Mobile,
                Status = StatusEnumMaps.MapUserStatus(user.Status),
                FailedAttempts = user.FailedAttempts,
                LockedUntil = user.LockedUntil != null ? new DateTimeOffset(DateTime.SpecifyKind(user.LockedUntil.Value, DateTimeKind.Utc)) : default,
                CreatedAt = new DateTimeOffset(DateTime.SpecifyKind(user.CreatedAt, DateTimeKind.Utc)),
                UpdatedAt = new DateTimeOffset(DateTime.SpecifyKind(user.UpdatedAt, DateTimeKind.Utc)),
            },
            AvailableTenants = MembershipViews.FromEntities(members),
            UserId = user.Id,
            CurrentTenantId = currentTenantId,
            // 契约 required string；msw oracle 取请求体 clientId ?? ""（ADR-0019 不适用：
            // login 的 clientId 是路由上下文回显，不是业务身份判权字段）。
            ClientId = body.ClientId ?? "",
            AccessToken = _jwt.IssueAccessToken(user.Id, currentTenantId),
            RefreshToken = $"refresh-{user.Id}-{((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds()}",
            TokenType = "Bearer",
            ExpiresIn = 3600,
        };
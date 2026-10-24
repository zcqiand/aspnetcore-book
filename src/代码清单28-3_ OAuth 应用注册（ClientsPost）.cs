    public override async Task<ApiClient> ClientsPost(CreateOAuthClientRequest body)
    {
        // 2026-09-12 四方 live（I45）根因修复：请求未带 validity 时（生成 DTO 是非可空 int，
        // 缺省 = CLR 0）此前落 0 → EF 视为 CLR default 跳过列、读回 DB 默认 7200/2592000，
        // 与 oracle（msw POST handler 实测）及 nextjs 的应用层默认 3600/86400 分叉。
        // 这里显式应用家族默认：未传/非正数 → accessToken 3600s / refreshToken 86400s。
        var e = new DbClient
        {
            Id = Guid.NewGuid(),
            ClientId = body.ClientId,
            ClientSecret = body.ClientSecret,
            ClientName = body.ClientName,
            GrantTypes = body.GrantTypes,
            RedirectUris = body.RedirectUris,
            Scopes = body.Scopes,
            AccessTokenValidity = body.AccessTokenValidity is int atv && atv > 0 ? atv : 3600,
            RefreshTokenValidity = body.RefreshTokenValidity is int rtv && rtv > 0 ? rtv : 86400,
            AutoApprove = body.AutoApprove ?? false,
            Status = 1, // active
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        _db.OauthClients.Add(e);
        await _db.SaveChangesAsync();
        return ToDto(e);
    }
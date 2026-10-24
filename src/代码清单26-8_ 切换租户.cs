    public override async Task<SwitchTenantResponse> Switch(string tenantId, string? clientId)
    {
        var uid = CurrentUserId()
            ?? throw new UnauthorizedAccessException("Bearer sub required for tenant switch");
        var tid = Guid.Parse(tenantId);
        var member = await _db.TenantMembers
            .FirstOrDefaultAsync(m => m.UserId == uid && m.TenantId == tid && m.Status != 0); // disabled 不可切换
        if (member is null)
            throw new KeyNotFoundException($"user {uid} is not a member of tenant {tid}");
        return new SwitchTenantResponse
        {
            AccessToken = _jwt.IssueAccessToken(uid, tid),
            RefreshToken = JwtIssuer.GenerateRefreshToken(uid),
            ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(_jwt.TtlSeconds),
            TenantId = tid,
        };
    }
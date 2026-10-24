    public override async Task<ApiApp> ApplicationsPost(
        string tenantId, SubscribeTenantApplicationRequest body)
    {
        _guard.VerifyPathTenant(tenantId);
        var tid = Guid.Parse(tenantId);
        // 无 (tenant_id, client_id) unique index —— 应用层查重，拒绝重复订阅
        //（否则 Patch/Delete 的 FirstOrDefaultAsync 会静默取任意一行）。
        // ArgumentException → 400 INVALID_REQUEST（Program.cs 异常映射段已映射；InvalidOperationException 未映射会落 500）。
        var duplicate = await _db.TenantApplications
            .AnyAsync(ta => ta.TenantId == tid && ta.ClientId == body.ClientId);
        if (duplicate)
        {
            throw new ArgumentException(
                $"subscription already exists: tenant={tenantId} client={body.ClientId}");
        }
        var client = await _db.OauthClients.FirstOrDefaultAsync(c => c.ClientId == body.ClientId)
            ?? throw new KeyNotFoundException($"client {body.ClientId} not found");
        var e = new DbApp
        {
            Id = Guid.NewGuid(),
            TenantId = tid,
            ClientId = body.ClientId,
            Status = 1, // active
            // 5.18：expireTime? 可选——不传存 NULL（永久订阅），不再落 MinValue(0001-01-01)。
            ExpireTime = body.ExpireTime?.UtcDateTime,
            CreatedAt = DateTime.UtcNow,
        };
        _db.TenantApplications.Add(e);
        await _db.SaveChangesAsync();
        return ToDto(e);
    }
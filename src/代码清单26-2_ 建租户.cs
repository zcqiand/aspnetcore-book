    public override async Task<ApiTenant> TenantsPost(CreateTenantRequest body)
    {
        var e = new DbTenant
        {
            Id = Guid.NewGuid(),
            TenantKey = body.TenantKey,
            Name = body.Name,
            Status = 1, // active
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        _db.Tenants.Add(e);
        await _db.SaveChangesAsync();
        return ToDto(e);
    }
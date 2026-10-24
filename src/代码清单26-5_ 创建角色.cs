    public override async Task<ApiRole> RolesPost(string tenantId, CreateSysRoleRequest body)
    {
        _guard.VerifyPathTenant(tenantId);
        var e = new DbRole
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.Parse(tenantId),
            ClientId = body.ClientId,
            RoleCode = body.RoleCode,
            RoleName = body.RoleName,
            Description = body.Description,
            IsPreset = false,
            Status = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        _db.SysRoles.Add(e);
        await _db.SaveChangesAsync();
        return ToDto(e);
    }
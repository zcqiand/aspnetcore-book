    public override async Task<RoleMenuGrant> MenusPut(string tenantId, string roleId, string? clientId, SetSysRoleMenusRequest body)
    {
        _guard.VerifyPathTenant(tenantId);
        var id = Guid.Parse(roleId);
        var role = await _db.SysRoles
            .Include(r => r.Menus)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new KeyNotFoundException($"role {roleId} not found");

        var menuIds = (body.MenuIds ?? new List<string>())
            .Where(m => Guid.TryParse(m, out _))
            .Select(m => Guid.Parse(m))
            .ToHashSet();

        // EF skip nav：清空旧 nav，再加新的（AppDbContext HasMany.UsingEntity 自动维护 junction）
        role.Menus.Clear();
        if (menuIds.Count > 0)
        {
            var newMenus = await _db.SysMenus.Where(m => menuIds.Contains(m.Id)).ToListAsync();
            foreach (var m in newMenus) role.Menus.Add(m);
        }
        role.UpdatedAt = DateTime.UtcNow; // touch — 聚合 updatedAt 来源（家族约定）
        await _db.SaveChangesAsync();
        return ToGrant(role);
    }
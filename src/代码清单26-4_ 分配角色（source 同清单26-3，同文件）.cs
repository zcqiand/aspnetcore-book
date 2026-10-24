    public override async Task<DtoMemberView> Roles(string tenantId, string userId, SetTenantMemberRolesRequest body)
    {
        _guard.VerifyPathTenant(tenantId);
        var tid = Guid.Parse(tenantId);
        var member = await FindMemberAsync(tenantId, ParseGuid(userId), includeRoles: true)
            ?? throw new KeyNotFoundException("Member not found");
        var roleIds = ParseRoleIds(body.RoleIds);
        // 候选角色按 sys_role.tenant_id 过滤：跨租户 roleId 静默忽略，不给本租户成员挂别租户角色。
        var roles = roleIds.Count == 0
            ? new List<DbRole>()
            : await _db.SysRoles.Where(r => roleIds.Contains(r.Id) && r.TenantId == tid).ToListAsync();
        member.Roles.Clear();
        foreach (var r in roles) member.Roles.Add(r);
        member.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        var u = await _db.SysUsers.FirstAsync(x => x.Id == member.UserId);
        return ToFlatView(member, u);
    }
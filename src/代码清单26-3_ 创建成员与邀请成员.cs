    public override async Task<DtoMemberView> MembersPost(string tenantId, CreateSysUserRequest body)
    {
        _guard.VerifyPathTenant(tenantId);
        var tid = Guid.Parse(tenantId);
        var user = new DbUser
        {
            Id = Guid.NewGuid(),
            Username = body.Username,
            Password = body.Password != null ? $"plain:{body.Password}" : "",
            Email = body.Email,
            Status = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        _db.SysUsers.Add(user);
        var member = new DbMember
        {
            Id = Guid.NewGuid(),
            TenantId = tid,
            UserId = user.Id,
            Status = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        _db.TenantMembers.Add(member);
        await _db.SaveChangesAsync();

        await _audit.WriteAsync(
            tenantId,
            CallerUserId()?.ToString(),
            "user_created",
            targetUserId: user.Id.ToString(),
            new Dictionary<string, object?> { ["userId"] = user.Id.ToString() });
        var u = await _db.SysUsers.FirstAsync(x => x.Id == member.UserId);
        return ToFlatView(member, u);
    }

    public override async Task<DtoNestedMemberView> Invitations(string tenantId, Body3 body)
    {
        _guard.VerifyPathTenant(tenantId);
        var user = new DbUser
        {
            Id = Guid.NewGuid(),
            Username = body.Email ?? Guid.NewGuid().ToString(),
            Password = "",
            Email = body.Email,
            Status = 2, // invited（I42：invitation 建的是 status=2 invited 真 user 行，不是 1 active）
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        _db.SysUsers.Add(user);
        var member = new DbMember
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.Parse(tenantId),
            UserId = user.Id,
            MemberName = user.Username, // 家族约定：invitation 响应 memberName = username（= email）
            Status = 1, // member 立即 active（I42 oracle：user=invited, member=active）
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        _db.TenantMembers.Add(member);
        await _db.SaveChangesAsync();
        var u = await _db.SysUsers.FirstAsync(x => x.Id == member.UserId);
        return ToNestedView(member, u);
    }
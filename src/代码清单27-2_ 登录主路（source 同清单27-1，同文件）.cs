    public override async Task<LoginResponse> Login(LoginRequest body)
    {
        // M01.F04.I02 锁定检查（先查 — 即使密码对也不让锁定中账号登录）
        var username = body.Username ?? "";
        _failedLogins.EnsureNotLocked(username);

        // M01.F04.I03 账号密码登录
        var user = await _db.SysUsers.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null || string.IsNullOrEmpty(body.Password))
        {
            _failedLogins.RecordFailure(username);
            throw new UnauthorizedAccessException("invalid credentials");
        }

        // Phase 5：dev seed password 写成 "plain:{password}"；真实换 argon2
        var ok = user.Password == $"plain:{body.Password}" || user.Password == body.Password;
        if (!ok)
        {
            _failedLogins.RecordFailure(username);
            throw new UnauthorizedAccessException("invalid credentials");
        }
        // SysUser.Status 是 short（1=active，0/2=disabled/suspended）—— shared schema 演进
        // 9/7 重组后 status 列从 enum 改为 smallint，具体语义由 shared/src/db/seed.ts 定。
        if (user.Status != 1)
            throw new UnauthorizedAccessException("user disabled");

        // 成功 — 清失败计数 + 写 saas session cookie
        _failedLogins.ResetSuccess(user.Username);

        // sys_user 是 global 自然人，TenantId 在 tenant_member 上（M01.F04 9/7 重组）；
        // 选第一个 active membership 作为 currentTenantId。
        // ADR-0032：availableTenants 是真值 TenantMembership[] —— tenant_application ⨝
        // tenant_member（client_id 匹配、member status=1 active），roleIds 由 MembershipViews
        // 走 tenant_member_role ⨝ sys_role。clientId 未传或该 app 无订阅时退化为全部 active
        // membership（msw oracle：handlers-extra login 只按 userId+active 过滤）。
        var membersQuery = _db.TenantMembers
            .Include(tm => tm.Roles)
            .Where(tm => tm.UserId == user.Id && tm.Status == 1);
        if (!string.IsNullOrEmpty(body.ClientId))
        {
            var appTenantIds = await _db.TenantApplications
                .Where(ta => ta.ClientId == body.ClientId)
                .Select(ta => ta.TenantId)
                .ToListAsync();
            if (appTenantIds.Count > 0)
                membersQuery = membersQuery.Where(tm => appTenantIds.Contains(tm.TenantId));
        }
        var members = await membersQuery.ToListAsync();
        var currentTenantId = members.FirstOrDefault()?.TenantId ?? Guid.Empty;

        // M01.F04.I03 写端点副作用 — login_success（2026-09-02 contract-test M96 audit 覆盖对齐，
        // 形状对齐 nextjs/msw/springboot：actor=target=登录用户，metadata={username}）
        await _audit.WriteAsync(
            currentTenantId.ToString(),
            user.Id.ToString(),
            "login_success",
            targetUserId: null,
            new Dictionary<string, object?> { ["username"] = user.Username });
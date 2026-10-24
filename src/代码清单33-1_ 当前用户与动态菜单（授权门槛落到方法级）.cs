    // @entry M00.F01.I01 — 当前会话： claims 解析当前用户与租户（book anchor）
    [Authorize]
    public override Task<CurrentUserSession> GetCurrentUser() =>
        Task.FromResult(_service.Me(ReadClaims()));

    // @entry M01.F04.I01 — 动态菜单：按当前用户/租户返回菜单树（book anchor）
    [Authorize]
    public override Task<System.Collections.Generic.ICollection<MenuNode>> GetMenus() =>
        Task.FromResult<System.Collections.Generic.ICollection<MenuNode>>(_service.Menus(ReadClaims()));

    [Authorize]
    public override Task<PermissionSet> GetPermissions() =>
        Task.FromResult(_service.Permissions());
    // @entry M00.F02.I01 — 选租户换发：校验租户成员资格并重签含租户 claim 的 token（book anchor）
    [Authorize]
    public override Task<LoginResponse> SwitchTenant([FromBody] SwitchTenantRequest body) =>
        Task.FromResult(_service.SwitchTenant(ReadClaims(), body));

    private IReadOnlyDictionary<string, object> ReadClaims() =>
        User.Claims.ToDictionary(c => c.Type, c => (object)c.Value);
    // @entry M01.F05.I04 — 刷新 token：以 refresh token 换发新会话（book anchor）
    public override Task<LoginResponse> Refresh([FromBody] RefreshTokenRequest body) =>
        Task.FromResult(_service.Refresh(body));
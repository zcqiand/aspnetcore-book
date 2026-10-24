    // @entry M01.F05.I02 — SSO 跳转：授权端点，透传 state/redirect_uri（book anchor）
    /** M01.F05.I02 — RFC 6749 §4.1.1：透传前端 state/redirect_uri，写签名 state cookie。 */
    // 参数名必须与生成基类一致（response_type 等 snake_case）：模型绑定按实现方法
    // 的参数名取 query，改成 responseType 会让前端发的 ?response_type= 绑不上 → 400。
    public override Task<SsoRedirect> SsoAuthorize(
        [FromQuery] OAuthResponseType response_type,
        [FromQuery] string? client_id,
        [FromQuery] string? redirect_uri,
        [FromQuery] string? state)
    {
        var result = _service.SsoAuthorize(redirect_uri ?? "", state ?? "");
        AppendStateCookie(result.CookieValue);
        return Task.FromResult(result.Redirect);
    }
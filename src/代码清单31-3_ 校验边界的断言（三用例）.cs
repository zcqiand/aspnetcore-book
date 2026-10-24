    [Fact]
    public async Task MembersPost_shortPassword_returns400InvalidRequest()
    {
        var tenantId = SeedTenant($"t-msv-{Guid.NewGuid():N}");
        using var client = ClientFor(tenantId);
        var name = $"u-msv-short-{Guid.NewGuid():N}";

        // password 5 字符，契约 @minLength(8) —— 实现前真 200 建行（红）
        var resp = await client.PostAsync(
            $"api/v1/tenants/{tenantId}/members",
            Json($$"""{"username":"{{name}}","email":"{{name}}@x.io","password":"short"}"""));

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        var body = await BodyAsync(resp);
        Assert.Equal("INVALID_REQUEST", body.GetProperty("code").GetString());
    }

    [Fact]
    public async Task MembersPost_missingRequiredUsername_returns400()
    {
        var tenantId = SeedTenant($"t-msv-{Guid.NewGuid():N}");
        using var client = ClientFor(tenantId);

        // username 是 [Required] —— 缺失走同一 DTO 属性级校验路径
        var resp = await client.PostAsync(
            $"api/v1/tenants/{tenantId}/members",
            Json("""{"email":"nouser@example.com","password":"long-enough-8"}"""));

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        var body = await BodyAsync(resp);
        Assert.Equal("INVALID_REQUEST", body.GetProperty("code").GetString());
    }

    [Fact]
    public async Task MembersPost_validBody_minBoundaryPassword_stillSucceeds()
    {
        var tenantId = SeedTenant($"t-msv-{Guid.NewGuid():N}");
        using var client = ClientFor(tenantId);
        var name = $"u-msv-ok-{Guid.NewGuid():N}";

        // password 恰 8 字符（契约下限边界）—— filter 不得误伤合规请求
        var resp = await client.PostAsync(
            $"api/v1/tenants/{tenantId}/members",
            Json($$"""{"username":"{{name}}","email":"{{name}}@x.io","password":"12345678"}"""));

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var body = await BodyAsync(resp);
        Assert.Equal(name, body.GetProperty("username").GetString());
    }
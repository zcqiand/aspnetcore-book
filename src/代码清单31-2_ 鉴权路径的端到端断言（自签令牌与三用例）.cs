    private HttpClient ClientFor(Guid tenantId)
    {
        var jwt = _factory.Services.GetRequiredService<JwtIssuer>()
            .IssueAccessTokenForTest(Guid.NewGuid().ToString(), tenantId.ToString());
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", jwt);
        return client;
    }

    private static async Task<JsonElement> BodyAsync(HttpResponseMessage resp)
    {
        var text = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(text);
        return doc.RootElement.Clone();
    }

    [Fact]
    [Trait("Fn", "M00.F02.I04")]
    public async Task MembersPatch_existingMember_legalBody_returns200()
    {
        var (tenantId, userId, _) = SeedActiveMember($"u-patch-{Guid.NewGuid():N}");
        using var client = ClientFor(tenantId);

        // 合法 body：email/mobile + 契约外 status 字段 —— 5.13-① 起 UpdateSysUserRequest
        // 已无 status（状态唯一通道 /status），body 里的 "status":"active" 是未知属性，
        // 必须被容忍（200，System.Text.Json 扩展数据忽略），不得 400/改写成员状态。
        var resp = await client.PatchAsync(
            $"api/v1/tenants/{tenantId}/members/{userId}",
            new StringContent(
                """{"email":"patched@example.com","mobile":"13900000000","status":"active"}""",
                Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var body = await BodyAsync(resp);
        Assert.Equal("patched@example.com", body.GetProperty("email").GetString());
        // 5.34 converter 语义回显锁：响应 status 枚举必须序列化为 snake_case 字符串
        // （非泛型 JsonStringEnumConverter(SnakeCaseLower)，Program.cs 2026-09-12 修复），
        // 不是数字（1）也不是 PascalCase（"Active"）。
        Assert.Equal("active", body.GetProperty("status").GetString());
    }

    [Fact]
    [Trait("Fn", "M00.F02.I04")]
    public async Task MembersPatch_unknownUserId_returns404NotFound()
    {
        var (tenantId, _, _) = SeedActiveMember($"u-patch404-{Guid.NewGuid():N}");
        using var client = ClientFor(tenantId);
        var ghost = Guid.NewGuid(); // 合法 GUID，但 (tenantId, userId) 无 member 行

        var resp = await client.PatchAsync(
            $"api/v1/tenants/{tenantId}/members/{ghost}",
            new StringContent(
                """{"email":"ghost@example.com","status":"active"}""",
                Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        var body = await BodyAsync(resp);
        Assert.Equal("NOT_FOUND", body.GetProperty("code").GetString());
    }

    [Fact]
    [Trait("Fn", "M01.F04.I01")]
    public async Task Login_wrongPassword_returns401Unauthorized()
    {
        var (_, _, username) = SeedActiveMember($"u-login-{Guid.NewGuid():N}"); // 用户存在、密码错 → 真错密码分支
        using var client = _factory.CreateClient();

        var resp = await client.PostAsync(
            "api/v1/auth/login",
            new StringContent(
                $$"""{"username":"{{username}}","password":"definitely-wrong","clientId":""}""",
                Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
        var body = await BodyAsync(resp);
        Assert.Equal("UNAUTHORIZED", body.GetProperty("code").GetString());
    }
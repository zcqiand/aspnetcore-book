    /// 实际不解析格式（仅 opaque string）, 存 oauth_codes 表的 code 列。
    /// </summary>
    public static string GenerateRefreshToken(Guid userId)
    {
        var rand = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("=", "").Replace("+", "-").Replace("/", "_");
        return $"saas-rt-{userId}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}-{rand}";
    }
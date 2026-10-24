    /// <summary>
    /// 生成 refresh token。格式 saas-rt-{userId}-{ts}-{rand} —— 与 saas-nextjs
    /// lib/oauth-store.ts:97-99 同款, 方便 lab 仓与 saas-nextjs 共享 audit log 排障。
    /// 实际不解析格式（仅 opaque string）, 存 oauth_codes 表的 code 列。
    /// </summary>
    public static string GenerateRefreshToken(Guid userId)
    {
        var rand = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("=", "").Replace("+", "-").Replace("/", "_");
        return $"saas-rt-{userId}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}-{rand}";
    }
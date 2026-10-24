    public override Task Logout()
    {
        // M01.F04.I06/I06 登出（无状态 JWT 仅前端清 cookie）
        // 2026-08-31 contract-test M96.F02.I23：无返回 action ASP.NET 默认给 200 空体，
        // 家族契约（msw/springboot/nextjs）logout 是 204 noContent —— 显式对齐。
        Response.StatusCode = StatusCodes.Status204NoContent;
        return Task.CompletedTask;
    }
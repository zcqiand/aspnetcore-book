    // @entry M05.F01.I03 + M05.F01.I04 + M05.F01.I06 — 仪表盘统计基础端点：核心指标卡（合格率等）+ 六阶段任务状态漏斗聚合（book anchor）
    public override Task<DashboardStats> GetDashboardStats() =>
        Task.FromResult(_service.GetDashboardStats(_tenantContext.TenantId));
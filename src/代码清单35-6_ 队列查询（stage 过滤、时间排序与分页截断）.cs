    internal static IQueryable<SampleReceipt> BuildFlowQueueQuery(
        LabDbContext db, string tenantId, FlowStatus stage, int pageSize) =>
        db.SampleReceipts
            .Where(r => r.TenantId == tenantId && r.FlowStatus == stage)
            .OrderBy(r => r.CreatedAt)
            .Take(Math.Clamp(pageSize <= 0 ? 50 : pageSize, 1, 200));
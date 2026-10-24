    // @entry M03.F03.I08 — 创建检测记录：必填字段映射落库（book anchor）
    public override Task<TestRecord> CreateTestRecord([FromBody] CreateTestRecordRequest body) =>
        Task.FromResult(_service.Create(_tenantContext.TenantId, body));
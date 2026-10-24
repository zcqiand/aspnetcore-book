    // @entry M03.F03.I03 — 创建样品：ext 缺省空字典，接样缺失拦截（book anchor）
    public override Task<Sample> CreateSample([FromBody] CreateSampleRequest body) =>
        Task.FromResult(_service.Create(_tenantContext.TenantId, body));
    // @entry M02.F01.I02 — 合同详情：缺失抛 404 语义（book anchor）
    public override Task<Contract> GetContract(string id) =>
        Task.FromResult(_service.Get(_tenantContext.TenantId, id));
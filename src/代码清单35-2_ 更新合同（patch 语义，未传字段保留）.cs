    // @entry M02.F01.I04 — 更新合同：patch 语义，未传字段保留（book anchor）
    public override Task<Contract> UpdateContract(string id, [FromBody] UpdateContractRequest body) =>
        Task.FromResult(_service.Update(_tenantContext.TenantId, id, body));
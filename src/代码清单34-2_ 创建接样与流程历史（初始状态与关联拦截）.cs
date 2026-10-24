    // @entry M03.F01.I03 — 创建接样：初始化 receiving 态+空流程历史，合同缺失拦截（book anchor）
    public override Task<SampleReceipt> CreateReceipt([FromBody] CreateSampleReceiptRequest body) =>
        Task.FromResult(_service.Create(_tenantContext.TenantId, body));

    public override Task<SampleReceipt> GetReceipt(string id) =>
        Task.FromResult(_service.Get(_tenantContext.TenantId, id));

    public override Task<SampleReceipt> UpdateReceipt(string id, [FromBody] UpdateSampleReceiptRequest body) =>
        Task.FromResult(_service.Update(_tenantContext.TenantId, id, body));

    public override Task DeleteReceipt(string id)
    {
        _service.Delete(_tenantContext.TenantId, id);
        return Task.CompletedTask;
    }

    // @entry M03.F01.I06 — 流程历史：按接样返回 FlowHistoryEntry 列表（book anchor）
    public override Task<System.Collections.Generic.ICollection<FlowHistoryEntry>> GetReceiptHistory(string id) =>
        Task.FromResult<System.Collections.Generic.ICollection<FlowHistoryEntry>>(
            _service.History(_tenantContext.TenantId, id).ToList());
    // @entry M04.F06.I02 — 型号新增：创建/修改时间戳相等初始化（book anchor）
    public override Task<InspectionModel> CreateModel([FromBody] CreateCatalogEntryRequest body) =>
        Task.FromResult(_service.CreateModel(_tenantContext.TenantId, body));
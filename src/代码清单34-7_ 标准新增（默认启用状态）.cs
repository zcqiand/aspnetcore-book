    // @entry M06.F04.I03 — 标准新增：默认启用状态（book anchor）
    public override Task<InspectionStandard> CreateStandard([FromBody] CreateInspectionStandardRequest body) =>
        Task.FromResult(_service.CreateStandard(body));
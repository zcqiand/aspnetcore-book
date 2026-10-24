    // @entry M06.F03.I03 — 参数新增：official 默认与空 aliases 语义（book anchor）
    public override Task<InspectionParameter> CreateParameter([FromBody] CreateInspectionParameterRequest body) =>
        Task.FromResult(_service.CreateParameter(body));
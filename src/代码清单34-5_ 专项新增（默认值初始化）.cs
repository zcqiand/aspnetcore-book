    // @entry M06.F01.I03 — 专项新增：默认值初始化（book anchor）
    public override Task<InspectionSpecialty> CreateSpecialty([FromBody] CreateInspectionSpecialtyRequest body) =>
        Task.FromResult(_service.CreateSpecialty(body));
    // @entry M06.F02.I03 — 项目新增：字段映射，专项缺失 restrict 拦截（book anchor）
    public override Task<InspectionObject> CreateObject([FromBody] CreateInspectionObjectRequest body) =>
        Task.FromResult(_service.CreateObject(body));
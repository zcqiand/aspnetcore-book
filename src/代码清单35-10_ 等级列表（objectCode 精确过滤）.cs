    // @entry M04.F08.I01 — 等级列表：objectCode 精确过滤（book anchor）
    public override Task<Response2> ListGrades(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] string? inspectionObjectCode,
        [FromQuery] string? keyword)
    {
        var items = _service.ListGrades(_tenantContext.TenantId, inspectionObjectCode, keyword).ToList();
        int count = items.Count;
        return Task.FromResult(new Response2
        {
            Items = items,
            Page = page ?? 1,
            PageSize = pageSize ?? count,
            Total = count,
        });
    }
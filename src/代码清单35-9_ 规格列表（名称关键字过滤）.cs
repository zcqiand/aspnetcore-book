    // @entry M04.F07.I01 — 规格列表：名称关键字过滤（book anchor）
    public override Task<Response4> ListSpecs(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] string? inspectionObjectCode,
        [FromQuery] string? keyword)
    {
        var items = _service.ListSpecs(_tenantContext.TenantId, inspectionObjectCode, keyword).ToList();
        int count = items.Count;
        return Task.FromResult(new Response4
        {
            Items = items,
            Page = page ?? 1,
            PageSize = pageSize ?? count,
            Total = count,
        });
    }
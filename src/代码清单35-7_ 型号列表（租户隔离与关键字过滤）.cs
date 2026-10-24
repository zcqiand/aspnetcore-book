    // === M04.F06 型号 ===

    // @entry M04.F06.I01 — 型号列表：租户隔离+关键字大小写不敏+排序（book anchor）
    public override Task<Response3> ListModels(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] string? inspectionObjectCode,
        [FromQuery] string? keyword)
    {
        var items = _service.ListModels(_tenantContext.TenantId, inspectionObjectCode, keyword).ToList();
        int count = items.Count;
        return Task.FromResult(new Response3
        {
            Items = items,
            Page = page ?? 1,
            PageSize = pageSize ?? count,
            Total = count,
        });
    }
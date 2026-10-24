{
    private readonly ContractService _service = service;
    private readonly ITenantContext _tenantContext = tenantContext;

    // @entry M02.F01.I01 — 合同列表：关键字（编码/项目名）+状态过滤（book anchor）
    public override Task<Response5> ListContracts(
        [FromQuery] int? page, [FromQuery] int? pageSize,
        [FromQuery] string? keyword, [FromQuery] ContractStatus? status)
    {
        var items = _service.List(_tenantContext.TenantId, keyword, status).ToList();
        return Task.FromResult(new Response5
        {
            Items = items,
            Page = page ?? 1,
            PageSize = pageSize ?? 20,
            Total = items.Count,
        });
    }

    // @entry M02.F01.I03 — 创建合同：生成 ID+默认启用（book anchor）
    public override Task<Contract> CreateContract([FromBody] CreateContractRequest body) =>
        Task.FromResult(_service.Create(_tenantContext.TenantId, body));
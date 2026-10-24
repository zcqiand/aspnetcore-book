[Authorize]
public sealed class ParamInterfacesController(DictionaryService service, JunctionService junction)
    : ParamInterfacesControllerBase
{
    private readonly DictionaryService _service = service;
    private readonly JunctionService _junction = junction;

    // @entry M06.F08.I01 — 参数界面列表：关键字过滤（book anchor）
    public override Task<Response14> ListParamInterfaces(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] string? keyword)
    {
        var items = _service.ListInterfaces(keyword).ToList();
        int count = items.Count;
        return Task.FromResult(new Response14
        {
            Items = items,
            Page = page ?? 1,
            PageSize = pageSize ?? items.Count,
            Total = count,
        });
    }
namespace EFLab;

// 实体类: 一个类对应一张表, 每个属性对应一列;
// EF Core 按约定把名为 Id 的属性认作主键, 不需要额外标注
public class Order
{
    public long Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Customer { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
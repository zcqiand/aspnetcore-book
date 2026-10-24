namespace MapLab;

// ── 手写路线:同一个映射,规则写成普通静态方法 ──
// 与下一节的规则类一字对应:改名、拍平、拼合、集合变形,每行都是显式代码
public static class ManualCustomerMapper
{
    public static CustomerDto ToDto(Customer source)
    {
        return new CustomerDto
        {
            Id = source.Id,
            Name = source.Name,
            // 改名:Email → ContactEmail
            ContactEmail = source.Email ?? string.Empty,
            // 拍平:Address?.City → HomeCity(空安全)
            HomeCity = source.Address?.City ?? string.Empty,
            // 组合:City + Street 拼成一行地址
            FullAddress = source.Address == null
                ? string.Empty
                : $"{source.Address.City} {source.Address.Street}",
            // 集合变形:Contacts(对象列表) → ContactPhones(电话字符串列表)
            ContactPhones = source.Contacts
                .Select(c => c.Phone ?? string.Empty)
                .ToList(),
        };
    }
}
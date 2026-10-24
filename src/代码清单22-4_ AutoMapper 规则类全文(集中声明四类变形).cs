using AutoMapper;

namespace MapLab;

// ── AutoMapper 路线:映射规则集中在一个 Profile 里 ──
// Id/Name 同名直搬交给约定;四个变形字段逐个 ForMember 显式交代
public class CustomerProfile : Profile
{
    public CustomerProfile()
    {
        CreateMap<Customer, CustomerDto>()
            // 改名:Email → ContactEmail
            .ForMember(d => d.ContactEmail, o => o.MapFrom(s => s.Email ?? string.Empty))
            // 拍平:Address.City → HomeCity(空安全;MapFrom 是表达式树,不能用 ?. 运算符)
            .ForMember(d => d.HomeCity, o => o.MapFrom(s =>
                s.Address == null ? string.Empty : s.Address.City))
            // 组合:City + Street 拼成一行地址
            .ForMember(d => d.FullAddress, o => o.MapFrom(s =>
                s.Address == null ? string.Empty : $"{s.Address.City} {s.Address.Street}"))
            // 集合变形:Contacts(对象列表) → ContactPhones(电话字符串列表)
            .ForMember(d => d.ContactPhones, o => o.MapFrom(s =>
                s.Contacts.Select(c => c.Phone ?? string.Empty).ToList()));
    }
}
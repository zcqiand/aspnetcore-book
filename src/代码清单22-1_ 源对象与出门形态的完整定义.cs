namespace MapLab;

// ── 源侧:客户模型(教学版,字段形态照 ch21 扩展口径:嵌套 Address + 集合 Contacts)──
// 内存态,零建库零连接串;此处的 Customer 是纯数据源,不挂任何验证注解
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public Address? Address { get; set; }
    public List<ContactChannel> Contacts { get; set; } = new();
}

public class Address
{
    public string? City { get; set; }
    public string? Street { get; set; }
}

public class ContactChannel
{
    public string? Phone { get; set; }
    public string? Label { get; set; }
}

// ── 目标侧:客户 DTO(出边界的形态,与源故意不同形,体现「非纯搬运」)──
// 一处改名:Email → ContactEmail;两处组合变形:FullAddress(City+Street 拼合)、
// ContactPhones(集合 Contacts 压成电话字符串列表);外加 Address?.City 的空安全拍平
public class CustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string HomeCity { get; set; } = string.Empty;
    public string FullAddress { get; set; } = string.Empty;
    public List<string> ContactPhones { get; set; } = new();
}
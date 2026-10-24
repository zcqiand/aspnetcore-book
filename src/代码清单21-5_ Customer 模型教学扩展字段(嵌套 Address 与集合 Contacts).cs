// 第21章教学扩展: 新增嵌套 Address 与集合 Contacts 两个字段
// (ch20 原有 Id/Name/Email 契约逐字不动), 为嵌套与集合校验提供练习场
public class Customer
{
    public int Id { get; set; }

    [Required(ErrorMessage = "客户名不能为空")]
    public string Name { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "邮箱格式不正确")]
    public string? Email { get; set; }

    // 第21章教学扩展: 联系地址(嵌套对象)
    public Address? Address { get; set; }

    // 第21章教学扩展: 备用联系方式(集合)
    public List<ContactChannel> Contacts { get; set; } = new();
}

// 第21章教学扩展: 联系地址(嵌套对象)
public class Address
{
    public string? City { get; set; }
    public string? Street { get; set; }
}

// 第21章教学扩展: 备用联系方式(集合元素)
public class ContactChannel
{
    public string? Phone { get; set; }
    public string? Label { get; set; }
}
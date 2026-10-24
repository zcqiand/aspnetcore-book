using ApiLab.Controllers;
using FluentValidation;

namespace ApiLab;

// ── 第21章: FluentValidation 规则集, 迁移并扩展 ch20 数据注解已有规则 ──
// 去留口径: ch20 的 [Required]/[EmailAddress] 注解保留不动
// (FluentValidation 叠加扩展, 不静默替换)。
public class CustomerValidator : AbstractValidator<Customer>
{
    public CustomerValidator()
    {
        // 与 ch20 注解同款规则(注解仍在, 此处为叠加复述)
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("客户名不能为空")
            .MaximumLength(50).WithMessage("客户名不能超过 50 个字符");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("邮箱格式不正确");

        // 嵌套模型: 地址对象给出即查, 内部属性逐条校验
        RuleFor(x => x.Address)
            .SetValidator(new AddressValidator()!);

        // 集合校验: 备用联系方式逐条过闸
        RuleForEach(x => x.Contacts)
            .SetValidator(new ContactChannelValidator());
    }
}

// 嵌套对象校验器(第21章教学扩展)
public class AddressValidator : AbstractValidator<Address>
{
    public AddressValidator()
    {
        RuleFor(x => x.City)
            .NotEmpty().WithMessage("联系城市不能为空")
            .MaximumLength(20).WithMessage("联系城市不能超过 20 个字符");

        RuleFor(x => x.Street)
            .MaximumLength(60).WithMessage("街道地址不能超过 60 个字符");
    }
}

// 集合元素校验器(第21章教学扩展)
public class ContactChannelValidator : AbstractValidator<ContactChannel>
{
    public ContactChannelValidator()
    {
        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("备用电话不能为空")
            .Matches(@"^\d{11}$").WithMessage("备用电话须为 11 位数字");

        RuleFor(x => x.Label)
            .NotEmpty().WithMessage("备用电话须带用途标签, 如「采购」");
    }
}
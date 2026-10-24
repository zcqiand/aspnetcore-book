// 头部 using 区新增两行:
using FluentValidation;
using FluentValidation.AspNetCore;

// 服务登记区, AddScoped<AuditLogActionFilter>() 之后新增:
// 第21章定案: FluentValidation.AspNetCore 自动验证接线 + 校验器批量登记
// (官方弃用状态见清单21-1 注, 真机实测可用; 路线取舍见正文三轮探测)
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CustomerValidator>();
// 文件位置: LifetimeLab/Services/ReportOptions.cs, 手动新建后放入, 与清单8-2 的 LifetimeReportService.cs 同目录同命名空间
// 与其他清单的关系: 属性名与清单9-1 的 Report 节键名一一对应, 校验特性在此声明、
// 由清单9-4 的 ValidateDataAnnotations 在启动时执行, 校验失败的异常最终被 /options 端点的 catch 接住
// 为什么需要这个类: 配置原始形态全是字符串, 手写转换与默认值散落在每个使用点,
// 选项模式把「配置长什么样」沉淀成一个可校验、可注入的类型, 错配置在启动那一刻就被拦下
// 新增 using 只有一个: DataAnnotations 特性([Required]/[Range])住在 System.ComponentModel.DataAnnotations,
// 该命名空间不在 Web SDK 的隐式 using 名单内, 缺了这行 [Required] 编译不过

using System.ComponentModel.DataAnnotations;

namespace LifetimeLab.Services;

// 自定义校验特性: [Required]/[Range] 只能表达「必填」「范围」这类通用规则,
// 「标题不许留占位标记」是本项目自己的规矩, 写成自定义特性随类声明、随绑定执行, 规矩跟着类型走
[AttributeUsage(AttributeTargets.Property)]
public sealed class RealTitleAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // 值缺失归 [Required] 管, 这里只管「内容是不是占位标记」, 两个特性职责不重叠
        if (value is not string title)
        {
            return ValidationResult.Success;
        }

        // 为什么盯住 TODO 与占位 两个词: 报表标题会印进 /export 的每一份输出,
        // 占位标题一旦绑进应用就静默出现在所有导出文件里, 比启动报错难看得多
        return title.Contains("TODO", StringComparison.OrdinalIgnoreCase) || title.Contains("占位")
            ? new ValidationResult($"Report:Title 是占位标题「{title}」, 启动前必须替换为真实标题。")
            : ValidationResult.Success;
    }
}

// 选项类: 属性必须公共可写(get/set), 绑定器按属性名与配置键逐一对位赋值(大小写无关),
// 这就是「属性名即配置键」的原因——重命名属性等于重命名配置键, 清单9-1 与本类必须一起动
public sealed class ReportOptions
{
    [Required]
    [RealTitle]
    public string Title { get; set; } = string.Empty;

    [Range(1, 1000)]
    public int MaxRows { get; set; }

    // 允许 0: 重试次数为 0 表示不重试, 是合法配置而非漏填
    [Range(0, 5)]
    public int RetryCount { get; set; }

    // 不加特性也能有默认值: 配置缺这个键时绑定器不动它, /export 拿到的就是 csv
    public string DefaultFormat { get; set; } = "csv";

    // 重写 ToString 让清单9-6 的三行输出直接可读: 三个接口各打一行, 逐字段肉眼对照
    public override string ToString() =>
        $"Title={Title}, MaxRows={MaxRows}, RetryCount={RetryCount}, DefaultFormat={DefaultFormat}";
}
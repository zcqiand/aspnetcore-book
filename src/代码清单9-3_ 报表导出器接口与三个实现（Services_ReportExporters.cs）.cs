// 文件位置: LifetimeLab/Services/ReportExporters.cs, 手动新建后放入, 与清单8-2 同目录同命名空间
// 一个文件装下 1 个接口 + 3 个实现, 体例与清单8-2 一致;
// 三个实现将由清单9-4 的 Scrutor 批量装配一次性注册, 本章不再手写任何导出器的 AddTransient
// 新增 using 有两个: JsonSerializer 住在 System.Text.Json, StringBuilder 住在 System.Text,
// 两个命名空间都不在 Web SDK 的隐式 using 名单内, 需显式引入
// (using System.Text.Json 只导入 System.Text.Json 自身, 不覆盖父级 System.Text);
// List/Linq 在隐式名单(System/System.Linq)里, 不用引入

using System.Text;
using System.Text.Json;

namespace LifetimeLab.Services;

// 导出器接口: 清单9-4 的 /export 端点不认识任何具体实现, 只认这份契约——
// FormatName 供格式分发比对, Export 把报表行拼成该格式正文, 两句合起来就是「一种导出格式」的全部定义
public interface IReportExporter
{
    // 格式名统一小写: 与 /export?format= 参数以及 Report:DefaultFormat 配置键直接做字符串比对
    string FormatName { get; }

    string Export(IEnumerable<string> rows);
}

// CSV 实现: 每行「行号,内容」; 内容里的半角逗号统一换成顿号,
// 为什么替换而不是转义: 教学示例不引 CSV 转义库, 先让读者亲眼看到「分隔符冲突」这个真实问题的最简处理
public sealed class CsvReportExporter : IReportExporter
{
    public string FormatName => "csv";

    public string Export(IEnumerable<string> rows)
    {
        // 空输入分支: rows 为 null 或一行都没有都算空报表, 返回只剩表头的结果而不是抛异常——
        // 「没有数据」是正常业务状态, 在导出器内部给它正常输出, 端点层就不必为它另写一路判断
        var materialized = rows?.ToList() ?? [];
        var builder = new StringBuilder();
        builder.AppendLine("index,content");
        for (var rowIndex = 0; rowIndex < materialized.Count; rowIndex++)
        {
            builder.AppendLine($"{rowIndex},{materialized[rowIndex].Replace(",", "、")}");
        }
        return builder.ToString();
    }
}

// JSON 实现: 把行集合整体序列化成 JSON 数组, 开缩进是为了让 JSON 结构在 curl 原始输出里更清楚;
// 默认编码器会把非 ASCII 字符转义成 \uXXXX, 中文显示为转义序列是真实行为(见清单9-6 的括注)
public sealed class JsonReportExporter : IReportExporter
{
    public string FormatName => "json";

    public string Export(IEnumerable<string> rows)
    {
        var materialized = rows?.ToList() ?? [];
        // 空集合序列化结果是 [], 与 CSV 空报表只剩表头的行为对齐:
        // 三种格式对「空报表」都给出了定义, 不留任何悬空分支
        return JsonSerializer.Serialize(materialized, new JsonSerializerOptions { WriteIndented = true });
    }
}

// 纯文本实现: 逐行编号加方括号前缀, 三种格式里唯一不加工行内容的一种
public sealed class TextReportExporter : IReportExporter
{
    public string FormatName => "text";

    public string Export(IEnumerable<string> rows)
    {
        var materialized = rows?.ToList() ?? [];
        if (materialized.Count == 0)
        {
            return "text: (空报表, 没有可导出的行)";
        }
        var builder = new StringBuilder();
        for (var rowIndex = 0; rowIndex < materialized.Count; rowIndex++)
        {
            builder.AppendLine($"[{rowIndex}] {materialized[rowIndex]}");
        }
        return builder.ToString();
    }
}
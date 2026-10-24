// 文件位置: ConfigLab/Program.cs, 整体替换 dotnet new web 模板生成的那份(完整文件, 可直接覆盖后运行)
// 与第 8/9 章的关系: 这是 ConfigLab 自己的 Program.cs, 与 FirstWebApp/LifetimeLab 的两份互不相干;
// 选项登记与启动校验的写法沿用第 9 章清单9-4 的定稿: 一行 AddOptions 加 Bind, 校验调用链在同一条 OptionsBuilder 上,
// 本章新增的是两个配置层(清单10-2/10-3)与用户机密层(清单10-4)怎么在这份文件里汇成一棵配置树
// 端口口径与第 8/9 章一致: 代码不写死端口, 由清单10-6 的 dotnet run --urls=http://localhost:5400 从命令行指定,
// 命令行参数在 URL 配置里优先级最高, 压过 launchSettings.json 模板随机分配的端口, 本章端口固定为 5400

// 新增 using 只有两个, 各管一件事:
// Microsoft.Extensions.Options 提供 IOptions<ReportOptions> 与 OptionsValidationException,
//   不在 Web SDK 的隐式 using 名单内, 缺了它 /config 端点的参数表编译不过;
// System.ComponentModel.DataAnnotations 提供 [Required]/[Range] 校验特性,
//   同样不在名单内, 缺了它文件末尾的选项类编译不过
// IConfiguration 与 StatusCodes 都在隐式名单(Microsoft.Extensions.Configuration / Microsoft.AspNetCore.Http)里, 不用引入
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// ===== 本章新增 1: 必需键的裸键 FailFast 检查(Build 之前) =====
// 与第 9 章 ValidateOnStart 的分工, 两层各拦一段, 谁也不替代谁:
// 本段管「键在不在」——裸键层, 直接按字符串键问配置树, 一份清单式报错一次列全;
// ValidateOnStart 管「值合不合法」——强类型契约层, 键都在但值越界(如 MaxRows=0)由它拦
// 为什么用裸键而不是选项类做这一步: 选项绑定要等 Build 之后才可解析, 而「缺键」这类问题
// 越早死越好, 所以把检查钉在 Build 之前, 应用根本不进入监听阶段
var requiredConfigKeys = new[] { "Report:Title", "Report:MaxRows" };
var missingConfigKeys = requiredConfigKeys
    .Where(configKey => string.IsNullOrEmpty(builder.Configuration[configKey]))
    .ToList();
if (missingConfigKeys.Count > 0)
{
    // 为什么一次性列全而不是发现第一个缺失就抛: 少写一个键、修一个键、再跑再炸, 来回折腾;
    // 缺几个、缺哪几个一眼看全, 修一轮就能启动
    throw new InvalidOperationException(
        $"缺少必需的配置键: {string.Join(", ", missingConfigKeys)}。" +
        "请检查 appsettings.json、appsettings.Development.json 或用户机密中的对应键。");
}

// ===== 本章新增 2: Report 选项绑定与启动校验(第 9 章清单9-4 同款: 一行 AddOptions 加 Bind, 校验调用链在同一条 OptionsBuilder 上) =====
// AddOptions 返回 OptionsBuilder, Bind 把 GetSection("Report") 圈出的 Report 节挂上绑定器;
// 此时树里已是清单10-2(基线)、清单10-3(环境层)、清单10-4(机密层)覆盖完成后的合并结果,
// 绑定器拿到的就是胜出值, 这份文件里没有任何一行代码需要知道「这个值来自哪一层」
builder.Services.AddOptions<ReportOptions>()
    .Bind(builder.Configuration.GetSection("Report"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

var app = builder.Build();

// ===== 实验端点: /config, 只读最终合并结果 =====
// 三个来源(基线层、环境层、机密层)登记进的是同一棵配置树, 这里读到的每一行
// 都是覆盖完成后的胜出值——端点是树的投影观察口, 不关心也不显示每个值来自哪一层,
// 「哪一层赢了」由清单10-6 逐层改动时对照得出
app.MapGet("/config", (IConfiguration configuration,
                       IOptions<ReportOptions> reportOptions) =>
{
    app.Logger.LogInformation("/config 请求进入, 输出各配置键的最终合并值");

    // 四个裸键行统一 PadRight 对齐, 肉眼逐行对照; 裸键缺失时取到的是 null,
    // 统一显示成 (缺失) 而不是空行, 免得「键在输出里但值看不见」被误读成端点故障
    string FormatConfigLine(string configKey, string? configValue) =>
        $"{configKey.PadRight(21)} => {configValue ?? "(缺失)"}";

    try
    {
        var lines = new List<string>
        {
            $"环境                => {app.Environment.EnvironmentName}",
            FormatConfigLine("Report:Title", configuration["Report:Title"]),
            FormatConfigLine("Report:MaxRows", configuration["Report:MaxRows"]),
            FormatConfigLine("Report:RetryCount", configuration["Report:RetryCount"]),
            FormatConfigLine("Report:DefaultFormat", configuration["Report:DefaultFormat"]),
            $"强类型视图          => {reportOptions.Value}"
        };
        return Results.Text(string.Join(Environment.NewLine, lines));
    }
    catch (OptionsValidationException validationError)
    {
        // 第一道门(ValidateOnStart)拦启动, 这层拦运行期: 若读者删掉 ValidateOnStart 做对照实验,
        // 错配置会延迟到第一次 .Value 访问才抛 OptionsValidationException, 由这里接住回 500
        app.Logger.LogError(validationError, "Report 选项未通过校验");
        return Results.Text($"Report 选项未通过校验: {string.Join("; ", validationError.Failures)}",
            statusCode: StatusCodes.Status500InternalServerError);
    }
    catch (Exception error)
    {
        // 兜底体例与第 8/9 章一致: 记下异常证据, 回 500 与一句人话, 不把堆栈泼给调用方
        app.Logger.LogError(error, "/config 读取配置失败");
        return Results.Text("读取配置失败, 请查看服务端日志。",
            statusCode: StatusCodes.Status500InternalServerError);
    }
});

// 兜底: 没人认领的请求统一落到这里, 规矩与第 8/9 章相同
app.MapFallback(() => Results.NotFound("你访问的路径不存在, 先试试 /config 。"));

app.Run();

// ===== 选项类: 简洁版, 属性与清单10-2 的 Report 节键名一一对应 =====
// 校验特性沿用第 9 章口径: [Required] 管 Title 必填, [Range] 管两个数值的合法区间;
// DefaultFormat 不加特性、用属性默认值 "csv" 兜底缺键场景, 口径与第 9 章清单9-2 一致
public sealed class ReportOptions
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Range(1, 1000)]
    public int MaxRows { get; set; }

    // 允许 0: 不重试是合法配置而非漏填
    [Range(0, 5)]
    public int RetryCount { get; set; }

    public string DefaultFormat { get; set; } = "csv";

    // 重写 ToString 让 /config 的强类型视图一行直接可读, 与四个裸键行逐字段肉眼对照
    public override string ToString() =>
        $"Title={Title}, MaxRows={MaxRows}, RetryCount={RetryCount}, DefaultFormat={DefaultFormat}";
}
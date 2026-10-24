// 文件位置: LifetimeLab/Services/LifetimeReportService.cs, 手动新建 Services 子目录后放入
// 一个文件装下 1 个接口 + 1 个基类 + 3 个实现; 命名空间用文件作用域写法(C# 10 起支持, net8.0 默认风格)
// Guid 与 DateTimeOffset 都在 Web SDK 的隐式 using 名单(System 命名空间)里, 无需额外引入
namespace LifetimeLab.Services;

// 公共报告契约: 本章的服务注册按具体类各自注册(见清单8-3), 接口本身不参与注册与解析,
// 保留它只为把三种实现的输出形态钉成一套——Describe 的格式、Instance ID 的取法都从这条契约与下面的基类来
public interface ILifetimeReporter
{
    // 生命周期名保留英文原名, 便于与注册方法名(AddSingleton/AddScoped/AddTransient)逐字对上
    string LifetimeName { get; }

    // 实例短 ID: 本章肉眼对照实例异同的统一格式, GUID 取前 8 位十六进制
    string InstanceId { get; }

    string Describe();
}

// 公共基类: ID 与创建时刻都在构造那一刻生成, 之后只读
// 「出生即定格」是实验成立的前提, 实例 ID 变不变, 完全取决于容器在什么时候新建它, 而不是类自己能决定的
public abstract class LifetimeReporterBase : ILifetimeReporter
{
    // ToString("N") 产出 32 位无连字符十六进制, 截前 8 位便于在控制台输出里肉眼比对
    public string InstanceId { get; } = Guid.NewGuid().ToString("N")[..8];

    // 记录创建时刻, 用于验证三次请求里 Singleton 的创建时刻始终停在应用首次用到它的那一瞬
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.Now;

    public abstract string LifetimeName { get; }

    public string Describe() => $"{LifetimeName} 实例 ID={InstanceId}(创建于 {CreatedAt:HH:mm:ss.fff})";
}

// 单例: 全应用一个实例, 构造发生在应用第一次用到它时, 之后一直活到进程退出
public sealed class SingletonReportService : LifetimeReporterBase
{
    public override string LifetimeName => "Singleton";
}

// 作用域: 每个请求一个实例, 请求结束即作废
public sealed class ScopedReportService : LifetimeReporterBase
{
    public override string LifetimeName => "Scoped";
}

// 暂时: 每次解析都新建一个实例, 谁要给谁一个新的
public sealed class TransientReportService : LifetimeReporterBase
{
    public override string LifetimeName => "Transient";
}
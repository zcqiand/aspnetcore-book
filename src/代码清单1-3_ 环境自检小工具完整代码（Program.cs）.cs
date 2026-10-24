using System.Reflection;

// 采用顶级语句（.NET 8 控制台模板默认形态），省去 Main 方法样板，少一层嵌套干扰

Console.WriteLine("== .NET 环境自检 ==");

// Environment.Version 返回的是执行本程序的 CLR 版本，而非 SDK 版本，两者不是一回事
Console.WriteLine($"CLR 版本  : {Environment.Version}");

// OSDescription 由运行时启动时自动探测，无需自己写平台判断代码
Console.WriteLine($"操作系统  : {System.Runtime.InteropServices.RuntimeInformation.OSDescription}");

// 目标框架在编译期被写进程序集特性里，从入口程序集读出来即为当初编译时的目标框架
var entryAssembly = Assembly.GetEntryAssembly();
var frameworkAttribute = entryAssembly?.GetCustomAttribute<TargetFrameworkAttribute>();
Console.WriteLine($"目标框架  : {frameworkAttribute?.FrameworkName ?? "(未识别)"}");

// args 是顶级语句内置的命令行参数数组；参数不足两个时给出用法提示后直接结束
if (args.Length < 2)
{
    Console.WriteLine("提示: 传入两个整数可体验除法演示, 例如 dotnet run -- 100 4");
    return 0;
}

try
{
    // 用 int 而非 double 是有意为之：整数除法除以零才抛 DivideByZeroException，
    // 浮点除零只会得到无穷大，触发不了错误处理分支，达不到演示目的
    int dividend = int.Parse(args[0]);
    int divisor = int.Parse(args[1]);

    Console.WriteLine($"{dividend} ÷ {divisor} = {dividend / divisor}");
}
catch (FormatException)
{
    // int.Parse 遇到非数字字符时抛出 FormatException，这是解析环节的兜底
    Console.WriteLine($"输入错误: \"{args[0]}\" 或 \"{args[1]}\" 不是有效整数。");
    return 1;
}
catch (DivideByZeroException)
{
    // 整数除法除数为零时抛出，这是计算环节的兜底，与解析兜底分开处理便于定位问题
    Console.WriteLine($"计算错误: 除数 {args[1]} 为零, 除法无法进行。");
    return 1;
}

return 0;
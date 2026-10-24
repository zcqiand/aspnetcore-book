// OrderPricerTests.cs —— 存放于 OrderFlow.Tests 项目, 整体替换模板占位文件 UnitTest1.cs。
// 测试项目已在清单3-2 中通过 dotnet add reference 同时引用了 Domain 与 App,
// 因此本文件可以直接 new Order 与 new OrderPricer, 无需再挂任何引用或包

using OrderFlow.Domain;

// ImplicitUsings 已开启, System 等常用命名空间由编译器自动补齐;
// xunit 模板另在 GlobalUsings.cs（见目录树）里全局引入了 Xunit, 这一行因此是双保险而非必需,
// 保留它让本文件脱离模板也能独立读懂; 全新工程里若两处都没有, [Fact] 特性将无法解析
using Xunit;

namespace OrderFlow.Tests;

// 测试类的价值不在「再写一遍校验」, 而在把清单3-4 的三段校验固化成机器可复验的断言:
// 正文与注释会过时、会有人手滑改坏, 但 dotnet test 每次构建都会把这三条路径重新走一遍,
// 任何一条校验被误删或改错类型, 测试立刻红, 这就是回归防线
public class OrderPricerTests
{
    // xunit 为每个测试方法都新建一个测试类实例, 字段不会在测试之间共享状态,
    // 所以放心在这里初始化被测对象, 不必担心上一个测试留下的副作用
    private readonly OrderPricer _pricer = new();

    [Fact]
    public void CalculateTotal_合法订单_返回单价乘以数量()
    {
        // 与清单3-5 主程序的合法订单同一份数据: 128.00 元 × 26,
        // 读者对照两份清单时数字能互相对上, 不必在两套口径之间来回换算
        Order order = new(1001, "阿里山茶行", 128.00m, 26);

        // decimal 的相等断言敢用精确比较而不留误差余量:
        // decimal 是十进制浮点, 128.00m * 26 无论算多少次都是同一个确定的 3328.00m,
        // 测试与实现跑的是同一个表达式, 不存在二进制浮点那种 0.1 加不准的累积误差
        Assert.Equal(3328.00m, _pricer.CalculateTotal(order));
    }

    [Fact]
    public void CalculateTotal_负金额_抛出ArgumentOutOfRange()
    {
        // 负单价订单与清单3-5 的非法订单同数据: 走的是清单3-4 的第三段校验
        Order order = new(1002, "云海咖啡豆", -640.00m, 3);

        // Assert.Throws 验证的是异常的精确类型: 只认 ArgumentOutOfRangeException 本身,
        // 命中它即证明「金额为负抛 ArgumentOutOfRangeException」这条分支被准确走到,
        // 若有人把抛出类型改成别的异常或干脆删掉校验, 这里当场失败
        Assert.Throws<ArgumentOutOfRangeException>(() => _pricer.CalculateTotal(order));
    }

    [Fact]
    public void CalculateTotal_空白客户名_抛出Argument()
    {
        // 客户名是三个空格而非空串: 清单3-4 特意用 IsNullOrWhiteSpace 挡纯空格,
        // 这个测试用例就是把「纯空格也算非法」这条容易被漏掉的边界钉死成断言
        Order order = new(1003, "   ", 210.00m, 2);

        // 注意 ArgumentOutOfRangeException 也派生自 ArgumentException,
        // 但 Assert.Throws 按精确类型匹配, 不会把上一个测试的异常类型放进来,
        // 两个异常测试因此各守各的分支, 互不串门
        Assert.Throws<ArgumentException>(() => _pricer.CalculateTotal(order));
    }
}
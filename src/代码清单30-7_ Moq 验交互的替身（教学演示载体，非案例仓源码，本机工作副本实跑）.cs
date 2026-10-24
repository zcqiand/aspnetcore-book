using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Saas.Identity.AspNetCore.Infrastructure.Audit;
using Xunit;

namespace Saas.Identity.AspNetCore.Tests.Teaching;

/// <summary>
/// 教学演示载体（Moq 验交互的替身），非案例仓源码——不入仓不提交。
/// saas 测试工程装了 Moq 但全树零使用（装而未用，本载体把它用起来）。
/// 对照手写替身（StubTenantContext 等只供返回值）：Setup 定返回、Verify 验交互。
/// </summary>
public class MockAuditWriterTeachingTest
{
    [Fact]
    public async Task Setup_returns_and_Verify_calledOnce()
    {
        var mock = new Mock<IAuditWriter>();
        mock.Setup(w => w.WriteAsync(
                "TENANT-001", "u-1", "login.success", "u-2",
                It.IsAny<IDictionary<string, object?>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await mock.Object.WriteAsync(
            "TENANT-001", "u-1", "login.success", "u-2",
            new Dictionary<string, object?>(), CancellationToken.None);

        // 验交互：这个调用发生了，且恰好一次
        mock.Verify(w => w.WriteAsync(
            "TENANT-001", "u-1", "login.success", "u-2",
            It.IsAny<IDictionary<string, object?>>(), It.IsAny<CancellationToken>()), Times.Once);
        // 验交互：另一个动作一次都没发生
        mock.Verify(w => w.WriteAsync(
            "TENANT-001", "u-1", "password.reset", "u-2",
            It.IsAny<IDictionary<string, object?>>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
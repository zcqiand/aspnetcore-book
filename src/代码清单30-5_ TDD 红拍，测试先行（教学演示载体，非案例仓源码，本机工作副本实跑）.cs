using System;
using System.Threading;
using Saas.Identity.AspNetCore.Security;
using Xunit;

namespace Saas.Identity.AspNetCore.Tests.Teaching;

/// <summary>
/// 教学演示载体（TDD 红-绿-重构），非案例仓源码——不入仓不提交。
/// RemainingLockout：未锁定/无记录返 null；锁定中返 UnlockAt-UtcNow 正值；到期归零不为负。
/// </summary>
public class FailedLoginStoreRemainingLockoutTest
{
    [Fact]
    public void UnknownOrNotLocked_returnsNull()
    {
        var store = new FailedLoginStore();
        Assert.Null(store.RemainingLockout("nobody"));          // 无记录
        store.RecordFailure("alice");
        Assert.Null(store.RemainingLockout("alice"));           // 有计数未锁定
    }

    [Fact]
    public void Locked_returnsPositiveRemaining()
    {
        var store = new FailedLoginStore(maxAttempts: 5, lockoutDuration: TimeSpan.FromSeconds(10));
        for (int i = 0; i < 5; i++) store.RecordFailure("bob");

        var remaining = store.RemainingLockout("bob");
        Assert.NotNull(remaining);
        Assert.True(remaining > TimeSpan.Zero);
        Assert.True(remaining <= TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void Expired_remainingNeverNegative_andLazyUnlockGivesNull()
    {
        var store = new FailedLoginStore(maxAttempts: 5, lockoutDuration: TimeSpan.FromMilliseconds(100));
        for (int i = 0; i < 5; i++) store.RecordFailure("carol");

        var deadline = DateTime.UtcNow.AddMilliseconds(500);
        while (DateTime.UtcNow < deadline)
        {
            var remaining = store.RemainingLockout("carol");
            // 跨过到期瞬间连续采样：任何一次采样都不允许是负值
            if (remaining is { } r)
                Assert.True(r >= TimeSpan.Zero);
            Thread.Sleep(10);
        }
        // 惰性解锁生效后：等同未锁定，返 null
        Assert.Null(store.RemainingLockout("carol"));
    }
}
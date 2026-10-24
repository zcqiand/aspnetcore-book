using System;
using Saas.Identity.AspNetCore.Security;

namespace Saas.Identity.AspNetCore.Tests.Teaching;

/// <summary>
/// 教学演示载体（TDD 绿拍与重构），非案例仓源码——不入仓不提交。
/// FailedLoginStore 是 sealed 类且 LockedUntil 为私有态，扩展方法借
/// EnsureNotLocked 抛出的 AccountLockedException.UnlockAt 拿到解锁时刻。
/// </summary>
public static class FailedLoginStoreTeachingExtensions
{
    public static TimeSpan? RemainingLockout(this FailedLoginStore store, string userId)
    {
        try
        {
            store.EnsureNotLocked(userId);
            return null;
        }
        catch (AccountLockedException ex)
        {
            // 重构拍：边界整理——到期瞬间钳到零，不允许负值漏出去
            var remaining = ex.UnlockAt - DateTime.UtcNow;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }
    }
}
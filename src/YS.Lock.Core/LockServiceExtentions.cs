﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace YS.Lock
{
    [SuppressMessage("Design", "CA1062:验证公共方法的参数", Justification = "<挂起>")]
    public static class LockServiceExtentions
    {
        public static Task<bool> Lock(this ILockService lockService, string key, TimeSpan timeSpan)
        {
            return lockService.Lock(key, key, timeSpan);
        }

        public static Task<bool> UnLock(this ILockService lockService, string key)
        {
            return lockService.UnLock(key, key);
        }
        public static Task<bool> Update(this ILockService lockService, string key, TimeSpan timeSpan)
        {
            return lockService.Update(key, key, timeSpan);
        }
        public static async Task<bool> GlobalRunOnce(this ILockService lockService, string key, Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (await lockService.Lock(key, TimeSpan.MaxValue))
            {
                try
                {
                    action();
                    return true;
                }
                finally
                {
                    await lockService.UnLock(key);
                }
            }
            return false;
        }
        public static async Task WaitFor(this ILockService lockService, string key, int millisecondsDelayInLoop = 100)
        {
            while (true)
            {
                await Task.Delay(millisecondsDelayInLoop);
                var (exists, _) = await lockService.Query<string>(key);
                if (!exists)
                {
                    break;
                }
            }
        }
        public static async Task GlobalRunOnceOrWaitFor(this ILockService lockService, string key, Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            var executed = await lockService.GlobalRunOnce(key, action);
            if (executed == false)
            {
                await lockService.WaitFor(key);
            }
        }
    }
}

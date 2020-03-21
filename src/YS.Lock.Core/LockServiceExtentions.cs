using System;
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
    }
}

using System;
using System.Threading.Tasks;

namespace YS.Lock.Impl.Redis
{
    [ServiceClass]
    public class RedisLockService : ILockService
    {
        public Task<bool> Lock(string key, TimeSpan timeSpan)
        {
            throw new NotImplementedException();
        }
    }
}

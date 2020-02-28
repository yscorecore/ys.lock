using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YS.Lock
{
    public interface ILockService
    {
        Task<bool> Lock(string key, TimeSpan timeSpan);
    }
}

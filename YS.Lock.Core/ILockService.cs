using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YS.Lock
{
    public interface ILockService
    {
        Task<bool> Lock<T>(string key, T value, TimeSpan timeSpan);
    }
}

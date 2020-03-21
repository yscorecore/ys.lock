using System;
using System.Threading.Tasks;

namespace YS.Lock
{
    public interface ILockService
    {
        Task<bool> Lock<T>(string key, T token, TimeSpan timeSpan);

        Task<bool> Update<T>(string key, T token, TimeSpan timeSpan);

        Task<bool> UnLock<T>(string key, T token);

        Task<(bool Exists, T Token)> Query<T>(string key);
    }
}

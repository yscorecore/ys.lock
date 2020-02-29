using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace YS.Lock.Impl.Memory
{
    [ServiceClass(typeof(ILockService), ServiceLifetime.Singleton)]
    public class MemoryLockService : ILockService
    {
        public MemoryLockService(IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache;
        }
        private IMemoryCache memoryCache;
        public Task<bool> Lock<T>(string key, T token, TimeSpan timeSpan)
        {
            // 双重否定加锁
            if (memoryCache.TryGetValue(key, out _))
            {
                return Task.FromResult(false);
            }
            else
            {
                lock (this)
                {
                    if (memoryCache.TryGetValue(key, out _))
                    {
                        return Task.FromResult(false);
                    }
                    else
                    {
                        memoryCache.Set(key, ConvertToString(token), timeSpan);
                        return Task.FromResult(true);
                    }
                }
            }
        }

        public Task<(bool Exists, T Token)> Query<T>(string key)
        {
            if (this.memoryCache.TryGetValue(key, out string val))
            {
                return Task.FromResult((true, ConvertToType<T>(val)));
            }
            return Task.FromResult((false, default(T)));
        }

        public Task<bool> UnLock<T>(string key, T token)
        {
            if (this.memoryCache.TryGetValue<string>(key, out string val))
            {
                if (ConvertToString(token) == val)
                {
                    lock (this)
                    {
                        if (this.memoryCache.TryGetValue(key, out _))
                        {
                            this.memoryCache.Remove(key);
                            return Task.FromResult(true);
                        }
                    }
                }
            }
            return Task.FromResult(false);
        }

        public Task<bool> Update<T>(string key, T token, TimeSpan timeSpan)
        {
            if (memoryCache.TryGetValue(key, out string val))
            {
                if (ConvertToString(token) == val)
                {
                    lock (this)
                    {
                        if (this.memoryCache.TryGetValue(key, out _))
                        {
                            memoryCache.Set(key, ConvertToString(token), timeSpan);
                            return Task.FromResult(true);
                        }
                    }
                }
            }
            return Task.FromResult(false);
        }

        private string ConvertToString<T>(T value)
        {
            return typeof(T) == typeof(string) ?
                    value as string :
                    JsonSerializer.Serialize(value);
        }
        private T ConvertToType<T>(string str)
        {
            return typeof(T) == typeof(string) ?
                     (T)(object)str :
                     JsonSerializer.Deserialize<T>(str);
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using System.Text.Json;
namespace YS.Lock.Impl.Redis
{
    [ServiceClass(typeof(ILockService), ServiceLifetime.Singleton)]
    public sealed class RedisLockService : ILockService, IDisposable
    {
        public RedisLockService(IOptions<RedisLockOptions> options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            this.options = options.Value;
            this.Connection = new Lazy<ConnectionMultiplexer>(this.CreateConnection, true);
            this.Database = new Lazy<IDatabase>(() => this.Connection.Value.GetDatabase(), true);
        }
        private readonly RedisLockOptions options;
        private readonly Lazy<ConnectionMultiplexer> Connection;
        private readonly Lazy<IDatabase> Database;
        public Task<bool> Lock<T>(string key, T token, TimeSpan timeSpan)
        {
            var database = this.Database.Value;
            var lockKey = this.GetRedisKey(key);
            var lockValue = this.ConvertToString(token);
            return database.LockTakeAsync(lockKey, lockValue, timeSpan);
        }
        public Task<bool> UnLock<T>(string key, T token)
        {
            var database = this.Database.Value;
            var lockKey = this.GetRedisKey(key);
            var lockValue = this.ConvertToString(token);
            return database.LockReleaseAsync(lockKey, lockValue);
        }
        public Task<bool> Update<T>(string key, T token, TimeSpan timeSpan)
        {
            var database = this.Database.Value;
            var lockKey = this.GetRedisKey(key);
            var lockValue = this.ConvertToString(token);
            return database.LockExtendAsync(lockKey, lockValue, timeSpan);
        }
        public async Task<(bool Exists, T Token)> Query<T>(string key)
        {
            var database = this.Database.Value;
            var lockKey = this.GetRedisKey(key);
            var tokenValue = await database.LockQueryAsync(lockKey);
            bool hasToken = tokenValue.HasValue;
            return (hasToken, hasToken ? ConvertToType<T>(tokenValue) : default);
        }
        private RedisKey GetRedisKey(string key)
        {
            return options.LockKeyPrefix + key;
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
        private ConnectionMultiplexer CreateConnection()
        {
            return options.Configuration != null ?
                    ConnectionMultiplexer.Connect(options.Configuration) :
                    ConnectionMultiplexer.Connect(options.ConnectionString);
        }


#pragma warning disable CA1063 // 正确实现 IDisposable
        void IDisposable.Dispose()
#pragma warning restore CA1063 // 正确实现 IDisposable
        {
            if (this.Connection.IsValueCreated)
            {
                this.Connection.Value.Dispose();
            }
        }
    }
}

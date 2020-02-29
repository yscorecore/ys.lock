using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using System.Text.Json;
namespace YS.Lock.Impl.Redis
{
    [ServiceClass(typeof(ILockService), ServiceLifetime.Singleton)]
    public class RedisLockService : ILockService, IDisposable
    {
        public RedisLockService(IOptions<RedisOptions> options)
        {
            this.options = options.Value;
            this.Connection = new Lazy<ConnectionMultiplexer>(this.CreateConnection, true);
            this.Database = new Lazy<IDatabase>(() => this.Connection.Value.GetDatabase(), true);
        }
        private RedisOptions options;
        private Lazy<ConnectionMultiplexer> Connection;
        private Lazy<IDatabase> Database;
        public async Task<bool> Lock<T>(string key, T value, TimeSpan timeSpan)
        {
            var database = this.Database.Value;
            var lockKey = this.GetRedisKey(key);
            var lockValue = this.ConvertToString(value);
            return await database.StringSetAsync(lockKey, lockValue, timeSpan, When.NotExists, CommandFlags.None);
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

        private ConnectionMultiplexer CreateConnection()
        {
            return options.Configuration != null ?
                    ConnectionMultiplexer.Connect(options.Configuration) :
                    ConnectionMultiplexer.Connect(options.ConnectionString);
        }


        public void Dispose()
        {
            if (this.Connection.IsValueCreated)
            {
                this.Connection.Value.Dispose();
            }
        }
    }
}

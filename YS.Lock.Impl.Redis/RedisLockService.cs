using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
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
        public async Task<bool> Lock(string key, TimeSpan timeSpan)
        {
            var lockKey = this.GetRedisKey(key);
            return await this.Database.Value.StringSetAsync(lockKey, "value", timeSpan, When.NotExists, CommandFlags.None);
        }
        private RedisKey GetRedisKey(string key)
        {
            return options.LockKeyPrefix + key;
        }

        private ConnectionMultiplexer CreateConnection()
        {
            if (options.Configuration != null)
            {
                return ConnectionMultiplexer.Connect(options.Configuration);
            }
            else
            {
                return ConnectionMultiplexer.Connect(options.ConnectionString);
            }
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

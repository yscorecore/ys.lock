using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis;
namespace YS.Lock.Impl.Redis
{
    [ServiceClass]
    public class RedisLockService : ILockService
    {
        /// <summary>
        /// The lazy connection.
        /// </summary>
        private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            ConfigurationOptions configuration = new ConfigurationOptions
            {
                AbortOnConnectFail = false,
                ConnectTimeout = 5000,
            };

            configuration.EndPoints.Add("localhost", 6379);

            return ConnectionMultiplexer.Connect(configuration.ToString());
        });

        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <value>The connection.</value>
        public static ConnectionMultiplexer Connection => lazyConnection.Value;
        public RedisLockService()
        {

        }
        private HashSet<string> set = new HashSet<string>();
        public Task<bool> Lock(string key, TimeSpan timeSpan)
        {
            var database = Connection.GetDatabase();
            return database.StringSetAsync(key, "value", timeSpan, When.NotExists, CommandFlags.None);
        }
    }
}

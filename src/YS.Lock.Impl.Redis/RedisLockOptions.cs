using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace YS.Lock.Impl.Redis
{
    [OptionsClass("Redis")]
    public class RedisLockOptions
    {
        public string LockKeyPrefix { get; set; } = "Lock_";
        public string ConnectionString { get; set; } = "localhost";
        public ConfigurationOptions Configuration { get; set; }
    }
}

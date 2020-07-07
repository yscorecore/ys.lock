using System;
using System.Collections.Generic;
using System.Text;
using StackExchange.Redis;
using YS.Knife;

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

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YS.Knife.Test;

namespace YS.Lock.Impl.Redis
{
    [TestClass]
    public static class TestEnvironment
    {
        [AssemblyInitialize()]
        public static void Setup(TestContext _)
        {
            var availablePort = Utility.GetAvailableTcpPort(6379);
            StartContainer(availablePort);
            SetConnectionString(availablePort);
        }

        [AssemblyCleanup()]
        public static void TearDown()
        {
            DockerCompose.Down();
        }
        private static void StartContainer(uint port)
        {
            DockerCompose.Up(new Dictionary<string, object>
            {
                ["REDIS_PORT"] = port
            });
        }
        private static void SetConnectionString(uint port)
        {

            Environment.SetEnvironmentVariable("Redis__ConnectionString", $"localhost:{port}");
        }
    }
}

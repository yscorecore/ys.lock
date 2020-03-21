using Knife.Hosting;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace YS.Lock
{
    public abstract class LockServiceTest : KnifeHost
    {
        public LockServiceTest()
        {
            this.lockService = this.Get<ILockService>();
        }
        private readonly ILockService lockService;

        [TestMethod]
        public async Task CanLockSimpleTypes()
        {
            var key = RandomUtility.RandomVarName(16);
            await lockService.Lock(key + "string", "", TimeSpan.FromSeconds(2));
            await lockService.Lock(key + "long", 1L, TimeSpan.FromSeconds(2));
            await lockService.Lock(key + "dateTime", DateTime.Now, TimeSpan.FromSeconds(2));
        }


        [TestMethod]
        public async Task ShouldReturnTrueWhenLockGivenNewKey()
        {
            var key = RandomUtility.RandomVarName(16);
            var res = await lockService.Lock(key, "token", TimeSpan.FromSeconds(2));
            Assert.IsTrue(res);
        }
        [TestMethod]
        public async Task ShouldReturnFalseWhenLockGivenExistsKey()
        {
            var key = RandomUtility.RandomVarName(16);
            await lockService.Lock(key, "token", TimeSpan.FromSeconds(2));
            var res2 = await lockService.Lock(key, "token2", TimeSpan.FromSeconds(2));
            Assert.IsFalse(res2);
        }


        [TestMethod]
        public async Task ShouldNotModifyExpiryWhenReLockFailure()
        {
            var key = RandomUtility.RandomVarName(16);
            await lockService.Lock(key, "token", TimeSpan.FromSeconds(2));
            await Task.Delay(1500);
            var res2 = await lockService.Lock(key, "token", TimeSpan.FromSeconds(2));
            Assert.IsFalse(res2);
            await Task.Delay(800);
            var res3 = await lockService.Lock(key, "token", TimeSpan.FromSeconds(2));
            Assert.IsTrue(res3);
        }

        [TestMethod]
        public async Task ShouldReturnTrueWhenLockGivenExpiredKey()
        {
            var key = RandomUtility.RandomVarName(16);
            await lockService.Lock(key, "token", TimeSpan.FromSeconds(2));
            await Task.Delay(2200);
            var res2 = await lockService.Lock(key, "token", TimeSpan.FromSeconds(2));
            Assert.IsTrue(res2);
        }

        [TestMethod]
        public void ShouldOnlyOneSuccessWhenConcurrenLock()
        {
            DateTime dateTime = DateTime.Now;
            int loopCount = 15;
            var successCount = Enumerable.Range(0, 4).AsParallel()
                                .Select(async p => await RunStep(p, dateTime, loopCount))
                                .Sum(p => p.Result);
            Assert.AreEqual(loopCount, successCount);

        }
        private async Task<int> RunStep(int taskId, DateTime start, int count)
        {
            int successCount = 0;
            for (int i = 0; i < count; i++)
            {
                var key = start.AddSeconds(i).ToString("HHmmss", CultureInfo.InvariantCulture);
                var success = await lockService.Lock(key, "token", TimeSpan.FromMinutes(2));
                successCount += Convert.ToInt32(success);
                if (success)
                {
                    Console.WriteLine($"Task {taskId} locked {key}.");
                }
                await Task.Delay(1000);
            }
            return successCount;
        }

        [TestMethod]
        public async Task ShouldReturnFalseWhenUnlockNotExistsKey()
        {
            var key = RandomUtility.RandomVarName(16);
            var res = await lockService.UnLock(key, "token");
            Assert.IsFalse(res);
        }
        [TestMethod]
        public async Task ShouldReturnTrueWhenUnlockGivenExistsKeyAndValidToken()
        {
            var key = RandomUtility.RandomVarName(16);
            await lockService.Lock(key, "token", TimeSpan.FromSeconds(2));
            var res = await lockService.UnLock(key, "token");
            Assert.IsTrue(res);
        }
        [TestMethod]
        public async Task ShouldReturnFalseWhenUnlockGivenExistsKeyAndInvalidToken()
        {
            var key = RandomUtility.RandomVarName(16);
            await lockService.Lock(key, "token", TimeSpan.FromSeconds(2));
            var res = await lockService.UnLock(key, "invalidtoken");
            Assert.IsFalse(res);
        }
        [TestMethod]
        public async Task ShouldReturnTrueWhenLockAfterUnlock()
        {
            var key = RandomUtility.RandomVarName(16);
            await lockService.Lock(key, "token", TimeSpan.FromSeconds(2));
            await lockService.UnLock(key, "token");
            var res = await lockService.Lock(key, "token", TimeSpan.FromSeconds(2));
            Assert.IsTrue(res);
        }

        [TestMethod]
        public async Task ShouldReturnFalseWhenUpdateGivenNoExistsKey()
        {
            var key = RandomUtility.RandomVarName(16);
            var res = await lockService.Update(key, "token", TimeSpan.FromSeconds(2));
            Assert.IsFalse(res);
        }

        [TestMethod]
        public async Task ShouldReturnFalseWhenUpdateGivenExpiredKey()
        {
            var key = RandomUtility.RandomVarName(16);
            await lockService.Lock(key, "token", TimeSpan.FromSeconds(1));
            await Task.Delay(1500);
            var res = await lockService.Update(key, "token", TimeSpan.FromSeconds(2));
            Assert.IsFalse(res);
        }
        [TestMethod]
        public async Task ShouldReturnTrueWhenUpdateGivenExistsKeyAndInvalidToken()
        {
            var key = RandomUtility.RandomVarName(16);
            await lockService.Lock(key, "token", TimeSpan.FromSeconds(2));
            await Task.Delay(1500);
            var res = await lockService.Update(key, "invalidtoken", TimeSpan.FromSeconds(2));
            Assert.IsFalse(res);

        }
        [TestMethod]
        public async Task ShouldReturnTrueWhenUpdateGivenExistsKeyAndValidToken()
        {
            var key = RandomUtility.RandomVarName(16);
            await lockService.Lock(key, "token", TimeSpan.FromSeconds(2));
            await Task.Delay(1500);
            var res = await lockService.Update(key, "token", TimeSpan.FromSeconds(2));
            Assert.IsTrue(res);
            await Task.Delay(1500);
            var canReLock = await lockService.Lock(key, "token2", TimeSpan.FromSeconds(2));
            Assert.IsFalse(canReLock);
        }

        [TestMethod]
        public async Task ShouldReturnNoExistsWhenQueryGivenNewKey()
        {
            var key = RandomUtility.RandomVarName(16);
            var (exists, token) = await lockService.Query<string>(key);
            Assert.IsFalse(exists);
            Assert.AreEqual(default, token);
        }

        [TestMethod]
        public async Task ShouldReturnTokenWhenQueryGivenExistsKey()
        {
            var key = RandomUtility.RandomVarName(16);
            var time = DateTime.Now;
            await lockService.Lock(key, time, TimeSpan.FromSeconds(2));
            var (exists, token) = await lockService.Query<DateTime>(key);
            Assert.IsTrue(exists);
            Assert.AreEqual(time, token);
        }

        [TestMethod]
        public async Task ShouldNotAffectExpiryWhenQueryGivenExistsKey()
        {
            var key = RandomUtility.RandomVarName(16);
            await lockService.Lock(key, "token", TimeSpan.FromSeconds(2));
            await Task.Delay(1500);
            var (exists, _) = await lockService.Query<string>(key);
            Assert.IsTrue(exists);
            await Task.Delay(800);
            var (exists2, _) = await lockService.Query<string>(key);
            Assert.IsFalse(exists2);
        }
    }
}

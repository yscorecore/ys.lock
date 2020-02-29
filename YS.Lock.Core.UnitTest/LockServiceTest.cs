using Knife.Hosting;
using System;
using System.Linq;
using Xunit;
using System.Threading.Tasks;
namespace YS.Lock
{
    public abstract class LockServiceTest : KnifeHost
    {
        public LockServiceTest()
        {
            this.lockService = this.Get<ILockService>();
        }
        private ILockService lockService;

        [Fact]
        public async Task ShouldReturnTrueWhenLockGivenNewKey()
        {
            var key = RandomUtility.RandomVarName(16);
            var res = await lockService.Lock(key, TimeSpan.FromSeconds(2));
            Assert.True(res);
        }
        [Fact]
        public async Task ShouldReturnFalseWhenLockGivenExistsKey()
        {
            var key = RandomUtility.RandomVarName(16);
            var res = await lockService.Lock(key, TimeSpan.FromSeconds(2));
            var res2 = await lockService.Lock(key, TimeSpan.FromSeconds(2));
            Assert.False(res2);
        }


        [Fact]
        public async Task ShouldNotModifyExpiryWhenReLockFailure()
        {
            var key = RandomUtility.RandomVarName(16);
            var res = await lockService.Lock(key, TimeSpan.FromSeconds(2));
            await Task.Delay(1500);
            var res2 = await lockService.Lock(key, TimeSpan.FromSeconds(2));
            Assert.False(res2);
            await Task.Delay(500);
            var res3 = await lockService.Lock(key, TimeSpan.FromSeconds(2));
            Assert.True(res3);
        }

        [Fact]
        public async Task ShouldReturnTrueWhenLockGivenExpiredKey()
        {
            var key = RandomUtility.RandomVarName(16);
            var res = await lockService.Lock(key, TimeSpan.FromSeconds(2));
            await Task.Delay(2000);
            var res2 = await lockService.Lock(key, TimeSpan.FromSeconds(2));
            Assert.True(res2);
        }

        [Fact]
        public void ShouldOnlyOneSuccessWhenConcurrenLock()
        {
            DateTime dateTime = DateTime.Now;
            int loopCount = 15;
            var successCount = Enumerable.Range(0, 4).AsParallel()
                                .Select(async p => await RunStep(p, dateTime,loopCount))
                                .Sum(p => p.Result);
            Assert.Equal(loopCount, successCount);
        }
        private async Task<int> RunStep(int taskId, DateTime start, int count)
        {
            int successCount = 0;
            for (int i = 0; i < count; i++)
            {
                var key = start.AddSeconds(i).ToString("HHmmss");
                var success = await lockService.Lock(key, TimeSpan.FromMinutes(2));
                successCount += Convert.ToInt32(success);
                if (success)
                {
                    Console.WriteLine($"Task {taskId} locked {key}.");
                }
                await Task.Delay(1000);
            }
            return successCount;
        }
    }
}

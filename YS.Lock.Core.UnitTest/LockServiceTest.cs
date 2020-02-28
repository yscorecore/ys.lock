using Knife.Hosting;
using System;
using System.Linq;
using Xunit;
using System.Threading.Tasks;
namespace YS.Lock.Core.UnitTest
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
            Assert.False(res);
        }

        [Fact]
        public async Task ShouldReturnTrueWhenLockGivenExpiredKey()
        {
            var key = RandomUtility.RandomVarName(16);
            var res = await lockService.Lock(key, TimeSpan.FromSeconds(2));
            await Task.Delay(2000);
            var res2 = await lockService.Lock(key, TimeSpan.FromSeconds(2));
            Assert.True(res);
        }

        [Fact]
        public void ShouldOnlyOneSuccessWhenConcurrenLock()
        {
            var successCount = Enumerable.Range(0, 5).AsParallel()
                                .Select(async p => await RunStep(30))
                                .Sum(p => p.Result);
            Assert.Equal(30, successCount);
        }
        private async Task<int> RunStep(int count)
        {
            int successCount = 0;
            for (int i = 0; i < count; i++)
            {
                var key = DateTime.Now.ToString("HHmmss");
                var success = await lockService.Lock(key, TimeSpan.FromMinutes(2));
                successCount += Convert.ToInt32(success);
                if (success)
                {
                    Console.WriteLine($"{Task.CurrentId} locked {key}.");
                }
                await Task.Delay(1000);
            }
            return successCount;
        }
    }
}

using Zol.IGrains;
using System;
using System.Threading.Tasks;
using Zol.Common;

namespace Zol.Grains
{
    public class TestGrain : Orleans.Grain, ITestGrain
    {
        private int num = 1;

        public Task AddCount(string taskName)
        {
            //var account = this.GrainFactory.GetGrain<ITest>("");//通过GrainFactory访问其他grain

            Logger.Debug(taskName + "----" + num);
            num++;
            return Task.CompletedTask;
        }
    }
}

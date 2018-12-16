using Zol.IGrains;
using System;
using System.Threading.Tasks;
using Zol.Common;

namespace Zol.Grains
{
    public class Test : Orleans.Grain, ITest
    {
        public Task<string> SayHello(string who)
        {
            Logger.Debug("who is " + who);
            return Task.FromResult("I am " + who);
        }
    }
}

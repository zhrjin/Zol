using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zol.IGrains
{
    public interface ITestGrain : Orleans.IGrainWithIntegerKey
    {
        Task AddCount(string taskName);
    }
}

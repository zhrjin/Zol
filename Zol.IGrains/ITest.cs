using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zol.IGrains
{
    public interface ITest : Orleans.IGrainWithStringKey
    {
        Task<string> SayHello(string who);
    }
}

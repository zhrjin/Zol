using Zol.IGrains;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Zol.OrleansWebApi.Controllers
{
    public class ValuesController : ApiController
    {
        private IClusterClient client;

        public ValuesController(IClusterClient client)
        {
            this.client = client;
        }

        [HttpGet]
        public async Task<string> Get(string who)
        {
            var grain = client.GetGrain<ITest>(System.Guid.NewGuid().ToString());

           

            return await grain.SayHello(who);
        }
    }
}

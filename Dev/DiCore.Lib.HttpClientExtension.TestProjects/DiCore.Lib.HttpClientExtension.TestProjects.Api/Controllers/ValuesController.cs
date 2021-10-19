using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using DiCore.Lib.HttpClientExtension.TestProjects.Api.Models;
using Serilog;

namespace DiCore.Lib.HttpClientExtension.TestProjects.Api.Controllers
{
    [Authorize]
    public class ValuesController : ApiController
    {
        public ValuesController()
        {
            
        }
        // GET api/values
        public IEnumerable<string> Get()
        {
            Log.Verbose($"Get method. User: {User?.Identity?.Name} ({User?.Identity?.AuthenticationType})");
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public async Task<int> Put(int id, [FromBody]InputModel value)
        {
            Log.Verbose($"Put method. User: {User?.Identity?.Name} ({User?.Identity?.AuthenticationType})");
            return await Task.FromResult(id);
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}

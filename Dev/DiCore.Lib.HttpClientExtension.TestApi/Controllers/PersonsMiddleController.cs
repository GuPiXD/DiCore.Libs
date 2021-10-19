using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Principal;
using System.Threading.Tasks;
using DiCore.Lib.RestClient.TestCore.Api.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DiCore.Lib.HttpClientExtension;


namespace DiCore.Lib.RestClient.TestCore.Api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class PersonsMiddleController : ControllerBase
    {
        private readonly HttpClient client;
        private const string PersonsRoute = "personsremote";

        public PersonsMiddleController(IHttpClientFactory clientFactory)
        {
            client = clientFactory.CreateClient("impersonate");
        }

        [HttpGet]
        public ActionResult<IEnumerable<PersonItem>> Get()
        {
            if (!(User.Identity is WindowsIdentity))
                return Unauthorized();

            var items = client.Get<PersonItem[]>(PersonsRoute);
            return Ok(items.Data);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PersonItem>> Get(Guid id)
        {
            if (!(User.Identity is WindowsIdentity))
                return Unauthorized();
            var item = await client.GetAsync<PersonItem>(PersonsRoute, id);
            if (item == null)
            {
                return NotFound();
            }

            return Ok(item.Data);
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Post([FromBody] Person value)
        {
            if (!(User.Identity is WindowsIdentity))
                return Unauthorized();
            var id = (await client.PostAsync<Guid>(value, PersonsRoute)).Data;
            return Ok(id);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Guid>> Put(Guid id, [FromBody] Person value)
        {
            if (!(User.Identity is WindowsIdentity))
                return Unauthorized();
            await client.PutAsync(value, PersonsRoute, id);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Guid>> Delete(Guid id)
        {
            if (!(User.Identity is WindowsIdentity))
                return Unauthorized();
            await client.DeleteAsync(PersonsRoute, id);
            return Ok();
        }
    }
}
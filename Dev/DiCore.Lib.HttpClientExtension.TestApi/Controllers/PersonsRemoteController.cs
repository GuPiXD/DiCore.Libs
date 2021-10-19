using System;
using System.Collections.Generic;
using DiCore.Lib.RestClient.TestCore.Api.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DiCore.Lib.RestClient.TestCore.Api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class PersonsRemoteController : ControllerBase
    {
        private readonly IStorage storage;
        private readonly UserSettings userSettings;


        public PersonsRemoteController(IStorage storage, UserSettings userSettings)
        {
            this.storage = storage;
            this.userSettings = userSettings;
        }

        private bool CheckUser()
        {
            return string.IsNullOrWhiteSpace(userSettings.AcceptedUserName) || string.Equals(User.Identity.Name,
                       userSettings.AcceptedUserName, StringComparison.OrdinalIgnoreCase);
        }

        [HttpGet]
        public ActionResult<IEnumerable<PersonItem>> Get()
        {
            if (!CheckUser())
            {
                return Forbid();
            }

            var items = storage.GetPersons();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public ActionResult<PersonItem> Get(Guid id)
        {
            if (!CheckUser())
            {
                return Forbid();
            }
            var item = storage.GetPerson(id);
            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        [HttpPost]
        public ActionResult<Guid> Post([FromBody] Person value)
        {
            if (!CheckUser())
            {
                return Forbid();
            }
            var id = storage.InsertPerson(value);
            return Ok(id);
        }

        [HttpPut("{id}")]
        public ActionResult Put(Guid id, [FromBody] Person value)
        {
            if (!CheckUser())
            {
                return Forbid();
            }
            if (!storage.UpdatePerson(id, value))
            {
                return NotFound();
            }

            return Ok();
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(Guid id)
        {
            if (!CheckUser())
            {
                return Forbid();
            }
            if (!storage.DeletePerson(id))
            {
                return NotFound();
            }

            return Ok();
        }
    }
}
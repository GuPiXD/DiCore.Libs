using System;
using System.Collections.Generic;
using DiCore.Lib.RestClient.TestCore.Api.Repository;
using Microsoft.AspNetCore.Mvc;

namespace DiCore.Lib.RestClient.TestCore.Api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PersonsController : ControllerBase
    {
        private readonly IStorage storage;


        public PersonsController(IStorage storage)
        {
            this.storage = storage;
        }

        [HttpGet]
        public ActionResult<IEnumerable<PersonItem>> Get()
        {
            var items = storage.GetPersons();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public ActionResult<PersonItem> Get(Guid id)
        {
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
            var id = storage.InsertPerson(value);
            return Ok(id);
        }

        [HttpPut("{id}")]
        public ActionResult Put(Guid id, [FromBody] Person value)
        {
            if (!storage.UpdatePerson(id, value))
            {
                return NotFound();
            }

            return Ok();
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public ActionResult Delete(Guid id)
        {
            if (!storage.DeletePerson(id))
            {
                return NotFound();
            }

            return Ok();
        }
    }
}
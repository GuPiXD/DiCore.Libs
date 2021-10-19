using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Principal;
using System.Threading.Tasks;
using DiCore.Lib.HttpClientExtension;
using DiCore.Lib.RestClient.TestCore.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace DiCore.Lib.RestClient.TestCore.Api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class FilesMiddleController : ControllerBase
    {
        private readonly HttpClient client;
        private const string FilesRoute = "filesremote";
        private const string ManyPath = "many";
        private const string ObjectPath = "object";

        private string GetContentType(string fileName)
        {
            new FileExtensionContentTypeProvider().TryGetContentType(fileName, out var contentType);
            return contentType ?? "application/octet-stream";
        }


        public FilesMiddleController(IHttpClientFactory clientFactory)
        {
            client = clientFactory.CreateClient("impersonate");
        }


        [HttpGet("{id}")]
        public async Task<ActionResult> Get(Guid id)
        {
            if (!(User.Identity is WindowsIdentity))
                return Unauthorized();
            using (var memoryStream = new MemoryStream())
            {
                var result = await client.GetAsync(FilesRoute, memoryStream, id);
                memoryStream.Position = 0;
                return File(memoryStream.ToArray(), result.MediaType, result.FileName);
            }
        }

        [Consumes("multipart/form-data")]
        [HttpPost]
        public async Task<ActionResult<Guid>> Post([FromForm] IFormFile file)
        {
            if (!(User.Identity is WindowsIdentity))
                return Unauthorized();
            if (file == null)
            {
                return BadRequest("File is null");
            }

            using (var memoryStream = new MemoryStream())
            {
                file.CopyTo(memoryStream);
                memoryStream.Position = 0;
                var id = (await client.PostAsync<Guid>( new FileData(memoryStream, "file", file.FileName),
                    FilesRoute)).Data;
                return Ok(id);
            }
        }

        [Consumes("multipart/form-data")]
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(Guid id, [FromForm] IFormFile file)
        {
            if (!(User.Identity is WindowsIdentity))
                return Unauthorized();
            if (file == null)
            {
                return BadRequest("File is null");
            }

            using (var memoryStream = new MemoryStream())
            {
                file.CopyTo(memoryStream);
                memoryStream.Position = 0;
                await client.PutAsync(new FileData(memoryStream, "file", file.FileName),
                    FilesRoute, id);
                return Ok();
            }
        }

        [Consumes("multipart/form-data")]
        [HttpPost("many")]
        public async Task<ActionResult<TestDataMany>> PostMany([FromForm] string name, [FromForm] int len,
            [FromForm] double angle, [FromForm] Guid uid, [FromForm] IFormFile[] file)
        {
            if (!(User.Identity is WindowsIdentity))
                return Unauthorized();
            if (file == null)
            {
                return BadRequest("File is null");
            }

            var streams = new List<IMultipartData>()
            {
                new StringData("name", name),
                new StringData("len", len),
                new StringData("angle", angle),
                new StringData("uid", uid)
            };
            foreach (var formFile in file)
            {
                var memoryStream = new MemoryStream();
                formFile.CopyTo(memoryStream);

                memoryStream.Position = 0;
                streams.Add(new FileData(memoryStream, "file", formFile.FileName));
            }

            using (var multi = new MultipartCollection(streams))
            {
                var result = await client.PostAsync<TestDataMany>(multi, FilesRoute, ManyPath);
                return Ok(result.Data);
            }
        }

        [Consumes("multipart/form-data")]
        [HttpPost("object")]
        public async Task<ActionResult<TestDataMany>> PostObject([FromForm] ObjectFilesModel data)
        {
            if (!(User.Identity is WindowsIdentity))
                return Unauthorized();
            if (data == null)
            {
                return BadRequest("File is null");
            }

            using (var clientModel = data.ToClientModel())
            using (var multi = new MultipartCollection(clientModel))
            {
                var result = await client.PostAsync<TestDataMany>( multi, FilesRoute, ObjectPath);
                return Ok(result.Data);
            }
        }

        [Consumes("multipart/form-data")]
        [HttpPut("object/{id}")]
        public async Task<ActionResult<TestDataManyId>> PutObject(Guid id, [FromForm] ObjectFilesModel data)
        {
            if (!(User.Identity is WindowsIdentity))
                return Unauthorized();
            if (data == null)
            {
                return BadRequest("File is null");
            }

            using (var clientModel = data.ToClientModel())
            using (var multi = new MultipartCollection(clientModel))
            {
                var result = await client.PutAsync<TestDataManyId>( multi, FilesRoute, ObjectPath, id);
                return Ok(result.Data);
            }
        }
    }
}
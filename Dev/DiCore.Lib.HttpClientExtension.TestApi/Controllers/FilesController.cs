using System;
using System.Collections.Generic;
using System.IO;
using DiCore.Lib.RestClient.TestCore.Api.Models;
using DiCore.Lib.RestClient.TestCore.Api.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace DiCore.Lib.RestClient.TestCore.Api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IStorage storage;

        private string GetContentType(string fileName)
        {
            new FileExtensionContentTypeProvider().TryGetContentType(fileName, out var contentType);
            return contentType ?? "application/octet-stream";
        }


        public FilesController(IStorage storage)
        {
            this.storage = storage;
        }

        [HttpGet("{id}")]
        public ActionResult Get(Guid id)
        {
            var item = storage.GetFileData(id);
            if (item == null || item.Id == default(Guid))
            {
                return NotFound();
            }

            return File(item.Data, item.ContentType, item.FileName);
        }

        [Consumes("multipart/form-data")]
        [HttpPost]
        public ActionResult<Guid> Post([FromForm] IFormFile file)
        {
            if (file == null)
            {
                return BadRequest("File is null");
            }

            using (var memoryStream = new MemoryStream())
            {
                file.CopyTo(memoryStream);
                memoryStream.Position = 0;
                var item = new Repository.FileData()
                {
                    FileName = file.FileName,
                    Data = memoryStream.ToArray(),
                    ContentType = file.ContentType ?? GetContentType(file.FileName)
                };
                var id = storage.InsertFileData(item);

                return Ok(id);
            }
        }

        [Consumes("multipart/form-data")]
        [HttpPut("{id}")]
        public ActionResult Put(Guid id, [FromForm] IFormFile file)
        {
            if (file == null)
            {
                return BadRequest("File is null");
            }

            using (var memoryStream = new MemoryStream())
            {
                file.CopyTo(memoryStream);
                memoryStream.Position = 0;
                var item = new Repository.FileData()
                {
                    FileName = file.FileName,
                    Data = memoryStream.ToArray(),
                    ContentType = file.ContentType ?? GetContentType(file.FileName)
                };
                if (!storage.UpdateFileData(id, item))
                {
                    return NotFound();
                }

                return Ok();
            }
        }

        [Consumes("multipart/form-data")]
        [HttpPost("many")]
        public ActionResult<TestDataMany> PostMany([FromForm] string name, [FromForm] int len, [FromForm] double angle,
            [FromForm] Guid uid, [FromForm] IFormFile[] file)
        {
            var result = new TestDataMany()
            {
                Name = name,
                Len = len,
                Angle = angle,
                Uid = uid,
                Files = new Dictionary<string, string>()
            };
            foreach (var formFile in file)
            {
                using (var memoryStream = new MemoryStream())
                {
                    formFile.CopyTo(memoryStream);
                    memoryStream.Position = 0;
                    using (var streamReader = new StreamReader(memoryStream))
                    {
                        result.Files.Add(formFile.FileName, streamReader.ReadToEnd());
                    }
                }
            }

            return Ok(result);
        }

        [Consumes("multipart/form-data")]
        [HttpPost("object")]
        public ActionResult<TestDataMany> PostObject([FromForm] ObjectFilesModel data)
        {
            var result = new TestDataMany()
            {
                Name = data.Name,
                Len = data.Len,
                Angle = data.Angle,
                Uid = data.Uid,
                Flag = data.Flag,
                Timespan = data.Timespan,

                StringArray = data.StringArray
            };
            if (data.File != null)
            {
                result.Files = new Dictionary<string, string>();
                foreach (var formFile in data.File)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        formFile.CopyTo(memoryStream);
                        memoryStream.Position = 0;
                        using (var streamReader = new StreamReader(memoryStream))
                        {
                            result.Files.Add(formFile.FileName, streamReader.ReadToEnd());
                        }
                    }
                }
            }

            if (data.InnerModel != null)
            {
                result.InnerModel = new TestDataManyInner()
                {
                    Name = data.InnerModel.Name,
                    StringArray = data.InnerModel.StringArray,
                    Angle = data.InnerModel.Angle,
                    Flag = data.InnerModel.Flag,
                    Len = data.InnerModel.Len,
                    Timespan = data.InnerModel.Timespan,
                    Uid = data.InnerModel.Uid
                };
                if (data.InnerModel.File != null)
                {
                    result.InnerModel.Files = new Dictionary<string, string>();
                    foreach (var formFile in data.InnerModel.File)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            formFile.CopyTo(memoryStream);
                            memoryStream.Position = 0;
                            using (var streamReader = new StreamReader(memoryStream))
                            {
                                result.InnerModel.Files.Add(formFile.FileName, streamReader.ReadToEnd());
                            }
                        }
                    }
                }
            }

            return Ok(result);
        }

        [Consumes("multipart/form-data")]
        [HttpPut("object/{id}")]
        public ActionResult<TestDataManyId> PutObject(Guid id, [FromForm] ObjectFilesModel data)
        {
            var result = new TestDataManyId()
            {
                Id = id,
                Name = data.Name,
                Len = data.Len,
                Angle = data.Angle,
                Uid = data.Uid,
                Flag = data.Flag,
                Timespan = data.Timespan,
                StringArray = data.StringArray
            };
            if (data.File != null)
            {
                result.Files = new Dictionary<string, string>();
                foreach (var formFile in data.File)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        formFile.CopyTo(memoryStream);
                        memoryStream.Position = 0;
                        using (var streamReader = new StreamReader(memoryStream))
                        {
                            result.Files.Add(formFile.FileName, streamReader.ReadToEnd());
                        }
                    }
                }
            }

            if (data.InnerModel != null)
            {
                result.InnerModel = new TestDataManyInner()
                {
                    Name = data.InnerModel.Name,
                    StringArray = data.InnerModel.StringArray,
                    Angle = data.InnerModel.Angle,
                    Flag = data.InnerModel.Flag,
                    Len = data.InnerModel.Len,
                    Timespan = data.InnerModel.Timespan,
                    Uid = data.InnerModel.Uid
                };
                if (data.InnerModel.File != null)
                {
                    result.InnerModel.Files = new Dictionary<string, string>();
                    foreach (var formFile in data.InnerModel.File)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            formFile.CopyTo(memoryStream);
                            memoryStream.Position = 0;
                            using (var streamReader = new StreamReader(memoryStream))
                            {
                                result.InnerModel.Files.Add(formFile.FileName, streamReader.ReadToEnd());
                            }
                        }
                    }
                }
            }

            return Ok(result);
        }
    }
}
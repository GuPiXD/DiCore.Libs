using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiCore.Lib.HttpClientExtension;
using Microsoft.AspNetCore.Http;

namespace DiCore.Lib.RestClient.TestCore.Api.Models
{
    public class ObjectFilesInnerModel
    {
        public string Name { get; set; }
        public int Len { get; set; }
        public double Angle { get; set; }
        public Guid Uid { get; set; }
        public bool Flag { get; set; }
        public DateTime Timespan { get; set; }
        public IFormFile[] File { get; set; }
        public string[] StringArray { get; set; }

        protected IEnumerable<FileContent> MapIFormFile(IEnumerable<IFormFile> files)
        {
            if (files == null)
                yield break;
            foreach (var formFile in files)
            {
                var memoryStream = new MemoryStream();
                formFile.CopyTo(memoryStream);
                memoryStream.Position = 0;
                yield return new FileContent(formFile.FileName, memoryStream);
            }
        }

        public ObjectFilesInnerClientModel ToClientModel()
        {
            var clientModel = new ObjectFilesInnerClientModel()
            {
                Name = Name,
                Len = Len,
                Angle = Angle,
                Uid = Uid,
                Flag = Flag,
                Timespan = Timespan,
                StringArray = StringArray,
                File = MapIFormFile(File).ToArray()
            };
            return clientModel;
        }
    }
}
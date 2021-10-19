using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace DiCore.Lib.RestClient.TestCore.Api.Models
{
    public class ObjectFilesModel:ObjectFilesInnerModel
    {
        public ObjectFilesInnerModel InnerModel { get; set; }

        public new ObjectFilesClientModel ToClientModel()
        {
            var clientModel = new ObjectFilesClientModel()
            {
                Name = Name,
                Len = Len,
                Angle = Angle,
                Uid = Uid,
                Flag = Flag,
                Timespan = Timespan,
                StringArray = StringArray,
                File =MapIFormFile(File).ToArray(),
                InnerModel = InnerModel.ToClientModel()
            };
            return clientModel;


        }
    }
}
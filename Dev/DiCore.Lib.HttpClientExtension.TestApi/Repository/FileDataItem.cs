using System;

namespace DiCore.Lib.RestClient.TestCore.Api.Repository
{
    public class FileDataItem:FileData,IDbItem<Guid>
    {
        public Guid Id { get; set; }
    }
}
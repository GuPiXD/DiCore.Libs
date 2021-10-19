using System.IO;

namespace DiCore.Lib.Web
{
    public class FileResult
    {
        public FileResult(string fileName, string contentType, long contentLength, MemoryStream data)
        {
            FileName = fileName;
            ContentType = contentType;
            ContentLength = contentLength;
            Data = data;
        }

        public string FileName { get; }
        public string ContentType { get;  }
        public long ContentLength { get; }
        public MemoryStream Data { get;  }
    }
    
}

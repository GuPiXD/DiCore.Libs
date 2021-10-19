namespace DiCore.Lib.Web
{
    public class FileParameter
    {
        public FileParameter(byte[] file, string fileName, string contentType = null)
        {
            File = file;
            FileName = fileName;
            ContentType = contentType;
        }

        public byte[] File { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
    }
}

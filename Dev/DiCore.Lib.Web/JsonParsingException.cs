using System;

namespace DiCore.Lib.Web
{
    public class JsonParsingException : Exception
    {
        public JsonParsingException() : base()
        {

        }

        public JsonParsingException(string message) : base(message)
        {

        }

        public JsonParsingException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}

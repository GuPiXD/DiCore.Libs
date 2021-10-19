using System;
using System.Net;

namespace DiCore.Lib.Web
{
    public class WebClientException : Exception
    {
        public HttpStatusCode HttpStatusCode { get; }
        public string HttpStatusDescription { get; set; }
        public string MessageText { get; set; }
        public string Url { get; set; }

        public WebClientException() : base()
        {

        }

        public WebClientException(string message, string url = null) : base(message)
        {
            Url = url;
        }

        public WebClientException(string message, Exception innerException) : base(message, innerException)
        {

        }

        public WebClientException(string message, string url, Exception innerException) : base(message, innerException)
        {
            Url = url;
        }

        public WebClientException(HttpStatusCode httpStatusCode, string httpStatusDescription, string messageText,
            Exception innerException)
            : base(innerException.Message, innerException)
        {
            HttpStatusCode = httpStatusCode;
            HttpStatusDescription = httpStatusDescription;
            MessageText = messageText;
        }

        public WebClientException(HttpStatusCode httpStatusCode, string httpStatusDescription, string messageText,
            string url, Exception innerException)
            : base(innerException.Message, innerException)
        {
            HttpStatusCode = httpStatusCode;
            HttpStatusDescription = httpStatusDescription;
            MessageText = messageText;
            Url = url;
        }

        public override string ToString()
        {
            return $"{HttpStatusCode}---{MessageText}---{Url}-----\r\n {base.ToString()}";
        }
    }
}

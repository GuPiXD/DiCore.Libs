using System;
using System.Net;
using System.Net.Http.Headers;

namespace DiCore.Lib.HttpClientExtension
{
    /// <summary>
    /// Ответ сервера в виде файла
    /// </summary>
    public class FileResponse : Response
    {
        /// <inheritdoc />
        public FileResponse(HttpStatusCode statusCode, string reasonPhrase, Version version,
            HttpContentHeaders contentHeaders) : base(statusCode, reasonPhrase, version, null, contentHeaders)
        {
        }

        /// <summary>
        /// Тип содержимого
        /// </summary>
        public string MediaType => ContentHeaders?.ContentType?.MediaType;

        /// <summary>
        /// Размер файла
        /// </summary>
        public long? ContentLength => ContentHeaders?.ContentLength;

        /// <summary>
        /// Название файла
        /// </summary>
        public string FileName => ContentHeaders?.ContentDisposition?.FileName;
    }
}
using System;
using System.Net;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace DiCore.Lib.HttpClientExtension
{
    /// <summary>
    /// Ответ сервера с данными
    /// </summary>
    /// <typeparam name="T">Тип данных</typeparam>
    public class Response<T> : Response
    {
        private readonly Lazy<T> data;


        /// <summary>
        /// Данные
        /// </summary>
        public T Data => data.Value;

        /// <inheritdoc />
        public Response(HttpStatusCode statusCode, string reasonPhrase, Version version, string content,
            HttpContentHeaders contentHeaders) : base(statusCode, reasonPhrase, version, content, contentHeaders)
        {
            data = new Lazy<T>(() =>
                IsSuccessStatusCode 
                    ? JsonConvert.DeserializeObject<T>(Content)
                    : default(T));
        }
    }
}
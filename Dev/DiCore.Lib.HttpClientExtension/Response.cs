using System;
using System.Net;
using System.Net.Http.Headers;

namespace DiCore.Lib.HttpClientExtension
{
    /// <summary>
    /// Ответ сервера
    /// </summary>
    public class Response
    {
        /// <summary>
        /// Создание ответа
        /// </summary>
        /// <param name="statusCode">Статус HTTP</param>
        /// <param name="reasonPhrase">Сообщение HTTP</param>
        /// <param name="version">Версия HTTP</param>
        /// <param name="content">Текст сообщения</param>
        /// <param name="contentHeaders">Заголовки HTTP</param>
        public Response(HttpStatusCode statusCode, string reasonPhrase, Version version, string content,
            HttpContentHeaders contentHeaders)
        {
            StatusCode = statusCode;
            ReasonPhrase = reasonPhrase;
            Version = version;
            Content = content;
            ContentHeaders = contentHeaders;
        }


        /// <summary>
        /// Версия HTTP
        /// </summary>
        public Version Version { get; }

        /// <summary>
        /// Код статуса HTTP
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Сообщение HTTP
        /// </summary>
        public string ReasonPhrase { get; }

        /// <summary>
        /// Ответ успшене
        /// </summary>
        public bool IsSuccessStatusCode => (int) StatusCode >= 200 && (int) StatusCode <= 299;

        /// <summary>
        /// Произошла ошибка клиента
        /// </summary>
        public bool IsClientError => (int) StatusCode >= 400 && (int) StatusCode <= 499;

        /// <summary>
        /// Произошла ошибка сервера
        /// </summary>
        public bool IsServerError => (int) StatusCode >= 500 && (int) StatusCode <= 599;

        /// <summary>
        /// Текст сообщения
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// Заголовки HTTP
        /// </summary>
        public HttpContentHeaders ContentHeaders { get; }
    }
}
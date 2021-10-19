using System;
using System.Net.Http;
using System.Threading;
using DiCore.Lib.HttpClientExtension.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace DiCore.Lib.HttpClientExtension.DependencyInjection
{
    public static class HttpClientBuilderExtensions
    {
        /// <summary>
        /// Использовать учётные данные текущего потока
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IHttpClientBuilder UseDefaultCredentials(this IHttpClientBuilder builder)
        {
            return builder.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                UseDefaultCredentials = true,
                PreAuthenticate = true,
                AllowAutoRedirect = true,
                ClientCertificateOptions = ClientCertificateOption.Automatic
            });
        }

        /// <summary>
        /// Вызывать exception при возникновении ошибки клиента (4xx)
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IHttpClientBuilder ThrowClientErrors(this IHttpClientBuilder builder)
        {
            builder.AddHttpMessageHandler(_ => new ClientErrorsThrowHandler());
            return builder;
        }

        /// <summary>
        /// Вызывать exception при возникновении ошибки сервера (5xx)
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IHttpClientBuilder ThrowServerErrors(this IHttpClientBuilder builder)
        {
            builder.AddHttpMessageHandler(_ => new ServerErrorsThrowHandler());
            return builder;
        }

        /// <summary>
        /// Добавить обработчик запросов
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="handler">Обработчик</param>
        /// <returns></returns>
        public static IHttpClientBuilder HandleRequest(this IHttpClientBuilder builder,
            Action<HttpRequestMessage, CancellationToken> handler)
        {
            builder.AddHttpMessageHandler(_ => new CustomUserHandler(handler));
            return builder;
        }
    }
}
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DiCore.Lib.HttpClientExtension.Logging
{
    public static class HttpClientBuilderExtensions
    {
        public static IHttpClientBuilder AddMessageLoggingHandler<T>(this IHttpClientBuilder builder)
        {
            builder.Services.TryAddTransient<MessageLoggingHandler<T>>();
            return builder.AddHttpMessageHandler<MessageLoggingHandler<T>>();
        }

        public static IHttpClientBuilder AddMessageLoggingHandler(this IHttpClientBuilder builder)
        {
            builder.Services.TryAddTransient<MessageLoggingHandler<HttpClient>>();
            return builder.AddHttpMessageHandler<MessageLoggingHandler<HttpClient>>();
        }
    }
}
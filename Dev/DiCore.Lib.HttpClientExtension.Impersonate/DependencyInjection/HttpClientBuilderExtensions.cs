using System;
using System.Security.Principal;
using DiCore.Lib.HttpClientExtension.DependencyInjection;
using DiCore.Lib.HttpClientExtension.Impersonate.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace DiCore.Lib.HttpClientExtension.Impersonate.DependencyInjection
{
    public static class HttpClientBuilderExtensions
    {
        /// <summary>
        /// Использовать имперсонацию
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IHttpClientBuilder Impersonate(this IHttpClientBuilder builder)
        {
            builder.Services.AddTransient<ImpersonateMessageHandler>();
            builder.AddHttpMessageHandler<ImpersonateMessageHandler>();
            builder.UseDefaultCredentials();
            return builder;
        }
        /// <summary>
        /// Использовать учетные данные при имперсонации
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="identityAccess">Функция получения учётных данных</param>
        /// <returns></returns>
        public static IServiceCollection UseIdentity(this IServiceCollection serviceCollection,
            Func<IServiceProvider, Func<WindowsIdentity>> identityAccess)
        {
            serviceCollection.AddTransient<IIdentityAccessor>(sc => new DefaultIdentityAccessor(identityAccess(sc)));
            return serviceCollection;
        }
    }
}
using Microsoft.Extensions.DependencyInjection;

namespace DiCore.Lib.HttpClientExtension.Impersonate.HttpContext.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Использовать учетные данные пользователя из контекста Http
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <returns></returns>
        public static IServiceCollection UseIdentityFromHttpContext(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddHttpContextAccessor()
                .AddTransient<IIdentityAccessor, HttpContextIdentityAccessor>();
        }
    }
}
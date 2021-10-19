using Microsoft.AspNetCore.Builder;

namespace DiCore.Lib.CorsMiddleware.DependencyInjection
{
    public static class CorsMiddlewareExtensions
    {
        public static IApplicationBuilder UseCorsMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<CorsMiddleware>();
        }
    }
}
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DiCore.Lib.CorsMiddleware
{
    internal class CorsMiddleware
    {
        private readonly RequestDelegate next;

        public CorsMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var origin = context.Request.Headers["origin"].FirstOrDefault();
            var useCors = false;
            if (origin != null)
            {
                context.Response.Headers.Add("Access-Control-Allow-Origin", origin);
                useCors = true;
            }
            else
            {
                var referer = context.Request.Headers["referer"].FirstOrDefault();
                if (!string.IsNullOrEmpty(referer))
                {
                    var refererUrl = new Uri(referer);
                    context.Response.Headers.Add("Access-Control-Allow-Origin",
                        $"{refererUrl.Scheme}://{refererUrl.Host}:{refererUrl.Port}");
                    useCors = true;
                }
            }

            if (useCors)
            {
                context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                context.Response.Headers.Add("Access-Control-Allow-Headers",
                    "Origin, X-Requested-With, Content-Type, Accept");
                context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
            }

            if (string.Equals(context.Request.Method, "OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync("OK");
            }
            else
            {
                await next(context);
            }
        }
    }
}
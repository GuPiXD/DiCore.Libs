using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DiCore.Lib.HttpClientExtension.Logging
{
    internal class MessageLoggingHandler<T> : DelegatingHandler
    {
        private readonly ILogger<T> logger;

        public MessageLoggingHandler(ILogger<T> logger)
        {
            this.logger = logger;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            logger.LogTrace($"Url:{request.RequestUri}, method: {request.Method}");
            logger.LogTrace(
                $"Thread identity: {Thread.CurrentPrincipal?.Identity?.Name}, windows identity: {WindowsIdentity.GetCurrent().Name}, impersonation level: {WindowsIdentity.GetCurrent().ImpersonationLevel}");
            return base.SendAsync(request, cancellationToken);
        }
    }
}
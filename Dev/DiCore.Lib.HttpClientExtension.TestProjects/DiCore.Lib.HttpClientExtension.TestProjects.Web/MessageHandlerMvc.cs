using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using HttpContext = System.Web.HttpContext;

namespace DiCore.Lib.HttpClientExtension.TestProjects.Web
{
    public class MessageHandlerMvc : DelegatingHandler
    {
        private readonly ILogger<MessageHandlerMvc> logger;
        private readonly IHttpContextAccessor httpContextAccessor;

        //MessageHandlerMvc()
        public MessageHandlerMvc(ILogger<MessageHandlerMvc> logger, IHttpContextAccessor httpContextAccessor)
        {
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var identity = httpContextAccessor.HttpContext?.User?.Identity as WindowsIdentity;
            logger.LogTrace($"Http context identity: {identity?.Name}, thread identity: {Thread.CurrentPrincipal?.Identity?.Name}");
            return base.SendAsync(request, cancellationToken);
        }
    }
}
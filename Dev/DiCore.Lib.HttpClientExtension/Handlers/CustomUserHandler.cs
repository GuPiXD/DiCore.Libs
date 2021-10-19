using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DiCore.Lib.HttpClientExtension.Handlers
{
    internal class CustomUserHandler : DelegatingHandler
    {
        private readonly Action<HttpRequestMessage, CancellationToken> handler;

        public CustomUserHandler(Action<HttpRequestMessage, CancellationToken> handler)
        {
            this.handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            handler(request, cancellationToken);
            return base.SendAsync(request, cancellationToken);
        }
    }
}
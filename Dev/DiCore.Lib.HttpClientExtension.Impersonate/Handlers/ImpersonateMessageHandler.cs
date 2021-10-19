using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace DiCore.Lib.HttpClientExtension.Impersonate.Handlers
{
    internal class ImpersonateMessageHandler : DelegatingHandler
    {
        private readonly IIdentityAccessor identityAccessor;

        public ImpersonateMessageHandler(IIdentityAccessor identityAccessor)
        {
            this.identityAccessor = identityAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var identity = identityAccessor.Identity;
            var response = await WindowsIdentity.RunImpersonated(identity.AccessToken,
                () => base.SendAsync(request, cancellationToken));
            return response;
        }
    }
}
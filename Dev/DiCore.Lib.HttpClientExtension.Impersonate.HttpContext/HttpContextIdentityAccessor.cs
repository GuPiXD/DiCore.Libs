using System.Security.Principal;
using Microsoft.AspNetCore.Http;

namespace DiCore.Lib.HttpClientExtension.Impersonate.HttpContext
{
    internal class HttpContextIdentityAccessor : IIdentityAccessor
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public HttpContextIdentityAccessor(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public WindowsIdentity Identity => httpContextAccessor.HttpContext?.User?.Identity as WindowsIdentity;
    }
}
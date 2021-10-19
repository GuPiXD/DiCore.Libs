using System.Security.Principal;
using DiCore.Lib.HttpClientExtension.Impersonate;
using Microsoft.AspNetCore.Http;

namespace DiCore.Lib.RestClient.TestCore.Api
{
    public class HttpContextIdentityAccessor : IIdentityAccessor
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public HttpContextIdentityAccessor(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public WindowsIdentity Identity => httpContextAccessor.HttpContext.User.Identity as WindowsIdentity;
    }
}
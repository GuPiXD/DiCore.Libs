using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using HttpContext = System.Web.HttpContext;

namespace DiCore.Lib.HttpClientExtension.TestProjects.Web
{
    public class IdentityAccessorMvc
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly HttpContext httpContextBase;
        

        public IdentityAccessorMvc(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
            //this.httpContextBase = httpContextBase;
        }

        public WindowsIdentity Identity
        {
            get
            {
                var identity1 = httpContextBase?.User?.Identity as WindowsIdentity;
                var identity2 = HttpContext.Current?.User?.Identity as WindowsIdentity;
                var identity3 = httpContextBase?.User?.Identity as WindowsIdentity;
                return identity1 ?? identity2 ?? identity3;
            }
        }
    }
}
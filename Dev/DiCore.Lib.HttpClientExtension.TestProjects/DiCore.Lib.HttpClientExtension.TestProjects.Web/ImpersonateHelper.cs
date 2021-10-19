using System;
using System.Security.Principal;
using System.Threading;

namespace DiCore.Lib.HttpClientExtension.TestProjects.Web
{
    public static class ImpersonateHelper
    {
        public static T RunImpersonate<T>(this WindowsIdentity identity, Func<T> func)
        {
            using (ExecutionContext.SuppressFlow())
            {
                return WindowsIdentity.RunImpersonated(identity.AccessToken, func);
            }
        }
    }
}
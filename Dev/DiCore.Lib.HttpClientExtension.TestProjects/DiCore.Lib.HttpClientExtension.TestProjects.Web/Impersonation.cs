using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace DiCore.Lib.HttpClientExtension.TestProjects.Web
{
    public class Impersonation
    {
        public static Task<T> RunAsAsync<T>(WindowsIdentity identity, Func<Task<T>> func)
        {
            var result = Task.Run(() =>
            {
                using (ExecutionContext.SuppressFlow())
                {
                    return WindowsIdentity.RunImpersonated(identity.AccessToken,
                        func);
                }
            });
            return result;
        }

        public static T RunAs<T>(WindowsIdentity identity, Func<T> func)
        {
            using (ExecutionContext.SuppressFlow())
            {
                return WindowsIdentity.RunImpersonated(identity.AccessToken,
                    func);
            }
        }
    }
}
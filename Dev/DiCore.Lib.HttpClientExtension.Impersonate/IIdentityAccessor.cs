using System.Security.Principal;

namespace DiCore.Lib.HttpClientExtension.Impersonate
{
    public interface IIdentityAccessor
    {
        WindowsIdentity Identity { get; }
    }
}
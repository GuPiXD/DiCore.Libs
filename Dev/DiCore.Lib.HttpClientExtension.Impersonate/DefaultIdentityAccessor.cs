using System;
using System.Security.Principal;

namespace DiCore.Lib.HttpClientExtension.Impersonate
{
    internal class DefaultIdentityAccessor : IIdentityAccessor
    {
        private readonly Func<WindowsIdentity> identityAccess;

        public DefaultIdentityAccessor(Func<WindowsIdentity> identityAccess)
        {
            this.identityAccess = identityAccess;
        }


        public WindowsIdentity Identity => identityAccess();
    }
}
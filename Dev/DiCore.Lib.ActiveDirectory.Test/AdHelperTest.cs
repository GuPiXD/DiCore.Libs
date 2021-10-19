using System;
using DiCore.Lib.ActiveDirectory.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DiCore.Lib.ActiveDirectory.Test
{
    [TestClass]
    public class AdHelperTest
    {
        private const string CurrentDomainName = "ctd.tn.corp";
        private const int LocalUserNumber = 81;
        private const string RemoteUserLogin = @"KamakinSN@spb.tn.corp";

        private readonly Tuple<string, string, string> RemoteUserFullName = Tuple.Create("Камакин", "Сергей",
            "Николаевич");
        private readonly Guid localUserAdGuid = new Guid("ab51fa34-cb02-4a23-b88d-18aaabd77118");
        private AdHelper adHelperLocal;
        private AdHelper adHelperRemote;

        [TestInitialize]
        public void Init()
        {
            adHelperLocal = AdHelper.GetAdHelper();
            adHelperRemote = AdHelper.GetAdHelper("spb.tn.corp");
        }

        [TestMethod]
        public void GetCurrentDomainName()
        {
            var result = AdHelper.GetCurrentDomainName();
            Assert.AreEqual(CurrentDomainName, result, true);
        }

        [TestMethod]
        public void GetUserByNumber()
        {
            var adUser = adHelperLocal.GetUserByNumber(LocalUserNumber);
            Assert.AreEqual(localUserAdGuid, adUser.Id);
        }

        [TestMethod]
        public void GetRemoteUserByLogin()
        {
            var adUser = adHelperRemote.GetUserByLogin(RemoteUserLogin);
            Assert.AreEqual(RemoteUserFullName, Tuple.Create(adUser.LastName, adUser.FirstName, adUser.SecondName));
        }
    }
}

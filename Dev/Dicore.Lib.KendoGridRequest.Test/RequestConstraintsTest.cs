using System;
using DiCore.Lib.KendoGridRequest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Dicore.Lib.KendoGridRequest.Test
{
    [TestClass]
    public class RequestConstraintsTest
    {
        [TestMethod]
        public void ConfigNullConstructorTest()
        {
            var rc = new RequestConstraints(null);
            Assert.IsFalse(rc.ConstraintsPresent);
        }

        [TestMethod]
        public void ConfigurationNullAndVisibleNullConstructorTest()
        {
            // "{{\"Visible\": null, \"Configuration\": null}}"
            var input = new JObject();
            input.Add("Visible", null);
            input.Add("Configuration", null);
            var rc = new RequestConstraints(input);
        }

        [TestMethod]
        public void ConfigurationDontVisibleColumnTest()
        {
            var conf = JObject.Parse("{ \"Id\": { \"present\": false, \"visible\": false }, \"ArtifactLength\": { \"present\": true, \"visible\": true}, \"ArtifactNumber\": {\"present\": true, \"visible\": false }}");
            
            var input = new JObject();
            input.Add("Visible", true);
            input.Add("Configuration", conf);
            var rc = new RequestConstraints(input);
            Assert.IsNotNull(rc.Visible);
            Assert.IsTrue((bool)rc.Visible);
            Assert.IsFalse(rc.GetConfiguration("Id").Present);
            Assert.IsFalse(rc.GetConfiguration("Id").Visible);
            Assert.IsTrue(rc.GetConfiguration("ArtifactLength").Present);
            Assert.IsTrue(rc.GetConfiguration("ArtifactLength").Visible);
            Assert.IsTrue(rc.GetConfiguration("ArtifactNumber").Present);
            Assert.IsFalse(rc.GetConfiguration("ArtifactNumber").Visible);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DiCore.Lib.HttpClientExtension.ImpersonateHelpers.AspNet;
using Microsoft.Extensions.Logging;

namespace DiCore.Lib.HttpClientExtension.TestProjects.Web.Controllers
{
    //[Authorize]
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory clientFactory;
        private readonly ILogger<HomeController> logger;
        private readonly HttpClient clientGlobal;

        public HomeController(IHttpClientFactory clientFactory, ILogger<HomeController> logger)
        {
            this.clientFactory = clientFactory;
            this.logger = logger;
            clientGlobal = clientFactory.CreateClient("test");
        }

        [Authorize]
        public async Task<ActionResult> Index()
        {
            var identity = User.Identity as WindowsIdentity;
            var threadIdentity = WindowsIdentity.GetCurrent();
            logger.LogTrace(
                $"Index. App identity: {threadIdentity.Name}, impersonationLevel: {threadIdentity.ImpersonationLevel}, http identity: {identity?.Name}");
            //var identity = HttpContext.User as WindowsIdentity;
            var result = await clientGlobal.GetAsync<string[]>(identity, "values");
            var resultAsync = await clientGlobal.GetAsync<string[]>(identity, "values");
            var resultSync = clientGlobal.Get<string[]>(identity, "values");
            var resultPut = await clientGlobal.PutAsync<int>(identity, new {Value = "value"}, "values/1");
            return View();
        }

        [Authorize]
        public ActionResult About()
        {
            var identity = User.Identity as WindowsIdentity;
            var result = Task.Run(() => clientGlobal.GetAsync<string[]>(identity, "values")).Result;
            var resultSync = clientGlobal.Get<string[]>(identity, "values");
            return View();
        }

        //[Authorize]
        [Authorize]
        public async Task<ActionResult> Contact()
        {
            var identity = User.Identity as WindowsIdentity;
            var result =
                await ImpersonateHelper.RunImpersonate(identity, () => clientGlobal.GetAsync<string>("values"));
            return View();
        }
    }
}
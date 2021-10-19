using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DiCore.Lib.HttpClientExtension.TestProjects.Core.Models;
using Microsoft.AspNetCore.Authorization;

namespace DiCore.Lib.HttpClientExtension.TestProjects.Core.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public IActionResult Index()
        {
            var windowsIdentity = User.Identity as WindowsIdentity;
            if (windowsIdentity != null)
            {
                
                WindowsIdentity.RunImpersonated(windowsIdentity.AccessToken, () =>
                {
                    var httpClient = new HttpClient(new HttpClientHandler()
                    {
                        UseDefaultCredentials = true,
                        PreAuthenticate = true,
                        AllowAutoRedirect = true,
                        ClientCertificateOptions = ClientCertificateOption.Automatic

                    });
                    httpClient.BaseAddress = new Uri("http://localhost:44334/api/");
                    var response = httpClient.GetAsync("values").Result;

                    //var webClient = new WebClient()
                    //{
                    //    BaseAddress = "http://localhost:44334/api/",
                    //    UseDefaultCredentials = true
                    //};
                    //var responseWeb = webClient.DownloadString("values");
                });
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

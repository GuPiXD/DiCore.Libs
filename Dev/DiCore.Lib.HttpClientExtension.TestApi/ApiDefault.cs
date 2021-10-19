using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using DiCore.Lib.HttpClientExtension.DependencyInjection;
using DiCore.Lib.HttpClientExtension.Impersonate.DependencyInjection;
using DiCore.Lib.HttpClientExtension.Impersonate.HttpContext.DependencyInjection;
using DiCore.Lib.RestClient.TestCore.Api.Repository;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DiCore.Lib.RestClient.TestCore.Api
{
    public class ApiDefault
    {
        private readonly int port;
        public readonly IStorage Storage;
        private readonly bool windowsAuthentication;
        private readonly Uri remoteApi;
        private readonly string acceptedUser;
        private readonly bool impersonate;
        private CancellationTokenSource tokenSource;
        public string Url => $"http://localhost:{port}";

        public ApiDefault(int port, IStorage storage, bool windowsAuthentication = false, Uri remoteApi = null,
            string acceptedUser = null, bool impersonate = false)
        {
            this.port = port;
            Storage = storage;
            this.windowsAuthentication = windowsAuthentication;
            this.remoteApi = remoteApi;
            this.acceptedUser = acceptedUser;
            this.impersonate = impersonate;
        }

        public Task Start(params string[] args)
        {
            var host = BuildWebHost(args);
            tokenSource = new CancellationTokenSource();
            return host.RunAsync(tokenSource.Token);
        }

        public void Stop()
        {
            tokenSource.Cancel();
        }

        public IWebHost BuildWebHost(params string[] args)
        {
            return WebHost
                .CreateDefaultBuilder(args)
                .ConfigureServices(serviceCollection =>
                {
                    serviceCollection.AddSingleton(Storage);
                    //serviceCollection.AddRestClientDefaultCredentials(cfg => { cfg.BaseUrl = remoteApi; });
                    serviceCollection.AddSingleton(new UserSettings
                    {
                        AcceptedUserName = acceptedUser
                    });
                    if (impersonate)
                    {
                        serviceCollection
                            //.AddHttpContextAccessor()
                            //.UseIdentity(sp =>
                            //{
                            //    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
                            //    return () => httpContextAccessor.HttpContext.User.Identity as WindowsIdentity;
                            //})
                            .UseIdentityFromHttpContext()
                            .AddHttpClient("impersonate", client => { client.BaseAddress = new Uri($"{Url}/api/v1"); })
                            .Impersonate();
                        ;
                    }
                })
                .UseStartup<StartupDefault>()
                .UseHttpSys(options =>
                {
                    if (windowsAuthentication)
                    {
                        options.Authentication.Schemes = AuthenticationSchemes.Kerberos | AuthenticationSchemes.NTLM |
                                                         AuthenticationSchemes.Negotiate;
                        options.Authentication.AllowAnonymous = false;
                    }
                    else
                    {
                        options.Authentication.Schemes = AuthenticationSchemes.None;
                        options.Authentication.AllowAnonymous = true;
                    }


                    options.MaxConnections = null;
                    options.MaxRequestBodySize = 30000000;
                    options.UrlPrefixes.Add(Url);
                })
                .Build();
        }
    }
}
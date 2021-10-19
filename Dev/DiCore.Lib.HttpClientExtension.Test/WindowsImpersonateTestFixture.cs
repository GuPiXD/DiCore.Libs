using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Principal;
using DiCore.Lib.HttpClientExtension.DependencyInjection;
using DiCore.Lib.RestClient.TestCore.Api;
using DiCore.Lib.RestClient.TestCore.Api.Repository;
using DiCore.Lib.RestClient.TestCore.Logon;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32.SafeHandles;

namespace DiCore.Lib.RestClient.TestCore
{
    public class WindowsImpersonateTestFixture : IDisposable
    {
        private ApiDefault apiMiddle;
        private const int PortMiddle = Configs.WindowsImpersonateTestMiddleApiPort;
        private readonly ApiDefault apiRemote;
        private const int PortRemote = Configs.WindowsImpersonateTestRemoteApiPort;
        public string Url => $"{apiMiddle?.Url}/api/v1";
        private const string Domain = Configs.ImpersonateUserDomain;
        private const string Login = Configs.ImpersonateUserLogin;
        private const string Password = Configs.ImpersonateUserPassword;
        public readonly IServiceProvider ServiceProvider;

        public WindowsImpersonateTestFixture()
        {
            var storage = new StorageInMemory();
            storage.InitializePersons(new Dictionary<Guid, Person>()
            {
                {
                    new Guid("a8226429-31b8-48b9-a6a1-321bee4bbec9"),
                    new Person {Name = "Alice", Age = 23, Active = true}
                },
                {
                    new Guid("57204b27-e2ad-40ce-a863-5923e836c5fa"),
                    new Person {Name = "Боб", Age = 34, Active = false}
                }
            });
            var services = new ServiceCollection();
            services
                .AddSingleton<IStorage>(storage)
                //.AddHttpContextAccessor()
                //.AddRestClientDefaultCredentials("defaultCredentials", cfg => { cfg.BaseUrl = new Uri(Url); });
                //.AddTransient<IIdentityAccessor>(sc =>
                //{
                //    var identity = sc.GetService<IHttpContextAccessor>().HttpContext.User.Identity as WindowsIdentity;
                //    return new DefaultIdentityAccessor(identity);
                //})
                .AddHttpClient("defaultCredentials", client => { client.BaseAddress = new Uri(Url); })
                .UseDefaultCredentials()
                .ThrowServerErrors();
            ServiceProvider = services.BuildServiceProvider();
            var userName = WindowsIdentity.GetCurrent().Name;

            apiRemote = new ApiDefault(PortRemote, storage, true, null, userName);
            apiRemote.Start();
            var domain = Domain ?? Environment.MachineName;
            SafeAccessTokenHandle safeAccessTokenHandle;
            if (Win32NativeMethods.LogonUser(Login, domain, Password,
                (int) LogonType.LOGON32_LOGON_INTERACTIVE, (int) LogonProvider.LOGON32_PROVIDER_DEFAULT,
                out safeAccessTokenHandle))
            {
                WindowsIdentity.RunImpersonated(safeAccessTokenHandle, () =>
                {
                    apiMiddle = new ApiDefault(PortMiddle, storage, true, new Uri($"{apiRemote?.Url}/api/v1"),null,true);

                    apiMiddle.Start();
                });
            }
        }


        public void Dispose()
        {
            apiMiddle.Stop();
            apiRemote.Stop();
        }
    }
}
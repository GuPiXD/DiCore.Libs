using System;
using System.Collections.Generic;
using System.Security.Principal;
using DiCore.Lib.HttpClientExtension.DependencyInjection;
using DiCore.Lib.RestClient.TestCore.Api;
using DiCore.Lib.RestClient.TestCore.Api.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace DiCore.Lib.RestClient.TestCore
{
    public class WindowsAuthTestFixture : IDisposable

    {
        private readonly ApiDefault api;
        private const int Port = Configs.WindowsAuthTestApiPort;

        public string Url => $"{api?.Url}/api/v1";

        //public readonly IStorage Storage;
        public readonly IServiceProvider ServiceProvider;

        public WindowsAuthTestFixture()
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
                //.AddRestClientDefaultCredentials("defaultCredentials", cfg => { cfg.BaseUrl = new Uri(Url); });
                .AddHttpClient("defaultCredentials", client => { client.BaseAddress = new Uri(Url); })
                .UseDefaultCredentials();
            ServiceProvider = services.BuildServiceProvider();
            var userName = WindowsIdentity.GetCurrent().Name;
            api = new ApiDefault(Port, storage, true, null, userName);
            api.Start();
        }

        public void Dispose()
        {
            api.Stop();
        }
    }
}
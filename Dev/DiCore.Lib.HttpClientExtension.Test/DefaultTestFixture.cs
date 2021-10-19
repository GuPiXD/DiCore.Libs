using System;
using System.Collections.Generic;
using System.IO;
using DiCore.Lib.HttpClientExtension.DependencyInjection;
using DiCore.Lib.RestClient.TestCore.Api;
using DiCore.Lib.RestClient.TestCore.Api.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace DiCore.Lib.RestClient.TestCore
{
    public class DefaultTestFixture : IDisposable
    {
        private readonly ApiDefault api;
        private const int Port = Configs.DefaultTestApiPort;

        public string Url => $"{api?.Url}/api/v1";
        public string PersonsUrl => $"{api?.Url}/api/v1/persons";

        //public readonly IStorage Storage;
        public readonly IServiceProvider ServiceProvider;

        public DefaultTestFixture()
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

            
            services.AddSingleton<IStorage>(storage);
            services.AddLogging(cfg => cfg.AddSerilog());
            services
                .AddHttpClient("main", client => client.BaseAddress = new Uri(Url))
                .ThrowClientErrors()
                .ThrowServerErrors();
            services
                .AddHttpClient("persons", client => client.BaseAddress = new Uri(PersonsUrl))
                .ThrowClientErrors()
                .ThrowServerErrors();
            Log.Logger = new LoggerConfiguration().WriteTo.File($"logs/{DateTime.Now:yyMMddHH}.log").MinimumLevel
                .Verbose().CreateLogger();
            ServiceProvider = services.BuildServiceProvider();
            api = new ApiDefault(Port, storage);
            api.Start();
        }

        public void Dispose()
        {
            api.Stop();
        }
    }
}
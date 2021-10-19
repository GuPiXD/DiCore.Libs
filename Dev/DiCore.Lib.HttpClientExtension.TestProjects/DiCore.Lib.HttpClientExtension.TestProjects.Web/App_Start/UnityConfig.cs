using Microsoft.Extensions.DependencyInjection;
using System;
using System.Configuration;
using System.Security.Principal;
using System.Web;
using DiCore.Lib.HttpClientExtension.DependencyInjection;
using DiCore.Lib.HttpClientExtension.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Unity;
using Unity.AspNet.Mvc;
using Unity.Microsoft.DependencyInjection;
using HttpContext = System.Web.HttpContext;
using Serilog;

namespace DiCore.Lib.HttpClientExtension.TestProjects.Web
{
    /// <summary>
    /// Specifies the Unity configuration for the main container.
    /// </summary>
    public static class UnityConfig
    {
        #region Unity Container
        private static Lazy<IUnityContainer> container =
          new Lazy<IUnityContainer>(() =>
          {
              var container = new UnityContainer();
              RegisterTypes(container);
              return container;
          });

        /// <summary>
        /// Configured Unity Container.
        /// </summary>
        public static IUnityContainer Container => container.Value;
        #endregion

        /// <summary>
        /// Registers the type mappings with the Unity container.
        /// </summary>
        /// <param name="container">The unity container to configure.</param>
        /// <remarks>
        /// There is no need to register concrete types such as controllers or
        /// API controllers (unless you want to change the defaults), as Unity
        /// allows resolving a concrete type even if it was not previously
        /// registered.
        /// </remarks>
        public static void RegisterTypes(IUnityContainer container)
        {
            // NOTE: To load from web.config uncomment the line below.
            // Make sure to add a Unity.Configuration to the using statements.
            // container.LoadConfiguration();

            // TODO: Register your type's mappings here.
            // container.RegisterType<IProductRepository, ProductRepository>();
            
            var apiUrl = new Uri(ConfigurationManager.AppSettings["ApiUrl"]);
            var logFile = ConfigurationManager.AppSettings["LogFile"];
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.File(logFile, rollingInterval: RollingInterval.Day)
                .MinimumLevel.Verbose()
                .CreateLogger();
            var services = new ServiceCollection();
            services.AddLogging(cfg => {
                cfg.AddSerilog(dispose: true);
                cfg.SetMinimumLevel(LogLevel.Trace);
                cfg.AddDebug();
            });
            services

                .AddHttpClient("test",
                    cfg => cfg.BaseAddress = apiUrl)
                .UseDefaultCredentials()
                .AddMessageLoggingHandler();
            //.Impersonate();
            
            container.RegisterFactory<HttpContextBase>(_ => new HttpContextWrapper(HttpContext.Current), new PerRequestLifetimeManager());
            container.RegisterType<IdentityAccessorMvc>(new PerRequestLifetimeManager());
            container.BuildServiceProvider(services);
        }
    }
}
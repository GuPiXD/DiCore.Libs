using System.Collections.Generic;
using System.Globalization;
using DiCore.Lib.RestClient.TestCore.Api.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace DiCore.Lib.RestClient.TestCore.Api
{
    public class StartupDefault
    {
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddSingleton<IStorage, StorageInMemory>();
            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture =
                    new Microsoft.AspNetCore.Localization.RequestCulture(CultureInfo.InvariantCulture);
                options.SupportedCultures = new List<CultureInfo> {CultureInfo.InvariantCulture};
                options.RequestCultureProviders.Clear();
            });
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRequestLocalization();
            //CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            //CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            app.UseMvc();
        }
    }
}
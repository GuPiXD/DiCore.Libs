using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;

namespace DiCore.Lib.WebApi.Routing
{
    public class NamespaceHttpControllerSelector:DefaultHttpControllerSelector
    {
        private readonly HttpConfiguration configuration;
        public NamespaceHttpControllerSelector(HttpConfiguration configuration) : base(configuration)
        {
            this.configuration = configuration;
        }

        public override IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
        {
            var typeResolver = configuration.Services.GetHttpControllerTypeResolver();
            var assembliesResolver = configuration.Services.GetAssembliesResolver();
            if (typeResolver == null || assembliesResolver == null)
                return base.GetControllerMapping();
            var controllerTypes = typeResolver.GetControllerTypes(assembliesResolver);
            var result = controllerTypes
                .ToDictionary(c => c.FullName, c => new HttpControllerDescriptor(configuration, c.FullName, c));
            return result;
        }
    }
}

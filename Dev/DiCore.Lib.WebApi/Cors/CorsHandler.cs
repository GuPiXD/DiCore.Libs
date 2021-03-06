using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace DiCore.Lib.WebApi.Cors
{
    public class CorsHandler : DelegatingHandler
    {
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var referer = request.Headers.Referrer;
            var httpContext = request.Properties["MS_HttpContext"] as HttpContextWrapper;
            string origin = null;
            if (request.Headers.Contains("Origin"))
            {
                origin = request.Headers.GetValues("Origin").FirstOrDefault() ?? "*";
            }
            else
            {
                if (referer != null)
                    origin = string.Format("{0}://{1}:{2}", referer.Scheme, referer.Host, referer.Port);
            }
            if (origin == null)
            {
                return await base.SendAsync(request, cancellationToken);
            }
            HttpResponseMessage response;
            if (request.Method == HttpMethod.Options)
            {
                response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Headers.Add("Access-Control-Allow-Origin", origin);
                response.Headers.Add("Access-Control-Allow-Methods", "GET,POST,PUT,DELETE,OPTIONS");
                response.Headers.Add("Access-Control-Allow-Headers", "Content-type,Authorization");
                response.Headers.Add("Access-Control-Max-Age", "1728000");
                response.Headers.Add("Access-Control-Allow-Credentials", "true");
            }
            else if (!request.Headers.Contains("Authorization"))
            {
                
                response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    Content = new StringContent("Unathorized")
                };
                response.Headers.Add("Access-Control-Allow-Origin", origin);
                response.Headers.Add("Access-Control-Allow-Methods", "*");
                response.Headers.Add("Access-Control-Allow-Headers", "*");
                response.Headers.Add("Access-Control-Max-Age", "1728000");
                response.Headers.Add("Access-Control-Allow-Credentials", "true");
            }
            else
            {
                response = await base.SendAsync(request, cancellationToken);
                if (!response.Headers.Contains("Access-Control-Allow-Origin"))
                {
                    response.Headers.Add("Access-Control-Allow-Origin", origin);
                    response.Headers.Add("Access-Control-Allow-Methods", "*");
                    response.Headers.Add("Access-Control-Allow-Headers", "*");
                    response.Headers.Add("Access-Control-Max-Age", "1728000");
                    response.Headers.Add("Access-Control-Allow-Credentials", "true");
                }
            }
            return response;

        }
    }
}

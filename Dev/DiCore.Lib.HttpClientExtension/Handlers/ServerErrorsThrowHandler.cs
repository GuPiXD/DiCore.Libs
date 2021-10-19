using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DiCore.Lib.HttpClientExtension.Handlers
{
    internal class ServerErrorsThrowHandler:DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
                return response;
            if ((int)response.StatusCode >= 500 && (int)response.StatusCode <= 599)
            {
                return response.EnsureSuccessStatusCode();
            }

            return response;
        }
    }
}
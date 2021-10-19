using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace DiCore.Lib.HttpClientExtension
{
    public static partial class HttpClientExtensions
    {
        private const string ApplicationJsonHeader = "application/json";

        private static Uri GetUri(this HttpClient client, string url)
        {
            return client.BaseAddress == null ? new Uri(url, UriKind.Absolute) : new Uri(url, UriKind.Relative);
        }

        private static Uri NormalizeUri(Uri url)
        {
            return url.ToString().EndsWith("/") ? url : new Uri($"{url}/", UriKind.RelativeOrAbsolute);
        }

        private static Uri MakeUri(this HttpClient client, Uri url, params UrlParameter[] parameters)
        {
            if (!url.IsAbsoluteUri && client.BaseAddress == null)
            {
                return null;
            }

            var absoluteUri = url.IsAbsoluteUri
                ? NormalizeUri(url)
                : NormalizeUri(new Uri(NormalizeUri(client.BaseAddress), url));
            if (parameters != null && parameters.Any())
            {
                var urlParts = parameters.Where(a => a.Name == null).Select(a => a.Value?.ToString());
                var queryAttributes = parameters
                    .Where(a => !string.IsNullOrWhiteSpace(a.Name))
                    .Select(a => $"{a.Name}={Convert.ToString(a.Value, CultureInfo.InvariantCulture)}");
                var queryString = string.Join("&", queryAttributes);
                absoluteUri = urlParts.Aggregate(absoluteUri, (current, urlPart) => new Uri(current, $"{urlPart}/"));
                if (!string.IsNullOrWhiteSpace(queryString))
                {
                    absoluteUri = new Uri(absoluteUri, $"?{queryString}");
                }
            }

            return absoluteUri;
        }

        private static async Task<TOut> SendAsync<TOut>(this HttpClient client, HttpMethod method, Uri url,
            CancellationToken ct, Func<string, HttpResponseMessage, TOut> parse, HttpContent content,
            params UrlParameter[] parameters)
        {
            var absoluteUrl = client.MakeUri(url, parameters);
            using (var requestMessage = new HttpRequestMessage(method, absoluteUrl))
            {
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(ApplicationJsonHeader));
                if (content != null)
                {
                    requestMessage.Content = content;
                }

                using (var response = await client.SendAsync(requestMessage, ct).ConfigureAwait(false))
                {
                    var stringContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return parse(stringContent, response);
                }
            }
        }

        private static TOut Send<TOut>(this HttpClient client, HttpMethod method, Uri url,
            Func<string, HttpResponseMessage, TOut> parse, HttpContent content,
            params UrlParameter[] parameters)
        {
            return Task.Run(() => SendAsync(client, method, url, CancellationToken.None, parse, content, parameters))
                .Result;
        }
    }
}
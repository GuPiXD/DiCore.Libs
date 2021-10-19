using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;

namespace DiCore.Lib.Web
{
    public enum UploadMethod
    {
        Post,
        Put
    }

    public class SimpleWebClient : ISimpleWebClient
    {
        public int Timeout { get; set; } = 300000;
        private readonly TraceSource traceSource = new TraceSource("DiCore.Lib.Web");
        private readonly WindowsIdentity identity;
        
        public Uri BaseUri { get; }
        public MemoryStream Download(string url, params ClientAttribute[] attributes)
        {
            return Download(GetUri(url), attributes);
        }

        public FileResult DownloadExt(string url, params ClientAttribute[] attributes)
        {
            return DownloadExt(GetUri(url), attributes);
        }

        public FileResult DownloadExt(Uri url, params ClientAttribute[] attributes)
        {
            var absoluteUri = MakeUri(url, attributes);
            traceSource.TraceInformation($"Absolute URL: {absoluteUri}");
            if (absoluteUri == null)
            {
                return null;
            }
            WindowsImpersonationContext ctx = null;
            try
            {
                if (identity?.User != null)
                {
                    traceSource.TraceInformation($"Identity {identity?.Name} ({identity?.AuthenticationType})");
                    ctx = identity.Impersonate();
                }
                else
                {
                    traceSource.TraceInformation("Identity null");
                }
                var webClient = CreateWebClient();
                try
                {
                    var result = webClient.DownloadData(absoluteUri);
                    var contentType = webClient.ResponseHeaders["Content-Type"];
                    var contentLength = long.Parse(webClient.ResponseHeaders["Content-Length"] ?? "0");
                    var contentDisposition = Uri.UnescapeDataString(webClient.ResponseHeaders["Content-Disposition"]);
                    var fileName = contentDisposition.Substring((int) contentDisposition?.IndexOf("filename=") + 9)
                                       .Replace("\"", "").Trim() ;
                    
                    var stream = new MemoryStream(result) { Position = 0 };
                    return new FileResult(fileName, contentType, contentLength, stream);
                }
                catch (WebException exc)
                {
                    var response = (HttpWebResponse)exc.Response;
                    if (response != null)
                    {
                        var stream = response.GetResponseStream();
                        var text = stream != null ? new StreamReader(stream).ReadToEnd() : "";
                        throw new WebClientException(response.StatusCode, response.StatusDescription, text,
                            absoluteUri.ToString(), exc);
                    }
                    throw new WebClientException($"{exc.Status}:{exc.Message}", absoluteUri.ToString(), exc);
                }
            }
            finally
            {
                ctx?.Undo();
            }
        }

        public MemoryStream Download(Uri url, params ClientAttribute[] attributes)
        {
            var absoluteUri = MakeUri(url, attributes);
            traceSource.TraceInformation($"Absolute URL: {absoluteUri}");
            if (absoluteUri == null)
            {
                return null;
            }
            WindowsImpersonationContext ctx = null;
            try
            {
                if (identity?.User != null)
                {
                    traceSource.TraceInformation($"Identity {identity?.Name} ({identity?.AuthenticationType})");
                    ctx = identity.Impersonate();
                }
                else
                {
                    traceSource.TraceInformation("Identity null");
                }
                var webClient = CreateWebClient();
                try
                {
                    var result = webClient.DownloadData(absoluteUri);
                    var stream = new MemoryStream(result) {Position = 0};
                    return stream;
                }
                catch (WebException exc)
                {
                    var response = (HttpWebResponse)exc.Response;
                    if (response != null)
                    {
                        var stream = response.GetResponseStream();
                        var text = stream != null ? new StreamReader(stream).ReadToEnd() : "";
                        throw new WebClientException(response.StatusCode, response.StatusDescription, text,
                            absoluteUri.ToString(), exc);
                    }
                    throw new WebClientException($"{exc.Status}:{exc.Message}", absoluteUri.ToString(), exc);
                }
            }
            finally
            {
                ctx?.Undo();
            }
        }

        private WebClient CreateWebClient()
        {
            var webClient = new WebClientWrapper(Timeout)
            {
                UseDefaultCredentials = true

            };
            
            return webClient;
        }
        
        public void Upload(MemoryStream stream, UploadMethod uploadMethod, string url, params ClientAttribute[] attributes)
        {
            Upload<EmptyResponse>(stream, uploadMethod, url, attributes);
        }

        public TOut Upload<TOut>(MemoryStream stream, UploadMethod uploadMethod, string url,
            params ClientAttribute[] attributes)
        {
            return Upload<TOut>(stream, uploadMethod, GetUri(url), attributes);
        }

        public void Upload(MemoryStream stream, UploadMethod uploadMethod, Uri url, params ClientAttribute[] attributes)
        {
            Upload<EmptyResponse>(stream, uploadMethod, url, attributes);
        }

        public TOut Upload<TOut>(MemoryStream stream, UploadMethod uploadMethod, Uri url,
            params ClientAttribute[] attributes)
        {
            var absoluteUri = MakeUri(url, attributes);
            traceSource.TraceInformation($"Absolute URL: {absoluteUri}");
            if (absoluteUri == null)
            {
                return default(TOut);
            }
            WindowsImpersonationContext ctx = null;
            try
            {
                if (identity?.User != null)
                {
                    traceSource.TraceInformation($"Identity {identity?.Name} ({identity?.AuthenticationType})");
                    ctx = identity.Impersonate();
                }
                else
                {
                    traceSource.TraceInformation("Identity null");
                }
                var webClient = CreateWebClient();
                try
                {
                    var inputData = stream.ToArray();
                    var result = webClient.UploadData(absoluteUri, uploadMethod == UploadMethod.Put ? "PUT" : "POST",
                        inputData);
                    try
                    {
                        return typeof (TOut) == typeof (EmptyResponse)
                            ? default(TOut)
                            : JsonConverter.DeserializeObject<TOut>(Encoding.UTF8.GetString(result));
                    }
                    catch (Exception exc)
                    {
                        throw new JsonParsingException(exc.Message, exc);
                    }
                }
                catch (WebException exc)
                {
                    var response = (HttpWebResponse) exc.Response;
                    if (response != null)
                    {

                        var text = stream != null ? new StreamReader(stream).ReadToEnd() : "";
                        throw new WebClientException(response.StatusCode, response.StatusDescription, text,
                            absoluteUri.ToString(), exc);
                    }
                    throw new WebClientException($"{exc.Status}:{exc.Message}", absoluteUri.ToString(), exc);
                }
            }
            finally
            {
                ctx?.Undo();
            }
        }

        public void UploadMultipart<T>(T data, IEnumerable<FileParameter> files, UploadMethod uploadMethod, Uri url,
            params ClientAttribute[] attributes)
        {
            var absoluteUri = MakeUri(url, attributes);
            traceSource.TraceInformation($"Absolute URL: {absoluteUri}");
            if (absoluteUri == null)
            {
                return;
            }
            WindowsImpersonationContext ctx = null;
            try
            {
                if (identity?.User != null)
                {
                    traceSource.TraceInformation($"Identity {identity?.Name} ({identity?.AuthenticationType})");
                    ctx = identity.Impersonate();
                }
                else
                {
                    traceSource.TraceInformation("Identity null");
                }
                var webClient = CreateWebClient();
                try
                {
                    var inputData = new[]
                    {
                        new KeyValuePair<string, object>("data", JsonConverter.SerializeObject(data))
                    };
                    var filesData = files?.Select(f => new KeyValuePair<string, object>("file", f)).ToArray();
                    if (filesData != null)
                    {
                        inputData = inputData.Union(filesData).ToArray();
                    }
                    webClient.UploadDataMultipart(absoluteUri, uploadMethod == UploadMethod.Put ? "PUT" : "POST",
                        inputData);

                }
                catch (WebException exc)
                {
                    var response = (HttpWebResponse) exc.Response;
                    if (response != null)
                    {
                        var stream = response.GetResponseStream();
                        var text = stream != null ? new StreamReader(stream).ReadToEnd() : "";
                        throw new WebClientException(response.StatusCode, response.StatusDescription, text,
                            absoluteUri.ToString(), exc);
                    }
                    throw new WebClientException($"{exc.Status}:{exc.Message}", absoluteUri.ToString(), exc);
                }
            }
            finally
            {
                ctx?.Undo();
            }
        }

        public void UploadMultipart<T>(T data, IEnumerable<FileParameter> files, UploadMethod uploadMethod, string url,
            params ClientAttribute[] attributes)
        {
            UploadMultipart(data, files, uploadMethod, GetUri(url), attributes);
        }

        private Uri GetUri(string url)
        {
            //if (!url.EndsWith("/"))
            //    url = url + "/";
            return BaseUri == null ? new Uri(url, UriKind.Absolute) : new Uri(url, UriKind.Relative);
        }

        private Uri MakeUri(Uri url, params ClientAttribute[] attributes)
        {
            if (!url.IsAbsoluteUri && BaseUri == null)
            {
                return null;
            }
            var absoluteUri = url.IsAbsoluteUri ? url : new Uri(BaseUri, url);
            if (attributes != null && attributes.Any())
            {

                var urlParts = attributes.Where(a => a.Name == null).Select(a => a.Value?.ToString());
                var queryAttributes = attributes
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

        public SimpleWebClient(WindowsIdentity identity = null)
        {
            traceSource.TraceInformation($"Identity {identity?.Name} ({identity?.AuthenticationType})");
            this.identity = identity;
        }

        public SimpleWebClient(Uri url, WindowsIdentity identity = null) : this(identity)
        {
            var path = url.AbsoluteUri;
            if (!path.EndsWith("/"))
                path = path + "/";
            BaseUri = new Uri(path, UriKind.Absolute);
        }


        public SimpleWebClient(string url, WindowsIdentity identity = null) : this(identity)
        {
            if (string.IsNullOrWhiteSpace(url)) return;
            if (!url.EndsWith("/"))
                url = url + "/";
            BaseUri = new Uri(url, UriKind.Absolute);
        }

        

        public T Get<T>(string url, params ClientAttribute[] attributes)
        {
            return Get<T>(GetUri(url), attributes);
        }

        public T Get<T, TValue>(string url, TValue parameters)
        {
            return Get<T>(url, ClientAttribute.PropertiesToAttributes(parameters));
        }

        public T Get<T>(Uri url, params ClientAttribute[] attributes)
        {
            
            var absoluteUri = MakeUri(url, attributes);
            traceSource.TraceInformation($"Absolute URL: {absoluteUri}");
            if (absoluteUri == null)
            {
                return default(T);
            }
            WindowsImpersonationContext ctx = null;
            try
            {
                if (identity?.User != null)
                {
                    traceSource.TraceInformation($"Identity {identity?.Name} ({identity?.AuthenticationType})");
                    ctx = identity.Impersonate();
                }
                else
                {
                    traceSource.TraceInformation("Identity null");
                }
                var webClient = CreateWebClient();
                webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json; charset=utf-8");
                try
                {
                    // webClient.DownloadFile(absoluteUri, "x.tmp");
                     var result = webClient.DownloadData(absoluteUri);
                    try
                    {
                        return JsonConverter.DeserializeObject<T>(Encoding.UTF8.GetString(result));
                    }
                    catch (Exception exc)
                    {
                        throw new JsonParsingException(exc.Message, exc);
                    }
                }
                catch (WebException exc)
                {
                    var response = (HttpWebResponse) exc.Response;
                    if (response != null)
                    {
                        var stream = response.GetResponseStream();
                        var text = stream != null ? new StreamReader(stream).ReadToEnd() : "";
                        throw new WebClientException(response.StatusCode, response.StatusDescription, text,
                            absoluteUri.ToString(), exc);
                    }
                    throw new WebClientException($"{exc.Status}:{exc.Message}", absoluteUri.ToString(), exc);
                }
                


            }
            finally
            {
                ctx?.Undo();
            }
        }

        public TOut Post<TIn, TOut>(TIn data, string url, params ClientAttribute[] attributes)
        {
            return Post<TIn, TOut>(data, GetUri(url), attributes);
        }

        public TOut Post<TIn, TOut>(TIn data, Uri url, params ClientAttribute[] attributes)
        {
            var absoluteUri = MakeUri(url, attributes);
            traceSource.TraceInformation($"Absolute URL: {absoluteUri}");
            if (absoluteUri == null)
            {
                return default(TOut);
            }
            WindowsImpersonationContext ctx = null;
            try
            {
                if (identity?.User != null)
                {
                    traceSource.TraceInformation($"Identity {identity?.Name} ({identity?.AuthenticationType})");
                    ctx = identity.Impersonate();
                }
                else
                {
                    traceSource.TraceInformation("Identity null");
                }
                var webClient = CreateWebClient();
                webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json; charset=utf-8");
                webClient.Headers.Add(HttpRequestHeader.Accept,"application/json");
                webClient.Encoding = Encoding.UTF8;
              
                try
                {
                    var inputData = JsonConverter.SerializeObject(data);
                    var result = webClient.UploadData(absoluteUri, "POST", Encoding.UTF8.GetBytes(inputData));
                    try
                    {
                        return typeof (TOut) == typeof (EmptyResponse)
                            ? default(TOut)
                            : JsonConverter.DeserializeObject<TOut>(Encoding.UTF8.GetString(result));
                    }
                    catch (Exception exc)
                    {
                        throw new JsonParsingException(exc.Message, exc);
                    }
                }
                catch (WebException exc)
                {
                    var response = (HttpWebResponse) exc.Response;
                    if (response != null)
                    {
                        var stream = response.GetResponseStream();
                        var text = stream != null ? new StreamReader(stream).ReadToEnd() : "";
                        throw new WebClientException(response.StatusCode, response.StatusDescription, text,
                            absoluteUri.ToString(), exc);
                    }
                    throw new WebClientException($"{exc.Status}:{exc.Message}", absoluteUri.ToString(), exc);
                }
            }
            finally
            {
                ctx?.Undo();
            }
        }

        public void Post<T>(T data, string url, params ClientAttribute[] attributes)
        {
            Post<T, EmptyResponse>(data, url, attributes);
        }

        public void Post<T>(T data, Uri url, params ClientAttribute[] attributes)
        {
            Post<T, EmptyResponse>(data, url, attributes);
        }

        public TOut Put<TIn, TOut>(TIn data, string url, params ClientAttribute[] attributes)
        {
            return Put<TIn, TOut>(data, GetUri(url), attributes);
        }

        public TOut Put<TIn, TOut>(TIn data, Uri url, params ClientAttribute[] attributes)
        {
            var absoluteUri = MakeUri(url, attributes);
            traceSource.TraceInformation($"Absolute URL: {absoluteUri}");
            if (absoluteUri == null)
            {
                return default(TOut);
            }
            WindowsImpersonationContext ctx = null;
            try
            {
                if (identity?.User != null)
                {
                    traceSource.TraceInformation($"Identity {identity?.Name} ({identity?.AuthenticationType})");
                    ctx = identity.Impersonate();
                }
                else
                {
                    traceSource.TraceInformation("Identity null");
                }
                var webClient = CreateWebClient();
                webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json; charset=utf-8");
                webClient.Headers.Add(HttpRequestHeader.Accept, "application/json");
                
                webClient.Encoding = Encoding.UTF8;
                try
                {
                    var inputData = JsonConverter.SerializeObject(data);

                    var result = webClient.UploadData(absoluteUri, "PUT", Encoding.UTF8.GetBytes(inputData));
                    try
                    {
                        return typeof (TOut) == typeof (EmptyResponse)
                            ? default(TOut)
                            : JsonConverter.DeserializeObject<TOut>(Encoding.UTF8.GetString(result));

                    }
                    catch (Exception exc)
                    {
                        throw new JsonParsingException(exc.Message, exc);
                    }
                }
                catch (WebException exc)
                {
                    var response = (HttpWebResponse) exc.Response;
                    if (response != null)
                    {
                        var stream = response.GetResponseStream();
                        var text = stream != null ? new StreamReader(stream).ReadToEnd() : "";
                        throw new WebClientException(response.StatusCode, response.StatusDescription, text,
                            absoluteUri.ToString(), exc);
                    }
                    throw new WebClientException($"{exc.Status}:{exc.Message}", absoluteUri.ToString(), exc);
                }
            }
            finally
            {
                ctx?.Undo();
            }
        }

        public void Put<T>(T data, string url, params ClientAttribute[] attributes)
        {
            Put<T, EmptyResponse>(data, url, attributes);
        }

        public void Put<T>(T data, Uri url, params ClientAttribute[] attributes)
        {
            Put<T, EmptyResponse>(data, url, attributes);
        }

        public TOut Delete<TOut>(string url, params ClientAttribute[] attributes)
        {
            return Delete<TOut>(GetUri(url), attributes);
        }

        public TOut Delete<TOut>(Uri url, params ClientAttribute[] attributes)
        {
            var absoluteUri = MakeUri(url, attributes);
            traceSource.TraceInformation($"Absolute URL: {absoluteUri}");
            if (absoluteUri == null)
            {
                return default(TOut);
            }
            WindowsImpersonationContext ctx = null;
            try
            {
                if (identity?.User != null)
                {
                    traceSource.TraceInformation($"Identity {identity?.Name} ({identity?.AuthenticationType})");
                    ctx = identity.Impersonate();
                }
                else
                {
                    traceSource.TraceInformation("Identity null");
                }
                var webClient = CreateWebClient();
                webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json; charset=utf-8");
                webClient.Headers.Add(HttpRequestHeader.Accept, "application/json");
                webClient.Encoding = Encoding.UTF8;
                try
                {
                    var result = webClient.UploadData(absoluteUri, "DELETE", new byte[] {});
                    try
                    {
                        return typeof (TOut) == typeof (EmptyResponse)
                            ? default(TOut)
                            : JsonConverter.DeserializeObject<TOut>(Encoding.UTF8.GetString(result));
                    }
                    catch (Exception exc)
                    {
                        throw new JsonParsingException(exc.Message, exc);
                    }
                }
                catch (WebException exc)
                {
                    var response = (HttpWebResponse) exc.Response;
                    if (response != null)
                    {
                        var stream = response.GetResponseStream();
                        var text = stream != null ? new StreamReader(stream).ReadToEnd() : "";
                        throw new WebClientException(response.StatusCode, response.StatusDescription, text,
                            absoluteUri.ToString(), exc);
                    }
                    throw new WebClientException($"{exc.Status}:{exc.Message}", absoluteUri.ToString(), exc);
                }
            }
            finally
            {
                ctx?.Undo();
            }
        }

        public void Delete(string url, params ClientAttribute[] attributes)
        {
            Delete<EmptyResponse>(url, attributes);
        }

        public void Delete(Uri url, params ClientAttribute[] attributes)
        {
            Delete<EmptyResponse>(url, attributes);
        }

        
    }
}

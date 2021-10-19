using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace DiCore.Lib.Web
{
    internal static class WebClientHelper
    {
        private static byte[] GetFormData(IEnumerable<KeyValuePair<string, object>> data, string boundary)
        {
            using (var formDataStream = new MemoryStream())
            {
                var needsClrf = false;
                var encoding = Encoding.UTF8;

                foreach (var param in data)
                {
                    if (needsClrf)
                        formDataStream.Write(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));

                    needsClrf = true;

                    var fileParameter = param.Value as FileParameter;
                    if (fileParameter != null)
                    {
                        var fileToUpload = fileParameter;

                        var header = $"--{boundary}\r\nContent-Disposition: form-data; " +
                                        $"name=\"{param.Key}\"; " +
                                        $"filename=\"{fileToUpload.FileName ?? param.Key}\"\r\n" +
                                        $"Content-Type: {fileToUpload.ContentType ?? "application/octet-stream"}\r\n\r\n";

                        formDataStream.Write(encoding.GetBytes(header), 0, encoding.GetByteCount(header));

                        formDataStream.Write(fileToUpload.File, 0, fileToUpload.File.Length);
                    }
                    else
                    {
                        var postData = $"--{boundary}\r\n" +
                                       $"Content-Disposition: form-data; name=\"{param.Key}\"\r\n\r\n{param.Value}";
                        formDataStream.Write(encoding.GetBytes(postData), 0, encoding.GetByteCount(postData));
                    }
                }
                string footer = $"\r\n--{boundary}--\r\n";
                formDataStream.Write(encoding.GetBytes(footer), 0, encoding.GetByteCount(footer));

                formDataStream.Position = 0;
                var formData = new byte[formDataStream.Length];
                
                formDataStream.Read(formData, 0, formData.Length);
                formDataStream.Close();
                return formData;
            }
        }

        public static byte[] UploadDataMultipart(this WebClient webClient, Uri url, string method,
            IEnumerable<KeyValuePair<string, object>> data)
        {
            using (webClient)
            {
                var boundary = $"----------DiCore.Lib.SimpleWebClient{DateTime.Now.Ticks:x}";
                var formData = GetFormData(data, boundary);
                webClient.Headers.Add(HttpRequestHeader.ContentType, "multipart/form-data; boundary=" + boundary);
                var result = webClient.UploadData(url,method, formData);
                return result;
            }
        }
    }
}

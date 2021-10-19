using System;
using System.Collections.Generic;
using System.IO;

namespace DiCore.Lib.Web
{
    public interface ISimpleWebClient
    {
        Uri BaseUri { get; }
        int Timeout { get; set; }
        MemoryStream Download(string url, params ClientAttribute[] attributes);
        MemoryStream Download(Uri url, params ClientAttribute[] attributes);
        void Upload(MemoryStream stream, UploadMethod uploadMethod, string url,
            params ClientAttribute[] attributes);
        TOut Upload<TOut>(MemoryStream stream, UploadMethod uploadMethod, string url,
            params ClientAttribute[] attributes);
        void Upload(MemoryStream stream, UploadMethod uploadMethod, Uri url, params ClientAttribute[] attributes);
        TOut Upload<TOut>(MemoryStream stream, UploadMethod uploadMethod, Uri url, params ClientAttribute[] attributes);
        T Get<T>(string url, params ClientAttribute[] attributes);
        T Get<T, TValue>(string url, TValue parameters);
        T Get<T>(Uri url, params ClientAttribute[] attributes);        
        TOut Post<TIn, TOut>(TIn data, string url, params ClientAttribute[] attributes);
        TOut Post<TIn, TOut>(TIn data, Uri url, params ClientAttribute[] attributes);
        void Post<T>(T data, string url, params ClientAttribute[] attributes);
        void Post<T>(T data, Uri url, params ClientAttribute[] attributes);
        TOut Put<TIn, TOut>(TIn data, string url, params ClientAttribute[] attributes);
        TOut Put<TIn, TOut>(TIn data, Uri url, params ClientAttribute[] attributes);
        void Put<T>(T data, string url, params ClientAttribute[] attributes);
        void Put<T>(T data, Uri url, params ClientAttribute[] attributes);
        TOut Delete<TOut>(string url, params ClientAttribute[] attributes);
        TOut Delete<TOut>(Uri url, params ClientAttribute[] attributes);
        void Delete(string url, params ClientAttribute[] attributes);
        void Delete(Uri url, params ClientAttribute[] attributes);
        void UploadMultipart<T>(T data, IEnumerable<FileParameter> files, UploadMethod uploadMethod, Uri url,
            params ClientAttribute[] attributes);
        void UploadMultipart<T>(T data, IEnumerable<FileParameter> files, UploadMethod uploadMethod, string url,
            params ClientAttribute[] attributes);

        FileResult DownloadExt(string url, params ClientAttribute[] attributes);
        FileResult DownloadExt(Uri url, params ClientAttribute[] attributes);
    }
}

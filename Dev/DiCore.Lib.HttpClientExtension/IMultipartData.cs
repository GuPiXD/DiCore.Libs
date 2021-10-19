using System;
using System.Net.Http;

namespace DiCore.Lib.HttpClientExtension
{
    /// <summary>
    /// Данные в формате multipart-formdata
    /// </summary>
    public interface IMultipartData:IDisposable
    {
        /// <summary>
        /// Название поля
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Им файла (опционально)
        /// </summary>
        string FileName { get; }
        /// <summary>
        /// Контент
        /// </summary>
        HttpContent Content { get; }
    }
}
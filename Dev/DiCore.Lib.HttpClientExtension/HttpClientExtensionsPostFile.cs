using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DiCore.Lib.HttpClientExtension
{
    public static partial class HttpClientExtensions
    {
        /// <summary>
        /// Отправка данных форм (файлов) POST запросом на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="data">Данные файла</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Task<Response> PostAsync(this HttpClient client, FileData data, string url,
            params UrlParameter[] parameters)
        {
            return PostAsync(client, data, url, CancellationToken.None, parameters);
        }

        /// <summary>
        /// Отправка данных форм (файлов) POST запросом на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="data">Данные файла</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Task<Response> PostAsync(this HttpClient client, FileData data, Uri url,
            params UrlParameter[] parameters)
        {
            return PostAsync(client, data, url, CancellationToken.None, parameters);
        }

        /// <summary>
        /// Отправка данных форм (файлов) POST запросом на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="data">Данные файла</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Task<Response> PostAsync(this HttpClient client, FileData data, string url,
            CancellationToken ct, params UrlParameter[] parameters)
        {
            return PostAsync(client, data, client.GetUri(url), ct, parameters);
        }

        /// <summary>
        /// Отправка данных форм (файлов) POST запросом на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="data">Данные файла</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        public static async Task<Response> PostAsync(this HttpClient client, FileData data, Uri url,
            CancellationToken ct, params UrlParameter[] parameters)
        {
            using (var collection = new MultipartCollection(data))
            {
                return await PostAsync(client, collection, url, ct, parameters).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Отправка данных форм (файлов) POST запросом на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="data">Данные файла</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> PostAsync<TOut>(this HttpClient client, FileData data, string url,
            params UrlParameter[] parameters)
        {
            return PostAsync<TOut>(client, data, client.GetUri(url), CancellationToken.None, parameters);
        }

        /// <summary>
        /// Отправка данных форм (файлов) POST запросом на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="data">Данные файла</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> PostAsync<TOut>(this HttpClient client, FileData data, Uri url,
            params UrlParameter[] parameters)
        {
            return PostAsync<TOut>(client, data, url, CancellationToken.None, parameters);
        }

        /// <summary>
        /// Отправка данных форм (файлов) POST запросом на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="data">Данные файла</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> PostAsync<TOut>(this HttpClient client, FileData data, string url,
            CancellationToken ct, params UrlParameter[] parameters)
        {
            return PostAsync<TOut>(client, data, client.GetUri(url), ct, parameters);
        }

        /// <summary>
        /// Отправка данных форм (файлов) POST запросом на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="data">Данные файла</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static async Task<Response<TOut>> PostAsync<TOut>(this HttpClient client, FileData data, Uri url,
            CancellationToken ct, params UrlParameter[] parameters)
        {
            using (var collection = new MultipartCollection(data))
            {
                return await PostAsync<TOut>(client, collection, url, ct, parameters).ConfigureAwait(false);
            }
        }


        /// <summary>
        /// Отправка данных форм (файлов) POST запросом на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="data">Данные файла</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Response<TOut> Post<TOut>(this HttpClient client, FileData data, Uri url,
            params UrlParameter[] parameters)
        {
            using (var collection = new MultipartCollection(data))
            {
                return Post<TOut>(client, collection, url, parameters);
            }
        }

        /// <summary>
        /// Отправка данных форм (файлов) POST запросом на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="data">Данные файла</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Response<TOut> Post<TOut>(this HttpClient client, FileData data, string url,
            params UrlParameter[] parameters)
        {
            return Post<TOut>(client, data, client.GetUri(url), parameters);
        }

        /// <summary>
        /// Отправка данных форм (файлов) POST запросом на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="data">Данные файла</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Response Post(this HttpClient client, FileData data, Uri url, params UrlParameter[] parameters)
        {
            using (var collection = new MultipartCollection(data))
            {
                return Post(client, collection, url, parameters);
            }
        }

        /// <summary>
        /// Отправка данных форм (файлов) POST запросом на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="data">Данные файла</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Response Post(this HttpClient client, FileData data, string url, params UrlParameter[] parameters)
        {
            return Post(client, data, client.GetUri(url), parameters);
        }
    }
}
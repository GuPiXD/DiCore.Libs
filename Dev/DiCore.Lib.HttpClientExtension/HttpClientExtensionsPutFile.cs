using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DiCore.Lib.HttpClientExtension
{
    public static partial class HttpClientExtensions
    {
        /// <summary>
        /// Отправка данных форм (файлов) PUT-запросом на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="data">Данные файла</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> PutAsync<TOut>(this HttpClient client, FileData data, string url,
            params UrlParameter[] parameters)
        {
            return PutAsync<TOut>(client, data, client.GetUri(url), CancellationToken.None, parameters);
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT-запросом на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="data">Данные файла</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> PutAsync<TOut>(this HttpClient client, FileData data, Uri url,
            params UrlParameter[] parameters)
        {
            return PutAsync<TOut>(client, data, url, CancellationToken.None, parameters);
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT-запросом на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="data">Данные файла</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> PutAsync<TOut>(this HttpClient client, FileData data, string url,
            CancellationToken ct, params UrlParameter[] parameters)
        {
            return PutAsync<TOut>(client, data, client.GetUri(url), ct, parameters);
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT-запросом на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="data">Данные файла</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static async Task<Response<TOut>> PutAsync<TOut>(this HttpClient client, FileData data, Uri url,
            CancellationToken ct, params UrlParameter[] parameters)
        {
            using (var collection = new MultipartCollection(data))
            {
                return await PutAsync<TOut>(client, collection, url, ct, parameters).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT-запросом на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="data">Данные файла</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Task<Response> PutAsync(this HttpClient client, FileData data, string url,
            params UrlParameter[] parameters)
        {
            return PutAsync(client, data, client.GetUri(url), CancellationToken.None, parameters);
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT-запросом на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="data">Данные файла</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Task<Response> PutAsync(this HttpClient client, FileData data, Uri url,
            params UrlParameter[] parameters)
        {
            return PutAsync(client, data, url, CancellationToken.None, parameters);
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT-запросом на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="data">Данные файла</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Task<Response> PutAsync(this HttpClient client, FileData data, string url,
            CancellationToken ct, params UrlParameter[] parameters)
        {
            return PutAsync(client, data, client.GetUri(url), ct, parameters);
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT-запросом на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="data">Данные файла</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        public static async Task<Response> PutAsync(this HttpClient client, FileData data, Uri url,
            CancellationToken ct, params UrlParameter[] parameters)
        {
            using (var collection = new MultipartCollection(data))
            {
                return await PutAsync(client, collection, url, ct, parameters).ConfigureAwait(false);
            }
        }


        /// <summary>
        /// Отправка данных форм (файлов) PUT-запросом на сервер (в синхронном режиме)
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="data">Данные файла</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Response<TOut> Put<TOut>(this HttpClient client, FileData data, string url,
            params UrlParameter[] parameters)
        {
            return Put<TOut>(client, data, client.GetUri(url), parameters);
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT-запросом на сервер (в синхронном режиме)
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="data">Данные файла</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Response<TOut> Put<TOut>(this HttpClient client, FileData data, Uri url,
            params UrlParameter[] parameters)
        {
            using (var collection = new MultipartCollection(data))
            {
                return Put<TOut>(client, collection, url, parameters);
            }
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT-запросом на сервер (в синхронном режиме)
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="data">Данные файла</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Response Put(this HttpClient client, FileData data, string url, params UrlParameter[] parameters)
        {
            return Put(client, data, client.GetUri(url), parameters);
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT-запросом на сервер (в синхронном режиме)
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="data">Данные файла</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Response Put(this HttpClient client, FileData data, Uri url, params UrlParameter[] parameters)
        {
            using (var collection = new MultipartCollection(data))
            {
                return Put(client, collection, url, parameters);
            }
        }
    }
}
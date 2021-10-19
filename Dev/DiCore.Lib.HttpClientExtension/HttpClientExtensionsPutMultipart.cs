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
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="data">Данные форм</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> PutAsync<TOut>(this HttpClient client, MultipartCollection data,
            string url, params UrlParameter[] parameters)
        {
            return PutAsync<TOut>(client, data, client.GetUri(url), CancellationToken.None, parameters);
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT-запросом на сервер
        /// </summary>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="data">Данные форм</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> PutAsync<TOut>(this HttpClient client, MultipartCollection data,
            Uri url, params UrlParameter[] parameters)
        {
            return PutAsync<TOut>(client, data, url, CancellationToken.None, parameters);
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT-запросом на сервер
        /// </summary>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="data">Данные форм</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> PutAsync<TOut>(this HttpClient client, MultipartCollection data,
            string url, CancellationToken ct, params UrlParameter[] parameters)
        {
            return PutAsync<TOut>(client, data, client.GetUri(url), ct, parameters);
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT-запросом на сервер
        /// </summary>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="data">Данные форм</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> PutAsync<TOut>(this HttpClient client, MultipartCollection data,
            Uri url, CancellationToken ct, params UrlParameter[] parameters)
        {
            return SendAsync(client, HttpMethod.Put, url, ct,
                (stringContent, response) =>
                    new Response<TOut>(
                        response.StatusCode, response.ReasonPhrase, response.Version,
                        stringContent, response.Content.Headers),
                data.Content, parameters);
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT-запросом на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="data">Данные форм</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Task<Response> PutAsync(this HttpClient client, MultipartCollection data, string url,
            params UrlParameter[] parameters)
        {
            return PutAsync(client, data, client.GetUri(url), CancellationToken.None, parameters);
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT-запросом на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="data">Данные форм</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Task<Response> PutAsync(this HttpClient client, MultipartCollection data, Uri url,
            params UrlParameter[] parameters)
        {
            return PutAsync(client, data, url, CancellationToken.None, parameters);
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT-запросом на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="data">Данные форм</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Task<Response> PutAsync(this HttpClient client, MultipartCollection data, string url,
            CancellationToken ct, params UrlParameter[] parameters)
        {
            return PutAsync(client, data, client.GetUri(url), ct, parameters);
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT-запросом на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="data">Данные форм</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Task<Response> PutAsync(this HttpClient client, MultipartCollection data, Uri url,
            CancellationToken ct, params UrlParameter[] parameters)
        {
            return SendAsync(client, HttpMethod.Put, url, ct,
                (stringContent, response) =>
                    new Response(
                        response.StatusCode, response.ReasonPhrase, response.Version,
                        stringContent, response.Content.Headers),
                data.Content, parameters);
        }


        /// <summary>
        /// Отправка данных форм (файлов) PUT-запросом на сервер (в синхронном режиме)
        /// </summary>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="data">Данные форм</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Response<TOut> Put<TOut>(this HttpClient client, MultipartCollection data, string url,
            params UrlParameter[] parameters)
        {
            return Put<TOut>(client, data, client.GetUri(url), parameters);
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT-запросом на сервер (в синхронном режиме)
        /// </summary>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="data">Данные форм</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Response<TOut> Put<TOut>(this HttpClient client, MultipartCollection data, Uri url,
            params UrlParameter[] parameters)
        {
            return Send(client, HttpMethod.Put, url,
                (stringContent, response) =>
                    new Response<TOut>(
                        response.StatusCode, response.ReasonPhrase, response.Version,
                        stringContent, response.Content.Headers),
                data.Content, parameters);
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT-запросом на сервер (в синхронном режиме)
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="data">Данные форм</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Response Put(this HttpClient client, MultipartCollection data, string url,
            params UrlParameter[] parameters)
        {
            return Put(client, data, client.GetUri(url), parameters);
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT-запросом на сервер (в синхронном режиме)
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="data">Данные форм</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Response Put(this HttpClient client, MultipartCollection data, Uri url,
            params UrlParameter[] parameters)
        {
            return Send(client, HttpMethod.Put, url,
                (stringContent, response) =>
                    new Response(
                        response.StatusCode, response.ReasonPhrase, response.Version,
                        stringContent, response.Content.Headers),
                data.Content, parameters);
        }
    }
}
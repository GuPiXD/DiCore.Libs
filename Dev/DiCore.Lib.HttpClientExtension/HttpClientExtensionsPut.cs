using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DiCore.Lib.HttpClientExtension
{
    public static partial class HttpClientExtensions
    {
        /// <summary>
        /// Отправка PUT-запроса на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> PutAsync<TOut>(this HttpClient client, object data, string url,
            params UrlParameter[] parameters)
        {
            return PutAsync<TOut>(client, data, client.GetUri(url), CancellationToken.None, parameters);
        }

        /// <summary>
        /// Отправка PUT-запроса на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> PutAsync<TOut>(this HttpClient client, object data, Uri url,
            params UrlParameter[] parameters)
        {
            return PutAsync<TOut>(client, data, url, CancellationToken.None, parameters);
        }

        /// <summary>
        /// Отправка PUT-запроса на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> PutAsync<TOut>(this HttpClient client, object data, string url,
            CancellationToken ct, params UrlParameter[] parameters)
        {
            return PutAsync<TOut>(client, data, client.GetUri(url), ct, parameters);
        }

        /// <summary>
        /// Отправка PUT-запроса на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Task<Response> PutAsync(this HttpClient client, object data, string url,
            params UrlParameter[] parameters)
        {
            return PutAsync(client, data, client.GetUri(url), CancellationToken.None, parameters);
        }

        /// <summary>
        /// Отправка PUT-запроса на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Task<Response> PutAsync(this HttpClient client, object data, Uri url,
            params UrlParameter[] parameters)
        {
            return PutAsync(client, data, url, CancellationToken.None, parameters);
        }

        /// <summary>
        /// Отправка PUT-запроса на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Task<Response> PutAsync(this HttpClient client, object data, string url, CancellationToken ct,
            params UrlParameter[] parameters)
        {
            return PutAsync(client, data, client.GetUri(url), ct, parameters);
        }

        /// <summary>
        /// Отправка PUT-запроса на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        public static async Task<Response> PutAsync(this HttpClient client, object data, Uri url, CancellationToken ct,
            params UrlParameter[] parameters)
        {
            using (var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.Unicode,
                ApplicationJsonHeader))
            {
                return await SendAsync(client, HttpMethod.Put, url, ct,
                    (stringContent, response) =>
                        new Response(
                            response.StatusCode, response.ReasonPhrase, response.Version,
                            stringContent, response.Content.Headers),
                    content, parameters).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Отправка PUT-запроса на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static async Task<Response<TOut>> PutAsync<TOut>(this HttpClient client, object data, Uri url,
            CancellationToken ct, params UrlParameter[] parameters)
        {
            using (var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.Unicode,
                ApplicationJsonHeader))
            {
                return await SendAsync(client, HttpMethod.Put, url, ct,
                    (stringContent, response) =>
                        new Response<TOut>(
                            response.StatusCode, response.ReasonPhrase, response.Version,
                            stringContent, response.Content.Headers),
                    content, parameters).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Отправка PUT-запроса на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Response<TOut> Put<TOut>(this HttpClient client, object data, string url,
            params UrlParameter[] parameters)
        {
            return Put<TOut>(client, data, client.GetUri(url), parameters);
        }

        /// <summary>
        /// Отправка PUT-запроса на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Response<TOut> Put<TOut>(this HttpClient client, object data, Uri url,
            params UrlParameter[] parameters)
        {
            using (var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.Unicode,
                ApplicationJsonHeader))
            {
                return Send(client, HttpMethod.Put, url,
                    (stringContent, response) =>
                        new Response<TOut>(
                            response.StatusCode, response.ReasonPhrase, response.Version,
                            stringContent, response.Content.Headers),
                    content, parameters);
            }
        }

        /// <summary>
        /// Отправка PUT-запроса на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <typeparam name="TIn">Тип отправляемых данных</typeparam>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Response Put(this HttpClient client, object data, string url, params UrlParameter[] parameters)
        {
            return Put(client, data, client.GetUri(url), parameters);
        }

        /// <summary>
        /// Отправка PUT-запроса на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Response Put(this HttpClient client, object data, Uri url, params UrlParameter[] parameters)
        {
            using (var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.Unicode,
                ApplicationJsonHeader))
            {
                return Send(client, HttpMethod.Put, url,
                    (stringContent, response) =>
                        new Response(
                            response.StatusCode, response.ReasonPhrase, response.Version,
                            stringContent, response.Content.Headers),
                    content, parameters);
            }
        }
    }
}
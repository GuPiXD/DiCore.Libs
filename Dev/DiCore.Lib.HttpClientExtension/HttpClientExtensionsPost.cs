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
        /// Отправка POST-запроса на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <typeparam>Тип отправляемых данных
        ///     <name>TIn</name>
        /// </typeparam>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> PostAsync<TOut>(this HttpClient client, object data, string url,
            params UrlParameter[] parameters)
        {
            return PostAsync<TOut>(client, data, client.GetUri(url), CancellationToken.None, parameters);
        }

        /// <summary>
        /// Отправка POST-запроса на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> PostAsync<TOut>(this HttpClient client, object data, Uri url,
            params UrlParameter[] parameters)
        {
            return PostAsync<TOut>(client, data, url, CancellationToken.None, parameters);
        }

        /// <summary>
        /// Отправка POST-запроса на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> PostAsync<TOut>(this HttpClient client, object data, string url,
            CancellationToken ct, params UrlParameter[] parameters)
        {
            return PostAsync<TOut>(client, data, client.GetUri(url), ct, parameters);
        }

        /// <summary>
        /// Отправка POST-запроса на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Task<Response> PostAsync(this HttpClient client, object data, string url,
            params UrlParameter[] parameters)
        {
            return PostAsync(client, data, client.GetUri(url), CancellationToken.None, parameters);
        }

        /// <summary>
        /// Отправка POST-запроса на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Task<Response> PostAsync(this HttpClient client, object data, Uri url,
            params UrlParameter[] parameters)
        {
            return PostAsync(client, data, url, CancellationToken.None, parameters);
        }

        /// <summary>
        /// Отправка POST-запроса на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Task<Response> PostAsync(this HttpClient client, object data, string url,
            CancellationToken ct, params UrlParameter[] parameters)
        {
            return PostAsync(client, data, client.GetUri(url), ct, parameters);
        }

        /// <summary>
        /// Отправка POST-запроса на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        public static async Task<Response> PostAsync(this HttpClient client, object data, Uri url, CancellationToken ct,
            params UrlParameter[] parameters)
        {
            using (var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.Unicode,
                ApplicationJsonHeader))
            {
                return await SendAsync(client, HttpMethod.Post, url, ct,
                    (stringContent, response) =>
                        new Response(
                            response.StatusCode, response.ReasonPhrase, response.Version,
                            stringContent, response.Content.Headers),
                    content, parameters).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Отправка POST-запроса на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static async Task<Response<TOut>> PostAsync<TOut>(this HttpClient client, object data, Uri url,
            CancellationToken ct, params UrlParameter[] parameters)
        {
            using (var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.Unicode,
                ApplicationJsonHeader))
            {
                return await SendAsync(client, HttpMethod.Post, url, ct,
                    (stringContent, response) =>
                        new Response<TOut>(
                            response.StatusCode, response.ReasonPhrase, response.Version,
                            stringContent, response.Content.Headers),
                    content, parameters).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Отправка POST-запроса на сервер (в синхронном режиме)
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Response<TOut> Post<TOut>(this HttpClient client, object data, string url,
            params UrlParameter[] parameters)
        {
            return Post<TOut>(client, data, client.GetUri(url), parameters);
        }

        /// <summary>
        /// Отправка POST-запроса на сервер (в синхронном режиме)
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Response<TOut> Post<TOut>(this HttpClient client, object data, Uri url,
            params UrlParameter[] parameters)
        {
            using (var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.Unicode,
                ApplicationJsonHeader))
            {
                return Send(client, HttpMethod.Post, url,
                    (stringContent, response) =>
                        new Response<TOut>(
                            response.StatusCode, response.ReasonPhrase, response.Version,
                            stringContent, response.Content.Headers),
                    content, parameters);
            }
        }

        /// <summary>
        /// Отправка POST-запроса на сервер (в синхронном режиме)
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Response Post(this HttpClient client, object data, string url, params UrlParameter[] parameters)
        {
            return Post(client, data, client.GetUri(url), parameters);
        }

        /// <summary>
        /// Отправка POST-запроса на сервер (в синхронном режиме)
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Response Post(this HttpClient client, object data, Uri url, params UrlParameter[] parameters)
        {
            using (var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.Unicode,
                ApplicationJsonHeader))
            {
                return Send(client, HttpMethod.Post, url,
                    (stringContent, response) =>
                        new Response(
                            response.StatusCode, response.ReasonPhrase, response.Version,
                            stringContent, response.Content.Headers),
                    content, parameters);
            }
        }
    }
}
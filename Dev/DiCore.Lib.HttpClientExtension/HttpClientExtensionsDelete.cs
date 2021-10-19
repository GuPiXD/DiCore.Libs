using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DiCore.Lib.HttpClientExtension
{
    public static partial class HttpClientExtensions
    {
        /// <summary>
        /// Отправка DELETE-запроса на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> DeleteAsync<TOut>(this HttpClient client, string url,
            params UrlParameter[] parameters)
        {
            return DeleteAsync<TOut>(client, client.GetUri(url), CancellationToken.None, parameters);
        }

        /// <summary>
        /// Отправка DELETE-запроса на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> DeleteAsync<TOut>(this HttpClient client, Uri url,
            params UrlParameter[] parameters)
        {
            return DeleteAsync<TOut>(client, url, CancellationToken.None, parameters);
        }

        /// <summary>
        /// Отправка DELETE-запроса на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> DeleteAsync<TOut>(this HttpClient client, string url, CancellationToken ct,
            params UrlParameter[] parameters)
        {
            return DeleteAsync<TOut>(client, client.GetUri(url), ct, parameters);
        }

        /// <summary>
        /// Отправка DELETE-запроса на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Task<Response> DeleteAsync(this HttpClient client, string url, params UrlParameter[] parameters)
        {
            return DeleteAsync(client, client.GetUri(url), CancellationToken.None, parameters);
        }

        /// <summary>
        /// Отправка DELETE-запроса на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Task<Response> DeleteAsync(this HttpClient client, Uri url, params UrlParameter[] parameters)
        {
            return DeleteAsync(client, url, CancellationToken.None, parameters);
        }

        /// <summary>
        /// Отправка DELETE-запроса на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Task<Response> DeleteAsync(this HttpClient client, string url, CancellationToken ct,
            params UrlParameter[] parameters)
        {
            return DeleteAsync(client, client.GetUri(url), ct, parameters);
        }

        /// <summary>
        /// Отправка DELETE-запроса на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Task<Response> DeleteAsync(this HttpClient client, Uri url, CancellationToken ct,
            params UrlParameter[] parameters)
        {
            return SendAsync(client, HttpMethod.Delete, url, ct,
                (stringContent, response) =>
                    new Response(
                        response.StatusCode, response.ReasonPhrase, response.Version,
                        stringContent, response.Content.Headers),
                null, parameters);
        }

        /// <summary>
        /// Отправка DELETE-запроса на сервер
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> DeleteAsync<TOut>(this HttpClient client, Uri url, CancellationToken ct,
            params UrlParameter[] parameters)
        {
            return SendAsync(client, HttpMethod.Delete, url, ct,
                (stringContent, response) =>
                    new Response<TOut>(
                        response.StatusCode, response.ReasonPhrase, response.Version,
                        stringContent, response.Content.Headers),
                null, parameters);
        }

        /// <summary>
        /// Отправка DELETE-запроса на сервер (в синхронном режиме)
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Response<TOut> Delete<TOut>(this HttpClient client, string url, params UrlParameter[] parameters)
        {
            return Delete<TOut>(client, client.GetUri(url), parameters);
        }

        /// <summary>
        /// Отправка DELETE-запроса на сервер (в синхронном режиме)
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Response<TOut> Delete<TOut>(this HttpClient client, Uri url, params UrlParameter[] parameters)
        {
            return Send(client, HttpMethod.Delete, url,
                (stringContent, response) => new Response<TOut>(
                    response.StatusCode, response.ReasonPhrase, response.Version,
                    stringContent, response.Content.Headers),
                null, parameters);
        }

        /// <summary>
        /// Отправка DELETE-запроса на сервер (в синхронном режиме)
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Response Delete(this HttpClient client, string url, params UrlParameter[] parameters)
        {
            return Delete(client, client.GetUri(url), parameters);
        }

        /// <summary>
        /// Отправка DELETE-запроса на сервер (в синхронном режиме)
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Response Delete(this HttpClient client, Uri url, params UrlParameter[] parameters)
        {
            return Send(client, HttpMethod.Delete, url,
                (stringContent, response) => new Response(
                    response.StatusCode, response.ReasonPhrase, response.Version,
                    stringContent, response.Content.Headers),
                null, parameters);
        }
    }
}
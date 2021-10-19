using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DiCore.Lib.HttpClientExtension
{
    public static partial class HttpClientExtensions
    {
        /// <summary>
        /// Отправка GET-запроса на сервер
        /// </summary>
        /// <typeparam name="TOut">Тип получаемых данных</typeparam>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токен отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> GetAsync<TOut>(this HttpClient client, string url,
            CancellationToken ct, params UrlParameter[] parameters)
        {
            return client.GetAsync<TOut>(client.GetUri(url), ct, parameters);
        }

        /// <summary>
        /// Отправка GET-запроса на сервер
        /// </summary>
        /// <typeparam name="TOut">Тип получаемых данных</typeparam>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> GetAsync<TOut>(this HttpClient client, string url,
            params UrlParameter[] parameters)
        {
            return client.GetAsync<TOut>(client.GetUri(url), parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="url"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static Task<Response<TOut>> GetAsync<TOut>(this HttpClient client, Uri url,
            params UrlParameter[] parameters)
        {
            return client.GetAsync<TOut>(url, CancellationToken.None, parameters);
        }

        /// <summary>
        /// Отправка GET-запроса на сервер
        /// </summary>
        /// <typeparam name="TOut">Тип получаемых данных</typeparam>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токен отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> GetAsync<TOut>(this HttpClient client, Uri url, CancellationToken ct,
            params UrlParameter[] parameters)
        {
            return SendAsync(client, HttpMethod.Get, url, ct,
                (stringContent, response) =>
                    new Response<TOut>(
                        response.StatusCode, response.ReasonPhrase, response.Version,
                        stringContent, response.Content.Headers),
                null, parameters);
        }


        /// <summary>
        /// Получение контента с сервера в поток, для получения файлов
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="stream">Поток для сохранения данных</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<FileResponse> GetAsync(this HttpClient client, string url, Stream stream,
            params UrlParameter[] parameters)
        {
            return GetAsync(client, url, stream, CancellationToken.None, parameters);
        }

        /// <summary>
        /// Получение контента с сервера в поток, для получения файлов
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="stream">Поток для сохранения данных</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<FileResponse> GetAsync(this HttpClient client, Uri url, Stream stream,
            params UrlParameter[] parameters)
        {
            return GetAsync(client, url, stream, CancellationToken.None, parameters);
        }

        /// <summary>
        /// Получение контента с сервера в поток, для получения файлов
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="stream">Поток для сохранения данных</param>
        /// <param name="ct">Токен отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<FileResponse> GetAsync(this HttpClient client, string url, Stream stream,
            CancellationToken ct, params UrlParameter[] parameters)
        {
            return GetAsync(client, client.GetUri( url), stream, ct,
                parameters);
        }

        /// <summary>
        /// Получение контента с сервера в поток, для получения файлов
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="stream">Поток для сохранения данных</param>
        /// <param name="ct">Токен отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static async Task<FileResponse> GetAsync(this HttpClient client, Uri url, Stream stream,
            CancellationToken ct, params UrlParameter[] parameters)
        {
            var absoluteUrl = client.MakeUri(url, parameters);
            using (var response = await client.GetAsync(absoluteUrl, ct))
            {
                using (var responseStream = await response.Content.ReadAsStreamAsync())
                {
                    responseStream.CopyTo(stream);

                    return new FileResponse(response.StatusCode, response.ReasonPhrase, response.Version,
                        response.Content.Headers);
                }
            }
        }

        /// <summary>
        /// Отправка GET-запроса на сервер (в синхронном режиме)
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <typeparam name="TOut">Тип получаемых данных</typeparam>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Response<TOut> Get<TOut>(this HttpClient client, string url, params UrlParameter[] parameters)
        {
            return Get<TOut>(client, client.GetUri(url), parameters);
        }

        /// <summary>
        /// Отправка GET-запроса на сервер (в синхронном режиме)
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <typeparam name="TOut">Тип получаемых данных</typeparam>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Response<TOut> Get<TOut>(this HttpClient client, Uri url, params UrlParameter[] parameters)
        {
            return Send(client, HttpMethod.Get, url,
                (stringContent, response) => new Response<TOut>(
                    response.StatusCode, response.ReasonPhrase, response.Version,
                    stringContent, response.Content.Headers),
                null, parameters);
        }

        /// <summary>
        /// Получение контента с сервера в поток, для получения файлов (в синхронном режиме)
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="stream">Поток для сохранения данных</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static FileResponse Get(this HttpClient client, string url, Stream stream,
            params UrlParameter[] parameters)
        {
            return Get(client, client.GetUri(url), stream, parameters);
        }

        /// <summary>
        /// Получение контента с сервера в поток, для получения файлов (в синхронном режиме)
        /// </summary>
        /// <param name="client">Клиент HTTP</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="stream">Поток для сохранения данных</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static FileResponse Get(this HttpClient client, Uri url, Stream stream, params UrlParameter[] parameters)
        {
            return Task.Run(() => GetAsync(client, url, stream, CancellationToken.None, parameters)).Result;
        }
    }
}
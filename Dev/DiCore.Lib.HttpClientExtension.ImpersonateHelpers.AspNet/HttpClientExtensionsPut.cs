using System;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace DiCore.Lib.HttpClientExtension.ImpersonateHelpers.AspNet
{
    public static partial class HttpClientExtensions
    {
        /// <summary>
        /// Отправка данных форм (файлов) PUT запросом на сервер
        /// </summary>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="data">Данные файла</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Task<Response> PutAsync(this HttpClient client, WindowsIdentity identity, FileData data,
            string url, params UrlParameter[] parameters)
        {
            using (Suppress())
            {
                return WindowsIdentity.RunImpersonated(identity.AccessToken,
                    () => client.PutAsync(data, url, parameters));
            }
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT запросом на сервер
        /// </summary>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="data">Данные файла</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Task<Response> PutAsync(this HttpClient client, WindowsIdentity identity, FileData data, Uri url,
            params UrlParameter[] parameters)
        {
            using (Suppress())
            {
                return WindowsIdentity.RunImpersonated(identity.AccessToken,
                    () => client.PutAsync(data, url, parameters));
            }
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT запросом на сервер
        /// </summary>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="data">Данные файла</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Task<Response> PutAsync(this HttpClient client, WindowsIdentity identity, FileData data,
            string url, CancellationToken ct, params UrlParameter[] parameters)
        {
            using (Suppress())
            {
                return WindowsIdentity.RunImpersonated(identity.AccessToken,
                    () => client.PutAsync(data, url, ct, parameters));
            }
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT запросом на сервер
        /// </summary>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="data">Данные файла</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Task<Response> PutAsync(this HttpClient client, WindowsIdentity identity, FileData data, Uri url,
            CancellationToken ct, params UrlParameter[] parameters)
        {
            using (Suppress())
            {
                return WindowsIdentity.RunImpersonated(identity.AccessToken,
                    () => client.PutAsync(data, url, ct, parameters));
            }
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT запросом на сервер
        /// </summary>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="data">Данные файла</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> PutAsync<TOut>(this HttpClient client, WindowsIdentity identity,
            FileData data, string url, params UrlParameter[] parameters)
        {
            using (Suppress())
            {
                return WindowsIdentity.RunImpersonated(identity.AccessToken,
                    () => client.PutAsync<TOut>(data, url, parameters));
            }
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT запросом на сервер
        /// </summary>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="data">Данные файла</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> PutAsync<TOut>(this HttpClient client, WindowsIdentity identity,
            FileData data, Uri url, params UrlParameter[] parameters)
        {
            using (Suppress())
            {
                return WindowsIdentity.RunImpersonated(identity.AccessToken,
                    () => client.PutAsync<TOut>(data, url, parameters));
            }
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT запросом на сервер
        /// </summary>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="data">Данные файла</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> PutAsync<TOut>(this HttpClient client, WindowsIdentity identity,
            FileData data, string url, CancellationToken ct, params UrlParameter[] parameters)
        {
            using (Suppress())
            {
                return WindowsIdentity.RunImpersonated(identity.AccessToken,
                    () => client.PutAsync<TOut>(data, url, ct, parameters));
            }
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT запросом на сервер
        /// </summary>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="data">Данные файла</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> PutAsync<TOut>(this HttpClient client, WindowsIdentity identity,
            FileData data, Uri url, CancellationToken ct, params UrlParameter[] parameters)
        {
            using (Suppress())
            {
                return WindowsIdentity.RunImpersonated(identity.AccessToken,
                    () => client.PutAsync<TOut>(data, url, ct, parameters));
            }
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT запросом на сервер
        /// </summary>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="data">Данные файла</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Response<TOut> Put<TOut>(this HttpClient client, WindowsIdentity identity, FileData data, Uri url,
            params UrlParameter[] parameters)
        {
            return Task.Run(() => client.PutAsync<TOut>(identity, data, url, parameters)).Result;
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT запросом на сервер
        /// </summary>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="data">Данные файла</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Response<TOut> Put<TOut>(this HttpClient client, WindowsIdentity identity, FileData data,
            string url, params UrlParameter[] parameters)
        {
            return Task.Run(() => client.PutAsync<TOut>(identity, data, url, parameters)).Result;
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT запросом на сервер
        /// </summary>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="data">Данные файла</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Response Put(this HttpClient client, WindowsIdentity identity, FileData data, Uri url,
            params UrlParameter[] parameters)
        {
            return Task.Run(() => client.PutAsync(identity, data, url, parameters)).Result;
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT запросом на сервер
        /// </summary>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="data">Данные файла</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Response Put(this HttpClient client, WindowsIdentity identity, FileData data, string url,
            params UrlParameter[] parameters)
        {
            return Task.Run(() => client.PutAsync(identity, data, url, parameters)).Result;
        }
    }
}
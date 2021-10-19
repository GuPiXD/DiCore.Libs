using System;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace DiCore.Lib.HttpClientExtension.ImpersonateHelpers
{
    public static partial class HttpClientExtensions
    {
        /// <summary>
        /// Отправка данных форм (файлов) PUT запросом на сервер
        /// </summary>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="data">Данные форм</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Task<Response> PutAsync(this HttpClient client, WindowsIdentity identity,
            MultipartCollection data, string url, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken, () => client.PutAsync(data, url, parameters));
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT запросом на сервер
        /// </summary>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="data">Данные форм</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Task<Response> PutAsync(this HttpClient client, WindowsIdentity identity,
            MultipartCollection data, Uri url, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken, () => client.PutAsync(data, url, parameters));
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT запросом на сервер
        /// </summary>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="data">Данные форм</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Task<Response> PutAsync(this HttpClient client, WindowsIdentity identity,
            MultipartCollection data, string url, CancellationToken ct, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken,
                () => client.PutAsync(data, url, ct, parameters));
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT запросом на сервер
        /// </summary>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="data">Данные форм</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Task<Response> PutAsync(this HttpClient client, WindowsIdentity identity,
            MultipartCollection data, Uri url, CancellationToken ct, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken,
                () => client.PutAsync(data, url, ct, parameters));
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT запросом на сервер
        /// </summary>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="data">Данные форм</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> PutAsync<TOut>(this HttpClient client, WindowsIdentity identity,
            MultipartCollection data, string url, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken,
                () => client.PutAsync<TOut>(data, url, parameters));
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT запросом на сервер
        /// </summary>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="data">Данные форм</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> PutAsync<TOut>(this HttpClient client, WindowsIdentity identity,
            MultipartCollection data, Uri url, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken,
                () => client.PutAsync<TOut>(data, url, parameters));
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT запросом на сервер
        /// </summary>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="data">Данные форм</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> PutAsync<TOut>(this HttpClient client, WindowsIdentity identity,
            MultipartCollection data, string url, CancellationToken ct, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken,
                () => client.PutAsync<TOut>(data, url, ct, parameters));
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT запросом на сервер
        /// </summary>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="data">Данные форм</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> PutAsync<TOut>(this HttpClient client, WindowsIdentity identity,
            MultipartCollection data, Uri url, CancellationToken ct, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken,
                () => client.PutAsync<TOut>(data, url, ct, parameters));
        }


        /// <summary>
        /// Отправка данных форм (файлов) PUT запросом на сервер
        /// </summary>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="data">Данные форм</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Response<TOut> Put<TOut>(this HttpClient client, WindowsIdentity identity,
            MultipartCollection data,
            Uri url, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken,
                () => client.Put<TOut>(data, url, parameters));
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT запросом на сервер
        /// </summary>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="data">Данные форм</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Response<TOut> Put<TOut>(this HttpClient client, WindowsIdentity identity,
            MultipartCollection data,
            string url, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken,
                () => client.Put<TOut>(data, url, parameters));
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT запросом на сервер
        /// </summary>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="data">Данные форм</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Response Put(this HttpClient client, WindowsIdentity identity, MultipartCollection data,
            Uri url, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken, () => client.Put(data, url, parameters));
        }

        /// <summary>
        /// Отправка данных форм (файлов) PUT запросом на сервер
        /// </summary>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="data">Данные форм</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        public static Response Put(this HttpClient client, WindowsIdentity identity, MultipartCollection data,
            string url, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken, () => client.Put(data, url, parameters));
        }
    }
}
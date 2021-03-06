using System;
using System.IO;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace DiCore.Lib.HttpClientExtension.ImpersonateHelpers
{
    public static partial class HttpClientExtensions
    {
        /// <summary>
        /// Отправка GET запроса на сервер, используя имперсонацию
        /// </summary>
        /// <typeparam name="T">Тип получаемых данных</typeparam>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токен отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<T>> GetAsync<T>(this HttpClient client, WindowsIdentity identity,
            string url, CancellationToken ct, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken, () => client.GetAsync<T>(url, ct, parameters));
        }

        /// <summary>
        /// Отправка GET запроса на сервер, используя имперсонацию
        /// </summary>
        /// <typeparam name="T">Тип получаемых данных</typeparam>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<T>> GetAsync<T>(this HttpClient client, WindowsIdentity identity,
            string url, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken, () => client.GetAsync<T>(url, parameters));
        }

        /// <summary>
        /// Отправка GET запроса на сервер, используя имперсонацию
        /// </summary>
        /// <typeparam name="T">Тип получаемых данных</typeparam>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токен отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<T>> GetAsync<T>(this HttpClient client, WindowsIdentity identity,
            Uri url, CancellationToken ct, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken, () => client.GetAsync<T>(url, ct, parameters));
        }

        /// <summary>
        /// Отправка GET запроса на сервер, используя имперсонацию
        /// </summary>
        /// <typeparam name="T">Тип получаемых данных</typeparam>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<T>> GetAsync<T>(this HttpClient client, WindowsIdentity identity,
            Uri url, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken, () => client.GetAsync<T>(url, parameters));
        }

        /// <summary>
        /// Отправка GET запроса на сервер, используя имперсонацию (в синхронном режиме)
        /// </summary>
        /// <typeparam name="T">Тип получаемых данных</typeparam>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Response<T> Get<T>(this HttpClient client, WindowsIdentity identity, string url,
            params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken, () => client.Get<T>(url, parameters));
        }

        /// <summary>
        /// Отправка GET запроса на сервер, используя имперсонацию (в синхронном режиме)
        /// </summary>
        /// <typeparam name="T">Тип получаемых данных</typeparam>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Response<T> Get<T>(this HttpClient client, WindowsIdentity identity, Uri url,
            params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken, () => client.Get<T>(url, parameters));
        }

        /// <summary>
        /// Отправка POST-запроса на сервер, используя имперсонацию
        /// </summary>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="client">Клиент apil</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> PostAsync<TOut>(this HttpClient client, WindowsIdentity identity,
            object data, Uri url, CancellationToken ct, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken,
                () => client.PostAsync<TOut>(data, url, ct, parameters));
        }

        /// <summary>
        /// Отправка POST-запроса на сервер, используя имперсонацию
        /// </summary>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> PostAsync<TOut>(this HttpClient client, WindowsIdentity identity,
            object data, string url, CancellationToken ct, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken,
                () => client.PostAsync<TOut>(data, url, ct, parameters));
        }

        /// <summary>
        /// Отправка POST-запроса на сервер, используя имперсонацию
        /// </summary>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> PostAsync<TOut>(this HttpClient client, WindowsIdentity identity,
            object data, Uri url, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken,
                () => client.PostAsync<TOut>(data, url, parameters));
        }

        /// <summary>
        /// Отправка POST-запроса на сервер, используя имперсонацию
        /// </summary>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> PostAsync<TOut>(this HttpClient client, WindowsIdentity identity,
            object data, string url, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken,
                () => client.PostAsync<TOut>(data, url, parameters));
        }

        /// <summary>
        /// Отправка POST-запроса на сервер, используя имперсонацию
        /// </summary>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response> PostAsync(this HttpClient client, WindowsIdentity identity, object data, Uri url,
            CancellationToken ct, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken,
                () => client.PostAsync(data, url, ct, parameters));
        }

        /// <summary>
        /// Отправка POST-запроса на сервер, используя имперсонацию
        /// </summary>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response> PostAsync(this HttpClient client, WindowsIdentity identity, object data,
            string url, CancellationToken ct, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken,
                () => client.PostAsync(data, url, ct, parameters));
        }

        /// <summary>
        /// Отправка POST-запроса на сервер, используя имперсонацию
        /// </summary>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response> PostAsync(this HttpClient client, WindowsIdentity identity, object data,
            Uri url, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken, () => client.PostAsync(data, url, parameters));
        }

        /// <summary>
        /// Отправка POST-запроса на сервер, используя имперсонацию
        /// </summary>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response> PostAsync(this HttpClient client, WindowsIdentity identity, object data,
            string url, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken, () => client.PostAsync(data, url, parameters));
        }

        /// <summary>
        /// Отправка POST-запроса на сервер, используя имперсонацию (в синхронном режиме)
        /// </summary>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Response<TOut> Post<TOut>(this HttpClient client, WindowsIdentity identity, object data, Uri url,
            params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken,
                () => client.Post<TOut>(data, url, parameters));
        }

        /// <summary>
        /// Отправка POST-запроса на сервер, используя имперсонацию (в синхронном режиме)
        /// </summary>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Response<TOut> Post<TOut>(this HttpClient client, WindowsIdentity identity, object data,
            string url, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken,
                () => client.Post<TOut>(data, url, parameters));
        }


        /// <summary>
        /// Отправка POST-запроса на сервер, используя имперсонацию (в синхронном режиме)
        /// </summary>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Response Post(this HttpClient client, WindowsIdentity identity, object data, Uri url,
            params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken, () => client.Post(data, url, parameters));
        }

        /// <summary>
        /// Отправка POST-запроса на сервер, используя имперсонацию (в синхронном режиме)
        /// </summary>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Response Post(this HttpClient client, WindowsIdentity identity, object data, string url,
            params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken, () => client.Post(data, url, parameters));
        }

        /// <summary>
        /// Отправка PUT-запроса на сервер, используя имперсонацию
        /// </summary>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <param name="client">Клиент api</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> PutAsync<TOut>(this HttpClient client, WindowsIdentity identity, object data,
            string url, CancellationToken ct, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken,
                () => client.PutAsync<TOut>(data, url, ct, parameters));
        }

        /// <summary>
        /// Отправка PUT-запроса на сервер, используя имперсонацию
        /// </summary>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <param name="client">Клиент api</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> PutAsync<TOut>(this HttpClient client, WindowsIdentity identity, object data,
            Uri url, CancellationToken ct, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken,
                () => client.PutAsync<TOut>(data, url, ct, parameters));
        }

        /// <summary>
        /// Отправка PUT-запроса на сервер, используя имперсонацию
        /// </summary>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <param name="client">Клиент api</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> PutAsync<TOut>(this HttpClient client, WindowsIdentity identity, object data,
            Uri url, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken,
                () => client.PutAsync<TOut>(data, url, parameters));
        }

        /// <summary>
        /// Отправка PUT-запроса на сервер, используя имперсонацию
        /// </summary>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <param name="client">Клиент api</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> PutAsync<TOut>(this HttpClient client, WindowsIdentity identity, object data,
            string url, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken,
                () => client.PutAsync<TOut>(data, url, parameters));
        }

        /// <summary>
        /// Отправка PUT-запроса на сервер, используя имперсонацию
        /// </summary>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <param name="client">Клиент api</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response> PutAsync(this HttpClient client, WindowsIdentity identity, object data, Uri url,
            CancellationToken ct, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken,
                () => client.PutAsync(data, url, ct, parameters));
        }

        /// <summary>
        /// Отправка PUT-запроса на сервер, используя имперсонацию
        /// </summary>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <param name="client">Клиент api</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response> PutAsync(this HttpClient client, WindowsIdentity identity, object data, string url,
            CancellationToken ct, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken,
                () => client.PutAsync(data, url, ct, parameters));
        }

        /// <summary>
        /// Отправка PUT-запроса на сервер, используя имперсонацию
        /// </summary>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <param name="client">Клиент api</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response> PutAsync(this HttpClient client, WindowsIdentity identity, object data, Uri url,
            params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken, () => client.PutAsync(data, url, parameters));
        }

        /// <summary>
        /// Отправка PUT-запроса на сервер, используя имперсонацию
        /// </summary>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <param name="client">Клиент api</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response> PutAsync(this HttpClient client, WindowsIdentity identity, object data, string url,
            params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken, () => client.PutAsync(data, url, parameters));
        }

        /// <summary>
        /// Отправка PUT-запроса на сервер, используя имперсонацию (в синхронном режиме)
        /// </summary>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <param name="client">Клиент api</param>
        /// <returns>Результат запроса</returns>
        public static Response<TOut> Put<TOut>(this HttpClient client, WindowsIdentity identity, object data, Uri url,
            params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken, () => client.Put<TOut>(data, url, parameters));
        }

        /// <summary>
        /// Отправка PUT-запроса на сервер, используя имперсонацию (в синхронном режиме)
        /// </summary>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <param name="client">Клиент api</param>
        /// <returns>Результат запроса</returns>
        public static Response<TOut> Put<TOut>(this HttpClient client, WindowsIdentity identity, object data,
            string url, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken, () => client.Put<TOut>(data, url, parameters));
        }

        /// <summary>
        /// Отправка PUT-запроса на сервер, используя имперсонацию (в синхронном режиме)
        /// </summary>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <param name="client">Клиент api</param>
        /// <returns>Результат запроса</returns>
        public static Response Put(this HttpClient client, WindowsIdentity identity, object data, Uri url,
            params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken, () => client.Put(data, url, parameters));
        }

        /// <summary>
        /// Отправка PUT-запроса на сервер, используя имперсонацию (в синхронном режиме)
        /// </summary>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="data">Данные для отправки</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <param name="client">Клиент api</param>
        /// <returns>Результат запроса</returns>
        public static Response Put(this HttpClient client, WindowsIdentity identity, object data, string url,
            params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken, () => client.Put(data, url, parameters));
        }

        /// <summary>
        /// Отправка DELETE-запроса на сервер, используя имперсонацию
        /// </summary>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <param name="client">Клиент api</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> DeleteAsync<TOut>(this HttpClient client, WindowsIdentity identity, Uri url,
            CancellationToken ct, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken,
                () => client.DeleteAsync<TOut>(url, ct, parameters));
        }

        /// <summary>
        /// Отправка DELETE-запроса на сервер, используя имперсонацию
        /// </summary>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <param name="client">Клиент api</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> DeleteAsync<TOut>(this HttpClient client, WindowsIdentity identity,
            string url, CancellationToken ct, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken,
                () => client.DeleteAsync<TOut>(url, ct, parameters));
        }

        /// <summary>
        /// Отправка DELETE-запроса на сервер, используя имперсонацию
        /// </summary>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <param name="client">Клиент api</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> DeleteAsync<TOut>(this HttpClient client, WindowsIdentity identity, Uri url,
            params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken,
                () => client.DeleteAsync<TOut>(url, parameters));
        }

        /// <summary>
        /// Отправка DELETE-запроса на сервер, используя имперсонацию
        /// </summary>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <param name="client">Клиент api</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response<TOut>> DeleteAsync<TOut>(this HttpClient client, WindowsIdentity identity,
            string url, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken,
                () => client.DeleteAsync<TOut>(url, parameters));
        }

        /// <summary>
        /// Отправка DELETE-запроса на сервер, используя имперсонацию
        /// </summary>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <param name="client">Клиент api</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response> DeleteAsync(this HttpClient client, WindowsIdentity identity, Uri url,
            CancellationToken ct, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken, () => client.DeleteAsync(url, ct, parameters));
        }

        /// <summary>
        /// Отправка DELETE-запроса на сервер, используя имперсонацию
        /// </summary>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="ct">Токе отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <param name="client">Клиент api</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response> DeleteAsync(this HttpClient client, WindowsIdentity identity, string url,
            CancellationToken ct, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken, () => client.DeleteAsync(url, ct, parameters));
        }

        /// <summary>
        /// Отправка DELETE-запроса на сервер, используя имперсонацию
        /// </summary>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <param name="client">Клиент api</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response> DeleteAsync(this HttpClient client, WindowsIdentity identity, Uri url,
            params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken, () => client.DeleteAsync(url, parameters));
        }

        /// <summary>
        /// Отправка DELETE-запроса на сервер, используя имперсонацию
        /// </summary>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <param name="client">Клиент api</param>
        /// <returns>Результат запроса</returns>
        public static Task<Response> DeleteAsync(this HttpClient client, WindowsIdentity identity, string url,
            params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken, () => client.DeleteAsync(url, parameters));
        }

        /// <summary>
        /// Отправка DELETE-запроса на сервер, используя имперсонацию (в синхронном режиме)
        /// </summary>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <param name="client">Клиент api</param>
        /// <returns>Результат запроса</returns>
        public static Response<TOut> Delete<TOut>(this HttpClient client, WindowsIdentity identity, Uri url,
            params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken, () => client.Delete<TOut>(url, parameters));
        }

        /// <summary>
        /// Отправка DELETE-запроса на сервер, используя имперсонацию (в синхронном режиме)
        /// </summary>
        /// <typeparam name="TOut">Тип получаемых даных</typeparam>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <param name="client">Клиент api</param>
        /// <returns>Результат запроса</returns>
        public static Response<TOut> Delete<TOut>(this HttpClient client, WindowsIdentity identity, string url,
            params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken, () => client.Delete<TOut>(url, parameters));
        }

        /// <summary>
        /// Отправка DELETE-запроса на сервер, используя имперсонацию (в синхронном режиме)
        /// </summary>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <param name="client">Клиент api</param>
        /// <returns>Результат запроса</returns>
        public static Response Delete(this HttpClient client, WindowsIdentity identity, Uri url,
            params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken, () => client.Delete(url, parameters));
        }

        /// <summary>
        /// Отправка DELETE-запроса на сервер, используя имперсонацию (в синхронном режиме)
        /// </summary>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <param name="client">Клиент api</param>
        /// <returns>Результат запроса</returns>
        public static Response Delete(this HttpClient client, WindowsIdentity identity, string url,
            params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken, () => client.Delete(url, parameters));
        }

        /// <summary>
        /// Получение контента с сервера в поток, для получения файлов
        /// </summary>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="stream">Поток для сохранения данных</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<FileResponse> GetAsync(this HttpClient client, WindowsIdentity identity, string url,
            Stream stream, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken,
                () => client.GetAsync(url, stream, parameters));
        }

        /// <summary>
        /// Получение контента с сервера в поток, для получения файлов
        /// </summary>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="stream">Поток для сохранения данных</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<FileResponse> GetAsync(this HttpClient client, WindowsIdentity identity, Uri url,
            Stream stream, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken,
                () => client.GetAsync(url, stream, parameters));
        }

        /// <summary>
        /// Получение контента с сервера в поток, для получения файлов
        /// </summary>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="stream">Поток для сохранения данных</param>
        /// <param name="ct">Токен отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<FileResponse> GetAsync(this HttpClient client, WindowsIdentity identity, string url,
            Stream stream, CancellationToken ct, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken,
                () => client.GetAsync(url, stream, ct, parameters));
        }

        /// <summary>
        /// Получение контента с сервера в поток, для получения файлов
        /// </summary>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="stream">Поток для сохранения данных</param>
        /// <param name="ct">Токен отмены</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static Task<FileResponse> GetAsync(this HttpClient client, WindowsIdentity identity, Uri url,
            Stream stream, CancellationToken ct, params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken,
                () => client.GetAsync(url, stream, ct, parameters));
        }

        /// <summary>
        /// Получение контента с сервера в поток, для получения файлов (в синхронном режиме)
        /// </summary>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="stream">Поток для сохранения данных</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static FileResponse Get(this HttpClient client, WindowsIdentity identity, string url, Stream stream,
            params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken, () => client.Get(url, stream, parameters));
        }

        /// <summary>
        /// Получение контента с сервера в поток, для получения файлов (в синхронном режиме)
        /// </summary>
        /// <param name="client">Клиент api</param>
        /// <param name="identity">Учетные данные пользователя</param>
        /// <param name="url">Адрес запроса</param>
        /// <param name="stream">Поток для сохранения данных</param>
        /// <param name="parameters">Параметры запроса</param>
        /// <returns>Результат запроса</returns>
        public static FileResponse Get(this HttpClient client, WindowsIdentity identity, Uri url, Stream stream,
            params UrlParameter[] parameters)
        {
            return WindowsIdentity.RunImpersonated(identity.AccessToken, () => client.Get(url, stream, parameters));
        }
    }
}
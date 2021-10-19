using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reflection;

namespace DiCore.Lib.HttpClientExtension
{
    /// <summary>
    /// Набор данных multipart-formdata
    /// </summary>
    public class MultipartCollection : IDisposable
    {
        /// <summary>
        /// Форматирование
        /// </summary>
        public IFormatProvider FormatProvider { get; }

        private readonly IMultipartData[] datas;


        private IEnumerable<IMultipartData> GetDatas(string name, Type type, object value, IFormatProvider provider)
        {
            if (value == null)
                yield break;
            if (type == typeof(FileContent))
            {
                yield return new FileData((FileContent) value, name);
            }
            else if (type == typeof(string) || type.IsValueType)
            {
                yield return new StringData(name, value, provider);
            }
            else if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                foreach (var item in (IEnumerable) value)
                {
                    foreach (var multipartData in GetDatas(name, type.GetElementType(), item, provider))
                    {
                        yield return multipartData;
                    }
                }
            }
            else
            {
                foreach (var multipartData in GetDatas(value, provider, name))
                {
                    yield return multipartData;
                }
            }
        }


        private IEnumerable<IMultipartData> GetDatas(object data, IFormatProvider provider = null, string prefix = null)
        {
            var type = data.GetType();
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
            foreach (var property in properties)
            {
                var name = string.IsNullOrWhiteSpace(prefix) ? property.Name : $"{prefix}.{property.Name}";
                var val = property.GetValue(data);
                foreach (var multipartData in GetDatas(name, property.PropertyType, val, provider))
                {
                    yield return multipartData;
                }
            }
        }


        private IEnumerable<IMultipartData> IterateDatas(object data, IFormatProvider formatProvider,
            IMultipartData[] datas)
        {
            if (data != null)
            {
                if (data is IMultipartData multipartData)
                {
                    yield return multipartData;
                }
                else
                {
                    foreach (var multipartDataItem in GetDatas(data, formatProvider))
                    {
                        yield return multipartDataItem;
                    }
                }
            }

            if (datas == null)
            {
                yield break;
            }

            foreach (var item in datas)
            {
                yield return item;
            }
        }


        /// <summary>
        /// Создание набора данных
        /// </summary>
        /// <param name="data">Данные (объект)</param>
        /// <param name="datas">Данные </param>
        public MultipartCollection(object data, params IMultipartData[] datas) : this(data, null, datas)
        {
        }

        /// <summary>
        /// Создание набора данных
        /// </summary>
        /// <param name="data">Данные (объект)</param>
        /// <param name="formatProvider">Форматирование</param>
        /// <param name="datas">Дполнительные данные</param>
        public MultipartCollection(object data, IFormatProvider formatProvider, params IMultipartData[] datas)
        {
            FormatProvider = formatProvider;
            this.datas = IterateDatas(data, formatProvider, datas).ToArray();
        }

        /// <summary>
        /// Создание набора данных
        /// </summary>
        /// <param name="datas">Данные </param>
        public MultipartCollection(params IMultipartData[] datas) : this(null, null, datas)
        {
        }


        /// <summary>
        /// Удаление
        /// </summary>
        public void Dispose()
        {
            foreach (var multipartItem in datas)
            {
                multipartItem.Dispose();
            }
        }

        /// <summary>
        /// Контент HTTP
        /// </summary>
        public HttpContent Content
        {
            get
            {
                var content =
                    new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture));
                foreach (var multipartParameter in datas)
                {
                    if (string.IsNullOrWhiteSpace(multipartParameter.FileName))
                    {
                        content.Add(multipartParameter.Content, multipartParameter.Name);
                    }
                    else
                    {
                        content.Add(multipartParameter.Content, multipartParameter.Name, multipartParameter.FileName);
                    }
                }

                return content;
            }
        }
    }
}
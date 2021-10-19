using System;
using System.Globalization;
using System.Net.Http;

namespace DiCore.Lib.HttpClientExtension
{
    /// <summary>
    /// Строковые данные multipart-formdata
    /// </summary>
    public class StringData : IMultipartData
    {
        /// <summary>
        /// Удаление
        /// </summary>
        public void Dispose()
        {
        }
        /// <summary>
        /// Создание
        /// </summary>
        /// <param name="name">Название поля</param>
        /// <param name="value"></param>
        /// <param name="formatProvider"></param>
        public StringData(string name, object value, IFormatProvider formatProvider = null)
        {
            Name = name;
            Value = value;
            FormatProvider = formatProvider;
        }
        /// <summary>
        /// Название поля
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Значение поля
        /// </summary>
        public object Value { get; }
        /// <summary>
        /// Форматирование
        /// </summary>
        public IFormatProvider FormatProvider { get; }
        /// <summary>
        /// Имя файла (всегда null)
        /// </summary>
        public string FileName => null;
        /// <summary>
        /// Контент HTTP
        /// </summary>
        public HttpContent Content =>
            new StringContent(Convert.ToString(Value, FormatProvider ?? CultureInfo.CurrentCulture));
    }
}
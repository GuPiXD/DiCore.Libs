using System;
using System.Collections.Generic;
using System.Reflection;

namespace DiCore.Lib.HttpClientExtension
{
    /// <summary>
    /// Параметр url
    /// </summary>
    public class UrlParameter
    {
        /// <summary>
        /// Пустой параметр
        /// </summary>
        public static UrlParameter NullParameter => new UrlParameter();
        /// <summary>
        /// Создание параметра
        /// </summary>
        /// <param name="name">Название</param>
        /// <param name="value">Значение</param>
        public UrlParameter(string name, long value)
        {
            Name = name;
            Value = value;
        }
        /// <summary>
        /// Создание параметра
        /// </summary>
        /// <param name="name">Название</param>
        /// <param name="value">Значение</param>
        public UrlParameter(string name, long? value)
        {
            Name = name;
            Value = value;
        }
        /// <summary>
        /// Создание параметра
        /// </summary>
        /// <param name="name">Название</param>
        /// <param name="value">Значение</param>
        public UrlParameter(string name, string value)
        {
            Name = name;
            Value = value;
        }
        /// <summary>
        /// Создание параметра
        /// </summary>
        /// <param name="name">Название</param>
        /// <param name="value">Значение</param>
        public UrlParameter(string name, Guid value)
        {
            Name = name;
            Value = value;
        }
        /// <summary>
        /// Создание параметра
        /// </summary>
        /// <param name="name">Название</param>
        /// <param name="value">Значение</param>
        public UrlParameter(string name, Guid? value)
        {
            Name = name;
            Value = value;
        }
        /// <summary>
        /// Создание параметра
        /// </summary>
        /// <param name="name">Название</param>
        /// <param name="value">Значение</param>
        public UrlParameter(string name, double value)
        {
            Name = name;
            Value = value;
        }
        /// <summary>
        /// Создание параметра
        /// </summary>
        /// <param name="name">Название</param>
        /// <param name="value">Значение</param>
        public UrlParameter(string name, double? value)
        {
            Name = name;
            Value = value;
        }
        /// <summary>
        /// Создание параметра
        /// </summary>
        /// <param name="name">Название</param>
        /// <param name="value">Значение</param>
        public UrlParameter(string name, DateTime value)
        {
            Name = name;
            Value = value;
        }
        /// <summary>
        /// Создание параметра
        /// </summary>
        /// <param name="name">Название</param>
        /// <param name="value">Значение</param>
        public UrlParameter(string name, DateTime? value)
        {
            Name = name;
            Value = value;
        }
        /// <summary>
        /// Создание параметра
        /// </summary>
        /// <param name="name">Название</param>
        /// <param name="value">Значение</param>
        private UrlParameter(long value)
        {
            Name = null;
            Value = value;
        }

        /// <summary>
        /// Создание параметра
        /// </summary>
        /// <param name="value">Значение</param>
        private UrlParameter(string value)
        {
            Name = null;
            Value = value;
        }
        /// <summary>
        /// Создание параметра
        /// </summary>
        /// <param name="value">Значение</param>
        private UrlParameter(Guid value)
        {
            Name = null;
            Value = value;
        }
        /// <summary>
        /// Создание параметра
        /// </summary>
        /// <param name="value">Значение</param>
        private UrlParameter()
        {
            Name = null;
            Value = null;
        }
        /// <summary>
        /// Название
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Значение
        /// </summary>
        public object Value { get; }
        /// <summary>
        /// Преобразование в url-параметр
        /// </summary>
        /// <param name="value">Значение</param>
        public static implicit operator UrlParameter(string value)
        {
            return new UrlParameter(value);
        }
        /// <summary>
        /// Преобразование в url-параметр
        /// </summary>
        /// <param name="value">Значение</param>
        public static implicit operator UrlParameter(long value)
        {
            return new UrlParameter(value);
        }
        /// <summary>
        /// Преобразование в url-параметр
        /// </summary>
        /// <param name="value">Значение</param>
        public static implicit operator UrlParameter(Guid value)
        {
            return new UrlParameter(value);
        }


        internal static UrlParameter[] PropertiesToAttributes(params object[] objects)
        {
            var attributes = new List<UrlParameter>();

            foreach (var o in objects)
            {
                var properties =
                    o.GetType()
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);


                foreach (var property in properties)
                {

                    attributes.Add(new UrlParameter(property.Name, property.GetValue(o, null)?.ToString()));
                }
            }

            return attributes.ToArray();
        }
    }
}
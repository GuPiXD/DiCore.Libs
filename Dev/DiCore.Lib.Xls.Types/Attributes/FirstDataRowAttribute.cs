using System;
using System.Linq;
using System.Reflection;

namespace DiCore.Lib.Xls.Types.Attributes
{
    /// <summary>
    /// Атрибут, позволяющий задать номер строки с которой начинаются строки с данными
    /// </summary>
    public class FirstDataRowAttribute : Attribute
    {
        public FirstDataRowAttribute(int value)
        {
            Value = value;
        }

        public int Value { get; set; }

        public static int? GetFirstDataRow<T>()
        {
            var attribute = typeof(T).GetCustomAttributes(typeof(FirstDataRowAttribute), true).
                FirstOrDefault() as FirstDataRowAttribute;
            if (attribute != null)
                return attribute.Value;
            return null;
        }

        public static int? GetFirstDataRow(PropertyInfo propertyInfo)
        {
            object[] attrs = propertyInfo.GetCustomAttributes(true);
            foreach (object attr in attrs)
            {
                var attribute = attr as FirstDataRowAttribute;
                if (attribute != null)
                {
                    return attribute.Value;
                }
            }
            return null;
        }

        public static int? GetFirstDataRow(Type type)
        {
            object[] attrs = type.GetCustomAttributes(true);
            foreach (object attr in attrs)
            {
                var attribute = attr as FirstDataRowAttribute;
                if (attribute != null)
                {
                    return attribute.Value;
                }
            }
            return null;
        }

        public static bool Exist(PropertyInfo propertyInfo)
        {
            object[] attrs = propertyInfo.GetCustomAttributes(true);
            foreach (object attr in attrs)
            {
                var attribute = attr as FirstDataRowAttribute;
                if (attribute != null)
                    return true;
            }
            return false;
        }
    }
}

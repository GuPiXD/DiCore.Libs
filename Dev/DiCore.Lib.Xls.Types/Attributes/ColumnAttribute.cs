using System;
using System.Linq;
using System.Reflection;

namespace DiCore.Lib.Xls.Types.Attributes
{
    /// <summary>
    /// Атрибут, позволяющий задать имя столбца из которого будут 
    /// получаться данные для данного свойства
    /// </summary>
    public class ColumnAttribute : Attribute
    {
        public ColumnAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public static string GetColumnName<T>()
        {
            var attribute = typeof(T).GetCustomAttributes(typeof(ColumnAttribute), true).FirstOrDefault() as ColumnAttribute;
            if (attribute != null)
                return attribute.Name;
            return null;
        }

        public static string GetColumnName(PropertyInfo propertyInfo)
        {
            object[] attrs = propertyInfo.GetCustomAttributes(true);
            foreach (object attr in attrs)
            {
                var attribute = attr as ColumnAttribute;
                if (attribute != null)
                {
                    return attribute.Name;
                }
            }
            return null;
        }

        public static bool Exist(PropertyInfo propertyInfo)
        {
            object[] attrs = propertyInfo.GetCustomAttributes(true);
            foreach (object attr in attrs)
            {
                var attribute = attr as ColumnAttribute;
                if (attribute != null)
                    return true;
            }
            return false;
        }
    }
}

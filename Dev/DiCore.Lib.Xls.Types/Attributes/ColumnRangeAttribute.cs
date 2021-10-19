using System;
using System.Linq;
using System.Reflection;

namespace DiCore.Lib.Xls.Types.Attributes
{
    /// <summary>
    /// Атрибут, позволяющий задать диапазон столбцов, которые будут анализироваться для получения данных
    /// значение Min должно быть в диапазоне "A-Z"
    /// значение Max должно быть в диапазоне "A-ZZ"
    /// </summary>
    public class ColumnRangeAttribute: Attribute
    {
        public ColumnRangeAttribute(string min, string max)
        {
            Min = min;
            Max = max;
        }

        public string Min { get; set; }
        public string Max { get; set; }

        public static ColumnRangeAttribute Get(PropertyInfo propertyInfo)
        {
            object[] attrs = propertyInfo.GetCustomAttributes(true);
            foreach (object attr in attrs)
            {
                var attribute = attr as ColumnRangeAttribute;
                if (attribute != null)
                {
                    return attribute;
                }
            }
            return null;
        }

        public static ColumnRangeAttribute Get<T>()
        {
            var attribute = typeof(T).GetCustomAttributes(typeof(FirstDataRowAttribute), true).
                FirstOrDefault() as ColumnRangeAttribute;
            if (attribute != null)
                return attribute;
            return null;
        }
    }
}

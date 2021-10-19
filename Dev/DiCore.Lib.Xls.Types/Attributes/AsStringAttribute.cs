using System;
using System.Reflection;

namespace DiCore.Lib.Xls.Types.Attributes
{
    /// <summary>
    /// Атрибут, позволяющий получить значение типа string (независимо от типа описанного в классе данных)
    /// Предназначен для использования совместно с методом обратного вызова BeforeSetValueDelegate beforeSetValue
    /// задаваемого в конструкторе класса XlsLoader.
    /// </summary>
    public class AsStringAttribute : Attribute
    {
        /// <summary>
        /// Проверка наличия атрибута
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        public static bool Exist(PropertyInfo propertyInfo)
        {
            object[] attrs = propertyInfo.GetCustomAttributes(true);
            foreach (object attr in attrs)
            {
                var attribute = attr as AsStringAttribute;
                if (attribute != null)
                    return true;
            }
            return false;
        }
    }
}

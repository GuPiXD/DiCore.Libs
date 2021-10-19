using System;
using System.Linq;
using System.Reflection;

namespace DiCore.Lib.Xls.Types.Attributes
{
    /// <summary>
    /// Атрибут, позволяющий задать имя страницы в документе
    /// </summary>
    public class SheetAttribute: Attribute
    {
        public SheetAttribute(string name)
        {
            Name = name;
        }
        public string Name { get; set; }

        public static string GetSheetName<T>()
        {
            var attribute = typeof(T).GetCustomAttributes(typeof(SheetAttribute), true).FirstOrDefault() as SheetAttribute;
            if (attribute != null)
                return attribute.Name;
            return null;
        }

        public static string GetSheetName(PropertyInfo propertyInfo)
        {
            object[] attrs = propertyInfo.GetCustomAttributes(true);
            foreach (object attr in attrs)
            {
                var attribute = attr as SheetAttribute;
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
                var attribute = attr as SheetAttribute;
                if (attribute != null)
                    return true;
            }
            return false;
        }
    }
}

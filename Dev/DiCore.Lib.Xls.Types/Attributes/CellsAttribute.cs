using System;
using System.Linq;
using System.Reflection;

namespace DiCore.Lib.Xls.Types.Attributes
{
    public class CellsAttribute : Attribute
    {
        public CellsAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public static string GetCellsName<T>()
        {
            var attribute = typeof(T).GetCustomAttributes(typeof(CellsAttribute), true).FirstOrDefault() as CellsAttribute;
            if (attribute != null)
                return attribute.Name;
            return null;
        }

        public static string GetCellsName(PropertyInfo propertyInfo)
        {
            object[] attrs = propertyInfo.GetCustomAttributes(true);
            foreach (object attr in attrs)
            {
                var attribute = attr as CellsAttribute;
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
                var attribute = attr as CellsAttribute;
                if (attribute != null)
                    return true;
            }
            return false;
        }
    }
}

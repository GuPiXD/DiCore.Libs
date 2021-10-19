using System;
using System.Reflection;

namespace DiCore.Lib.Xls.Types.Attributes
{
    /// <summary>
    /// Атрибут, позволяющий рассматривать входные значения как допускающие null
    /// Поля типов, не допускающих null должны также быть объявлены как nullable
    /// </summary>
    public class AllowNullAttribute : Attribute
    {
        public static bool Exist(PropertyInfo propertyInfo)
        {
            object[] attrs = propertyInfo.GetCustomAttributes(true);
            foreach (object attr in attrs)
            {
                var attribute = attr as AllowNullAttribute;
                if (attribute != null)
                    return true;
            }
            return false;
        }
    }
}

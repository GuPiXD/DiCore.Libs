using System;
using System.Linq;
using System.Reflection;

namespace DiCore.Lib.Xls.Types.Attributes
{
    public class PictureSheetAttribute: Attribute
    {
        public PictureSheetAttribute(string name, int minCount, int maxCount)
        {
            Name = name;
            MinCount = minCount;
            MaxCount = maxCount;
        }

        public PictureSheetAttribute(string name, bool isLabel)
        {
            Name = name;
            IsLabel = isLabel;
        }

        public string Name { get; set; }

        public int MinCount { get; set; }

        public int MaxCount { get; set; }

        public bool? IsLabel { get; set; } = null;

        public static string GetSheetName<T>()
        {
            var attribute = typeof(T).GetCustomAttributes(typeof(PictureSheetAttribute), true).FirstOrDefault() as PictureSheetAttribute;
            if (attribute != null)
                return attribute.Name;
            return null;
        }

        public static string GetSheetName(PropertyInfo propertyInfo)
        {
            object[] attrs = propertyInfo.GetCustomAttributes(true);
            foreach (object attr in attrs)
            {
                var attribute = attr as PictureSheetAttribute;
                if (attribute != null)
                {
                    return attribute.Name;
                }
            }
            return null;
        }

        public static int? GetPictureMinCount(PropertyInfo propertyInfo)
        {
            object[] attrs = propertyInfo.GetCustomAttributes(true);
            foreach (object attr in attrs)
            {
                var attribute = attr as PictureSheetAttribute;
                if (attribute != null)
                {
                    return attribute.MinCount;
                }
            }
            return null;
        }

        public static int GetPictureMaxCount(PropertyInfo propertyInfo)
        {
            object[] attrs = propertyInfo.GetCustomAttributes(true);
            foreach (object attr in attrs)
            {
                var attribute = attr as PictureSheetAttribute;
                if (attribute != null)
                {
                    return attribute.MaxCount;
                }
            }
            return 0;
        }

        public static bool? GetPictureIsLabel(PropertyInfo propertyInfo)
        {
            object[] attrs = propertyInfo.GetCustomAttributes(true);
            foreach (object attr in attrs)
            {
                var attribute = attr as PictureSheetAttribute;
                if (attribute != null)
                {
                    return attribute.IsLabel;
                }
            }
            return null;
        }

        public static bool Exist(PropertyInfo propertyInfo)
        {
            object[] attrs = propertyInfo.GetCustomAttributes(true);
            foreach (object attr in attrs)
            {
                var attribute = attr as PictureSheetAttribute;
                if (attribute != null)
                    return true;
            }
            return false;
        }
    }
}

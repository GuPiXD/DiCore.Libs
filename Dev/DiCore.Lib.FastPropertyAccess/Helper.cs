using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DiCore.Lib.FastPropertyAccess
{
    public static class Helper
    {
        private static readonly Assembly SystemAssembly = typeof(object).Assembly;
        public static bool IsSystemType(this Type type) => type.Assembly == SystemAssembly;        

        public static T GetValue<T>(this Dictionary<string, object> d, string name)
        {
            if (!d.TryGetValue(name, out var value) || value is DBNull || value == null)
                return default(T);

            var t = typeof(T);
            t = Nullable.GetUnderlyingType(t) ?? t;
            return (T) Convert.ChangeType(value, t);
        }

        public static PropertyInfo[] GetBindableProperties(this Type type)
        {
            return type.GetProperties(BindingFlags.Instance |
                                      BindingFlags.Public |
                                      BindingFlags.SetProperty).OrderBy(p => p.MetadataToken).ToArray();
        }
    }
}

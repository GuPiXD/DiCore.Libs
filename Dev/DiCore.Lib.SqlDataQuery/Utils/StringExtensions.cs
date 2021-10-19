using System;

namespace DiCore.Lib.SqlDataQuery.Utils
{
    public static class StringExtensions
    {
        public static string Quotes(this string val, bool duplicate = false)
        {
            if (string.IsNullOrEmpty(val) || (!duplicate && (val[0] == '"' && val[val.Length - 1] == '"')))
                return val;

            return val.WrapUp('"');
        }

        public static string WrapUp(this string val, char ch)
        {
            return String.Concat(ch, val, ch);
        }
        public static string UnWrapUp(this string val, char ch)
        {
            return val.Trim(ch);
        }

        public static string WrapUp(this string val, char left, char right)
        {
            return String.Concat(left, val, right);
        }
        
        public static string WrapUp(this string val, string str)
        {
            return String.Concat(str, val, str);
        }

        public static string JointLeft(this string val, string str)
        {
            return String.Concat(str, val);
        }

        public static string JointRight(this string val, string str)
        {
            return String.Concat(val, str);
        }
    }
}
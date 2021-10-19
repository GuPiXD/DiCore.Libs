using System.Text;

namespace DiCore.Lib.SqlDataQuery.Utils
{
    public static class StringBuilderExtensions
    {
        public static StringBuilder Quotes(this StringBuilder sb, bool duplicate = false)
        {
            if (sb == null || sb.Length == 0 || (!duplicate && (sb[0] == '"' && sb[sb.Length - 1] == '"')))
                return sb;

            if (sb[0] != '"')
                sb.Insert(0, "\"");

            if (sb[sb.Length - 1] != '"')
                sb.Append("\"");

            return sb;
        }

        public static StringBuilder WrapUp(this StringBuilder sb, string str)
        {
            if (sb == null || sb.Length == 0)
                return sb;

            sb.Insert(0, str);
            sb.Append(str);

            return sb;
        }

        public static StringBuilder InBrackets(this StringBuilder sb)
        {
            if (sb == null || sb.Length == 0)
                return sb;

            sb.Insert(0, "\t(\n");
            sb.AppendLine("\t)");

            return sb;
        }
    }
}
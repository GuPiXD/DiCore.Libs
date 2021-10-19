using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiCore.Lib.ODFormGenerator
{
    public static class TypeNameConverter
    {
        public static string Convert(string name)
        {
            switch (name.ToLower())
            {
                case "integer":
                case "smallint":
                    return "целое число";
                case "real":
                case "double precision":
                    return "число с плавающей точкой";
                case "text":
                    return "строка";
                case "boolean":
                case "bool":
                    return "признак";
                case "uuid":
                    return "уникальный идентификатор";
                case "timestamp without time zone":
                    return "дата и время";
                case "date":
                    return "дата";
                default:
                    return name;
            }
        }
    }
}

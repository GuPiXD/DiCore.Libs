using System;
using System.Globalization;

namespace DiCore.Lib.SqlDataQuery.SqlCode
{
    /// <summary>
    /// Тип данных PostgreSql
    /// </summary>
    public enum EnDbDataType
    {
        Array = -2147483648,
        Bigint = 1,
        Boolean = 2,
        Box = 3,
        Bytea = 4,
        Circle = 5,
        Char = 6,
        Date = 7,
        Double = 8,
        Integer = 9,
        Line = 10,
        LSeg = 11,
        Money = 12,
        Numeric = 13,
        Path = 14,
        Point = 15,
        Polygon = 16,
        Real = 17,
        Smallint = 18,
        Text = 19,
        Time = 20,
        Timestamp = 21,
        Varchar = 22,
        Refcursor = 23,
        Inet = 24,
        Bit = 25,
        TimestampTZ = 26,
        Uuid = 27,
        Xml = 28,
        Oidvector = 29,
        Interval = 30,
        TimeTZ = 31,
        Name = 32,
        [Obsolete]
        Abstime = 33,
        MacAddr = 34,
        Json = 35,
        Jsonb = 36,
        Hstore = 37,
        InternalChar = 38,
        Varbit = 39,
        Unknown = 40,
        Oid = 41,
        Xid = 42,
        Cid = 43,
        Cidr = 44,
        TsVector = 45,
        TsQuery = 46,
        Enum = 47,
        Composite = 48,
        Regtype = 49,
        Citext = 51,
        Range = 1073741824,
    }

    public static class EnDbDataTypeExtensions
    {
        public static string ConvertFilterValue(this EnDbDataType columnType, string value)
        {
            if (columnType != EnDbDataType.Timestamp) return value;

            var dateTime = DateTime.ParseExact(value.Substring(0, 24), "ddd MMM d yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            value = $"{dateTime:MM-dd-yyyy HH:mm:ss}";
            return value;
        }

        /// <summary>
        /// Опредение принадлежности типа БД к числовому типу
        /// </summary>
        /// <param name="dataType">Тип данных БД</param>
        /// <returns>Принадлежность</returns>
        public static bool IsNumber(this EnDbDataType dataType)
        {
            if (dataType == EnDbDataType.Bigint ||
                dataType == EnDbDataType.Integer ||
                dataType == EnDbDataType.Double ||
                dataType == EnDbDataType.Money ||
                dataType == EnDbDataType.Numeric ||
                dataType == EnDbDataType.Real ||
                dataType == EnDbDataType.Smallint)
                return true;
            return false;
        }
    }
}
using Newtonsoft.Json.Linq;
using Npgsql;

namespace DiCore.Lib.PgSqlAccess.Helpers
{
    public static class JSonHelper
    {
        private static object ParseValue(this NpgsqlDataReader dataReader, int index)
        {
            if (dataReader.IsDBNull(index))
            {
                return null;
            }
            var colType = dataReader.GetDataTypeName(index);
            var colValue = colType == "json" || colType == "jsonb"
                ? JToken.Parse(dataReader.GetString(index))
                : dataReader.GetValue(index);
            return colValue;
        }

        public static JObject JSonFromDataReader(NpgsqlDataReader dataReader, string aggregateColumnAlias,
            string dataPropertyName = "data")
        {
            JObject result;
            var rowIndex = 0;
            var jsonRows = new JArray();
            JProperty aggregateColumnJson = null;
            while (dataReader.Read())
            {
                JObject row = new JObject();
                var fieldCount = dataReader.FieldCount;
                for (int i = 0; i < fieldCount; i++)
                {
                    var colName = dataReader.GetName(i);
                    var colValue = dataReader.ParseValue(i);


                    row.Add(new JProperty(colName, colValue));

                    if (aggregateColumnAlias != null && rowIndex == 0 && aggregateColumnAlias == colName)
                        aggregateColumnJson = new JProperty(aggregateColumnAlias, colValue);
                }

                jsonRows.Add(row);
                rowIndex++;
            }

            if (aggregateColumnJson == null)
                result = new JObject(new JProperty(dataPropertyName, jsonRows));
            else
                result = new JObject(new JProperty(dataPropertyName, jsonRows), aggregateColumnJson);
            return result;
        }

        public static JObject JSonFromDataReader(NpgsqlDataReader dataReader)
        {
            dataReader.Read();
            JObject obj = new JObject();
            var fieldCount = dataReader.FieldCount;
            for (int i = 0; i < fieldCount; i++)
            {
                var colName = dataReader.GetName(i);
                var colValue = dataReader.ParseValue(i);
                obj.Add(new JProperty(colName, colValue));
            }

            return obj;
        }

        public static bool JSonFromDataReader(NpgsqlDataReader dataReader, out JObject result)
        {
            result = new JObject();
            var isPresent = dataReader.Read();
            if (!isPresent)
                return false;
            var fieldCount = dataReader.FieldCount;
            for (int i = 0; i < fieldCount; i++)
            {
                var colName = dataReader.GetName(i);
                var colValue = dataReader.ParseValue(i);
                result.Add(new JProperty(colName, colValue));
            }

            return true;
        }
    }
}
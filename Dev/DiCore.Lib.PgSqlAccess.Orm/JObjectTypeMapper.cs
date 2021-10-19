using DiCore.Lib.PgSqlAccess.DataAccessObjects;
using Newtonsoft.Json.Linq;
using Newtonsoft;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;
using NpgsqlTypes;

namespace DiCore.Lib.PgSqlAccess.Orm
{
    public class JObjectTypeMapper : TypeMapper<JObject>
    {
        public override JObject Parse(object value)
        {
            if (value == null || value is DBNull)
                return null;

            return JObject.Parse((string)value);            
        }

        public override void SetValue(NpgsqlParameter parameter, JObject value)
        {
            parameter.Value = value == null ? (object)DBNull.Value : value.ToString();
            parameter.NpgsqlDbType = NpgsqlDbType.Jsonb;
        }
    }
}

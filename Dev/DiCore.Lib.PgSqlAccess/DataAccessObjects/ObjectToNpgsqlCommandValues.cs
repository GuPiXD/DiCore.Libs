using System;
using System.Linq;
using DiCore.Lib.PgSqlAccess.Types.QueryTypes;
using Npgsql;
using NpgsqlTypes;
using Helper = DiCore.Lib.PgSqlAccess.Helpers.Helper;

namespace DiCore.Lib.PgSqlAccess.DataAccessObjects
{
    /// <summary>
    /// Запись значений свойств класса в параметры команды
    /// </summary>
    public class ObjectToNpgsqlCommandValues
    {
        /// <summary>
        /// Запись значений свойств экземпляра класса типа T в параметры команды
        /// </summary>
        /// <param name="command">Команда</param>
        /// <param name="instance">Объект</param>
        public static void SetParameterValues(NpgsqlCommand command, object instance, bool toLowerCase = false)
        {
            if (instance == null) return;

            var typeObj = instance.GetType();
            var propertyInfos = Helper.TypePropertiesCache(typeObj).OrderBy(p => p.MetadataToken);
            foreach (var propertyInfo in propertyInfos)
            {
                var isNullable = false;
                var propType = propertyInfo.PropertyType;
                if (propType.IsGenericType &&
                    propType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    isNullable = true;
                    propType = propType.GetGenericArguments()[0];
                }


                var name = toLowerCase ? propertyInfo.Name.ToLower() : propertyInfo.Name;
                var param = new NpgsqlParameter() {ParameterName = name, IsNullable = isNullable};
                var paramValue = propertyInfo.GetValue(instance);

                if (propType == typeof(JSon))
                {
                    param.NpgsqlDbType = NpgsqlDbType.Json;
                    param.Value = (object) ((JSon) paramValue).Value ?? DBNull.Value;
                }
                else if (propType == typeof(JSonb))
                {
                    param.NpgsqlDbType = NpgsqlDbType.Jsonb;
                    param.Value = (object) ((JSonb) paramValue).Value ?? DBNull.Value;
                }
                else if (SqlMapper.TypeMappers.TryGetValue(propType, out var mapper))
                {
                    mapper.SetValue(param, paramValue);
                }
                else
                {
                    param.NpgsqlDbType = Helper.GetType(propertyInfo);
                    param.Value = paramValue ?? DBNull.Value;
                }

                command.Parameters.Add(param);
            }
        }
    }
}
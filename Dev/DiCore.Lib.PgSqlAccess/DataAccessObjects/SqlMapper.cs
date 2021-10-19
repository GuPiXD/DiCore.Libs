using System;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;

namespace DiCore.Lib.PgSqlAccess.DataAccessObjects
{
    public static class SqlMapper
    {
        internal static readonly IDictionary<Type, ITypeMapper> TypeMappers = new Dictionary<Type, ITypeMapper>();

        public static void AddTypeMapper<T>(TypeMapper<T> mapper) => AddTypeMapperImpl(typeof(T), mapper);

        private static void AddTypeMapperImpl<T>(Type type, TypeMapper<T> mapper)
        {
            if (TypeMappers.ContainsKey(type))
                throw new ArgumentException($"Type mapper for type {type} already exist", nameof(mapper));

            TypeMappers[type] = mapper;
        }
    }

    public abstract class TypeMapper<T>: ITypeMapper
    {
        /// <summary>
        /// Assign the value of a parameter before a command executes
        /// </summary>
        /// <param name="parameter">The parameter to configure</param>
        /// <param name="value">Parameter value</param>
        public abstract void SetValue(NpgsqlParameter parameter, T value);

        /// <summary>
        /// Parse a database value back to a typed value
        /// </summary>
        /// <param name="value">The value from the database</param>
        /// <returns>The typed value</returns>
        public abstract T Parse(object value);

        void ITypeMapper.SetValue(NpgsqlParameter parameter, object value)
        {
            if (value is DBNull)
            {
                parameter.Value = value;
            }
            else
            {
                SetValue(parameter, (T)value);
            }
        }

        object ITypeMapper.Parse(Type destinationType, object value)
        {
            return Parse(value);
        }
    }

    public interface ITypeMapper
    {
        void SetValue(NpgsqlParameter parameter, object value);
        object Parse(Type destinationType, object value);
    }

    public sealed class JsonTypeMapper<T>: TypeMapper<T>
    {
        public override void SetValue(NpgsqlParameter parameter, T value)
        {
            parameter.Value = value == null
                ? (object)DBNull.Value
                : JsonConvert.SerializeObject(value);

            parameter.NpgsqlDbType = NpgsqlDbType.Jsonb;
        }

        public override T Parse(object value)
        {
            if (value == null || value is DBNull) return default;

            var res = JsonConvert.DeserializeObject<T>(value.ToString());

            return res;
        }
    }
}

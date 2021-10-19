using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using DiCore.Lib.PgSqlAccess.DataAccessObjects;
using DiCore.Lib.PgSqlAccess.Types;
using DiCore.Lib.PgSqlAccess.Types.Attributes;
using Npgsql;
using NpgsqlTypes;

namespace DiCore.Lib.PgSqlAccess.Helpers
{
    public static class Helper
    {
        private static readonly Assembly SystemAssembly = typeof(object).Assembly;

        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> TypeProperties =
            new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();

        internal static bool IsSystemType(this Type type) => type.Assembly == SystemAssembly;

        /// <summary>
        /// Преобразование типа данных Type в тип данных DbType
        /// </summary>
        /// <param name="dateTypeName"></param>
        /// <returns></returns>
        internal static NpgsqlDbType DataTypeConvert(Type dateType)
        {
            return SqlMapper.TypeMappers.ContainsKey(dateType)
                ? NpgsqlDbType.Jsonb
                : DataTypeConvert(dateType.FullName);
        }

        /// <summary>
        /// Преобразование типа данных Type в тип данных DbType
        /// </summary>
        /// <param name="dateTypeName"></param>
        /// <returns></returns>
        internal static NpgsqlDbType DataTypeConvert(string dateTypeName)
        {
            switch (dateTypeName)
            {
                case "System.Int16":
                    return NpgsqlDbType.Smallint;
                case "System.Int32":
                    return NpgsqlDbType.Integer;
                case "System.Int64":
                    return NpgsqlDbType.Bigint;
                case "System.String":
                    return NpgsqlDbType.Text;
                case "System.Boolean":
                    return NpgsqlDbType.Boolean;
                case "System.DateTime":
                    return NpgsqlDbType.Timestamp;
                case "System.Double":
                    return NpgsqlDbType.Double;
                case "System.Guid":
                    return NpgsqlDbType.Uuid;
                case "System.Decimal":
                    return NpgsqlDbType.Numeric;
                case "System.Single":
                    return NpgsqlDbType.Real;
                case "System.TimeSpan":
                    return NpgsqlDbType.Interval;
                case "System.Byte[]":
                    return NpgsqlDbType.Bytea;
                case "System.Char":
                    return NpgsqlDbType.Char;
                case "System.Int16[]":
                    return NpgsqlDbType.Array | NpgsqlDbType.Smallint;
                case "System.Int32[]":
                    return NpgsqlDbType.Array | NpgsqlDbType.Integer;
                case "System.Int64[]":
                    return NpgsqlDbType.Array | NpgsqlDbType.Bigint;
                case "System.String[]":
                    return NpgsqlDbType.Array | NpgsqlDbType.Text;
                case "System.Boolean[]":
                    return NpgsqlDbType.Array | NpgsqlDbType.Boolean;
                case "System.DateTime[]":
                    return NpgsqlDbType.Array | NpgsqlDbType.Timestamp;
                case "System.Double[]":
                    return NpgsqlDbType.Array | NpgsqlDbType.Double;
                case "System.Guid[]":
                    return NpgsqlDbType.Array | NpgsqlDbType.Uuid;
                case "System.Decimal[]":
                    return NpgsqlDbType.Array | NpgsqlDbType.Numeric;
                case "System.Single[]":
                    return NpgsqlDbType.Array | NpgsqlDbType.Real;
                case "System.TimeSpan[]":
                    return NpgsqlDbType.Array | NpgsqlDbType.Interval;
                case "System.Char[]":
                    return NpgsqlDbType.Array | NpgsqlDbType.Char;
            }

            throw new ArgumentException("Не удалось преобразовать тип данных в тип NpgsqlDbType ({0})", dateTypeName);
        }

        /// <summary>
        /// Получение списка описаний полей для заданного типа
        /// </summary>
        /// <param name="t">Тип</param>
        /// <param name="withPublicSetter">Только поля с доступным методом set</param>
        /// <param name="byOrder">Использовать атрибут, задающий порядок свойств</param>
        /// <returns>Список описаний полей</returns>
        internal static List<PropertyDescription> GetPropertyDescriptions(Type t, bool withPublicSetter,
            bool withNestedTypes, bool byOrder = false, string baseName = "")
        {
            var propertyDescriptions = new List<PropertyDescription>();
            var properties = TypePropertiesCache(t).OrderBy(p => p.MetadataToken).ToArray();
            foreach (var property in properties)
            {
                var isPublicSetter = property.CanWrite && property.GetSetMethod( /*nonPublic*/ true).IsPublic;
                int? order = null;
                if (byOrder)
                {
                    var attr = property.GetCustomAttributes(typeof(PropertyOrderAttribute), false)
                        .Cast<PropertyOrderAttribute>().FirstOrDefault();
                    if (attr == null)
                        throw new ArgumentException(
                            $"У свойства \"{property.Name}\" отсутствует атрибут PropertyOrder");

                    order = attr.Order;
                }

                if (withPublicSetter && !isPublicSetter) continue;

                var propertyType = property.PropertyType;
                if (propertyType.IsSystemType() || SqlMapper.TypeMappers.ContainsKey(propertyType))
                {
                    propertyDescriptions.Add(
                        new PropertyDescription
                        {
                            Type = GetType(property),
                            PropertyType = propertyType,
                            Name = String.IsNullOrEmpty(baseName) ? property.Name : $"{baseName}#{property.Name}",
                            Order = order
                        });
                }
                else
                {
                    if (!withNestedTypes) continue;

                    var res = GetPropertyDescriptions(propertyType, withPublicSetter, withNestedTypes,
                        byOrder,
                        String.IsNullOrEmpty(baseName) ? property.Name : $"{baseName}#{property.Name}");
                    propertyDescriptions.AddRange(res);
                }
            }

            return propertyDescriptions;
        }

        internal static IEnumerable<PropertyInfo> TypePropertiesCache(Type type)
        {
            if (TypeProperties.TryGetValue(type.TypeHandle, out IEnumerable<PropertyInfo> pis))
            {
                return pis;
            }

            var properties = type.GetProperties().ToArray();
            TypeProperties[type.TypeHandle] = properties;
            return properties;
        }

        public static NpgsqlDbType GetType(PropertyInfo propertyInfo)
        {
            var typeAttr = propertyInfo.GetCustomAttributes(typeof(NpgsqlDbTypeAttribute), false)
                .Cast<NpgsqlDbTypeAttribute>().FirstOrDefault();
            var type = typeAttr == null ? (NpgsqlDbType?) null : (NpgsqlDbType) typeAttr.Type;

            if (type == null)
            {
                type = DataTypeConvert(Nullable.GetUnderlyingType(propertyInfo.PropertyType) ??
                                       propertyInfo.PropertyType);
            }

            return type.Value;
        }

        internal static List<PropertyDescription> GetProperties<T>(out bool usingOrder, bool withPublicSetter = false,
            bool withNestedTypes = false)
        {
            var usingOrderAttr = typeof(T).GetCustomAttribute(typeof(UsingPropertyOrderAttribute));
            usingOrder = usingOrderAttr != null;
            return GetPropertyDescriptions(typeof(T), withPublicSetter, withNestedTypes, usingOrder);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameterDescriptions"></param>
        /// <param name="toLowerCase"></param>
        internal static void CreateParameters(NpgsqlCommand command, List<PropertyDescription> parameterDescriptions,
            bool toLowerCase = false)
        {
            foreach (var parameter in parameterDescriptions)
            {
                var parameterName = toLowerCase ? parameter.Name.ToLower() : parameter.Name;
                NpgsqlParameter npgParameter = new NpgsqlParameter(parameterName, parameter.Type)
                {
                    ParameterName = parameterName
                };
                command.Parameters.Add(npgParameter);
            }
        }

        internal static void CreatePositionalParameters(NpgsqlCommand command,
            List<PropertyDescription> parameterDescriptions)
        {
            foreach (var parameter in parameterDescriptions)
            {
                var npgParameter = new NpgsqlParameter(null, parameter.Type)
                {
                    ParameterName = null
                };
                command.Parameters.Add(npgParameter);
            }
        }

        internal static string WrapText(string text)
        {
            var length = 170;
            var result = new StringBuilder();
            var currentIndex = 0;

            while (currentIndex < text.Length)
            {
                var ln = text.Length - currentIndex > length ? length : text.Length - currentIndex;
                result.AppendLine(text.Substring(currentIndex, ln));
                currentIndex += length;
            }

            return result.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataReader"></param>
        /// <param name="propertyNames"></param>
        /// <returns></returns>
        public static Tuple<int, int>[] GetDbFieldPropNamesMatching(IDataReader dataReader,
            List<PropertyDescription> propertyNames, out List<string> excludeProps)
        {
            var result = new List<Tuple<int, int>>(dataReader.FieldCount);
            excludeProps = null;

            for (var fIndex = 0; fIndex < dataReader.FieldCount; fIndex++)
            {
                var drFieldName = dataReader.GetName(fIndex);
                if (fIndex < propertyNames.Count && drFieldName == propertyNames[fIndex].Name)
                {
                    result.Add(new Tuple<int, int>(fIndex, fIndex));
                }
                else
                {
                    var propIndex = propertyNames.FindIndex(e => e.Name == drFieldName);
                    if (propIndex >= 0)
                    {
                        result.Add(new Tuple<int, int>(fIndex, propIndex));
                    }
                    else
                    {
                        if (excludeProps == null)
                            excludeProps = new List<string>();

                        if (fIndex < propertyNames.Count)
                            excludeProps.Add(propertyNames[fIndex].Name);
                    }
                }
            }

            return result.ToArray();
        }
    }
}
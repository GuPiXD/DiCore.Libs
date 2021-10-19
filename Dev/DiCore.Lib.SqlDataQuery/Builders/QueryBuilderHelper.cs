using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DiCore.Lib.SqlDataQuery.QueryParameter;
using DiCore.Lib.SqlDataQuery.SqlCode;

namespace DiCore.Lib.SqlDataQuery.Builders
{
    internal static class QueryBuilderHelper
    {
        public static EnDbDataType ToEnDbDataType<T>(this QueryCreatorConfig<T> config, MemberInfo info)
        {
            var type = info.GetMemberType();
            type = Nullable.GetUnderlyingType(type) ?? type;

            //TODO: Добавить кэш 
            if (type == typeof(int) || type == typeof(uint))
            {
                return EnDbDataType.Integer;
            }

            if (type == typeof(long) || type == typeof(ulong))
            {
                return EnDbDataType.Bigint;
            }

            if (type == typeof(short) || type == typeof(ushort) || type == typeof(byte))
            {
                return EnDbDataType.Smallint;
            }

            if (type == typeof(string))
            {
                return EnDbDataType.Text;
            }

            if (type == typeof(Guid))
            {
                return EnDbDataType.Uuid;
            }

            if (type == typeof(float))
            {
                return EnDbDataType.Real;
            }

            if (type == typeof(double))
            {
                return EnDbDataType.Double;
            }

            if (type == typeof(DateTime))
            {
                return EnDbDataType.Date;
            }

            if (type == typeof(TimeSpan))
            {
                return EnDbDataType.Timestamp;
            }

            if (type == typeof(bool))
            {
                return EnDbDataType.Boolean;
            }

            if (config.TypeDescriptions.ContainsKey(type))
            {
                return EnDbDataType.Jsonb;
            }

            throw new NotSupportedException($"Type {type} not supported");
        }

        public static bool IsNullConstant(this Expression exp)
        {
            return exp.NodeType == ExpressionType.Constant && ((ConstantExpression) exp).Value == null;
        }

        public static bool IsNeedBrackets(this BinaryExpression b)
        {
            return b.Left.NodeType == ExpressionType.Add
                   || b.Left.NodeType == ExpressionType.Subtract
                   || b.Right.NodeType == ExpressionType.Add
                   || b.Right.NodeType == ExpressionType.Subtract;
        }

        public static EnFilterOperation ToFilterOperation(this BinaryExpression b)
        {
            EnFilterOperation operation;

            switch (b.NodeType)
            {
                case ExpressionType.Equal:
                    operation = IsNullConstant(b.Right) ? EnFilterOperation.IsNull : EnFilterOperation.Equals;
                    break;

                case ExpressionType.NotEqual:
                    operation = IsNullConstant(b.Right) ? EnFilterOperation.IsNotNull : EnFilterOperation.NotEquals;
                    break;

                case ExpressionType.LessThan:
                    operation = EnFilterOperation.LessThan;
                    break;

                case ExpressionType.LessThanOrEqual:
                    operation = EnFilterOperation.LessThanOrEquals;
                    break;

                case ExpressionType.GreaterThan:
                    operation = EnFilterOperation.Greater;
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    operation = EnFilterOperation.GreaterOrEquals;
                    break;

                default:
                    throw new NotSupportedException($"The binary operator '{b.NodeType}' is not supported");
            }

            return operation;
        }

        public static string ToFilterLogic(this BinaryExpression b)
        {
            string logic;

            switch (b.NodeType)
            {
                case ExpressionType.AndAlso:
                case ExpressionType.And:
                    logic = "AND";
                    break;

                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    logic = "OR";
                    break;

                default:
                    throw new NotSupportedException($"The binary operator '{b.NodeType}' is not supported");
            }

            return logic;
        }

        public static string ToArithmeticOperation(this BinaryExpression b)
        {
            string operation;

            switch (b.NodeType)
            {
                case ExpressionType.Add:
                    operation = "+";
                    break;
                case ExpressionType.Subtract:
                    operation = "-";
                    break;

                case ExpressionType.Multiply:
                    operation = "*";
                    break;

                case ExpressionType.Divide:
                    operation = "/";
                    break;

                default:
                    throw new NotSupportedException($"The binary operator '{b.NodeType}' is not supported");
            }

            return operation;
        }

        public static string GetTableName<T>(this QueryCreatorConfig<T> qcc, Type entityType)
        {
            return qcc.TypeDescriptions.TryGetValue(entityType, out var td) ? td.TableName : entityType.GetTableName();
        }

        public static string GetTableName(this Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            return type.IsGenericType ? type.Name.Substring(0, type.Name.LastIndexOf('`')) : type.Name;
        }

        public static string GetTableName(this MemberExpression m)
        {
            var tableName = String.Empty;
            
            while ((m = m.Expression as MemberExpression) != null)
            {
                tableName = String.IsNullOrEmpty(tableName)
                    ? m.Member.GetMemberType().Name
                    : $"{m.Member.GetMemberType().Name}#{tableName}";
            }

            return tableName;
        }

        public static string GetColumnName(this MemberInfo info)
        {
            return info.Name;
        }

        public static string GetTableName<T>()
        {
            return GetTableName(typeof(T));
        }

        public static string GetTableAlias(string tableName)
        {
            return tableName;
        }

        public static string GetColumnAlias(string columnName)
        {
            return columnName;
        }

        public static string GetJointColumnAlias(string tableName, string columnName)
        {
            return $"{GetTableAlias(tableName)}#{GetColumnAlias(columnName)}";
        }
        
        public static string GetJointIdColumnAlias(string joinTableName)
        {
            return $"{joinTableName}Id";
        }

        public static PropertyInfo[] GetBindableProperties(this Type type)
        {
            return type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty).ToArray();
        }
    }
}
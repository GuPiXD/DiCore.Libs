using System;

namespace DiCore.Lib.SqlDataQuery.QueryParameter
{
    /// <summary>
    /// Оператор фильтрации
    /// </summary>
    public enum EnFilterOperation
    {
        Equals,
        NotEquals,
        Greater,
        GreaterOrEquals,
        LessThan,
        LessThanOrEquals,
        StartsWith,
        EndsWith,
        Contains,
        NotContains,
        Empty,
        NotEmpty,
        IsNull,
        IsNotNull
    }

    public static class EnFilterOperationExtensions
    {
        public static bool IsNoValueFilter(this EnFilterOperation filterOperation)
        {
            return filterOperation == EnFilterOperation.IsNotNull ||
                   filterOperation == EnFilterOperation.IsNull;
        }

        public static bool IsEmptyValueFilter(this EnFilterOperation filterOperation)
        {
            return filterOperation == EnFilterOperation.Empty ||
                   filterOperation == EnFilterOperation.NotEmpty;
        }

        public static string ConvertFilterValue(this EnFilterOperation filterOperation, string value)
        {
            switch (filterOperation)
            {
                case EnFilterOperation.Contains:
                    value = $"%{value}%";
                    break;
                case EnFilterOperation.StartsWith:
                    value = $"{value}%";
                    break;
                case EnFilterOperation.EndsWith:
                    value = $"%{value}";
                    break;                
            }

            return value;
        }

        public static EnFilterOperation FromString(string op)
        {
            switch (op)
            {
                //equal ==
                case "eq":
                case "==":
                case "isequalto":
                case "equals":
                case "equalto":
                case "equal":
                    return EnFilterOperation.Equals;
                //not equal !=
                case "neq":
                case "!=":
                case "isnotequalto":
                case "notequals":
                case "notequalto":
                case "notequal":
                case "ne":
                    return EnFilterOperation.NotEquals;
                // Greater
                case "gt":
                case ">":
                case "isgreaterthan":
                case "greaterthan":
                case "greater":
                    return EnFilterOperation.Greater;
                // Greater or equal
                case "gte":
                case ">=":
                case "isgreaterthanorequalto":
                case "greaterthanequal":
                case "ge":
                    return EnFilterOperation.GreaterOrEquals;
                // Less
                case "lt":
                case "<":
                case "islessthan":
                case "lessthan":
                case "less":
                    return EnFilterOperation.LessThan;
                // Less or equal
                case "lte":
                case "<=":
                case "islessthanorequalto":
                case "lessthanequal":
                case "le":
                    return EnFilterOperation.LessThanOrEquals;
                case "startswith":
                    return EnFilterOperation.StartsWith;

                case "endswith":
                    return EnFilterOperation.EndsWith;
                //string.Contains()
                case "contains":
                    return EnFilterOperation.Contains;
                case "doesnotcontain":
                    return EnFilterOperation.NotContains;
                case "isempty":
                    return EnFilterOperation.Empty;
                case "isnotempty":
                    return EnFilterOperation.NotEmpty;
                case "isnull":
                    return EnFilterOperation.IsNull;
                case "isnotnull":
                    return EnFilterOperation.IsNotNull;
                default:
                    return EnFilterOperation.Contains;
            }
        }

        public static string ToString(this EnFilterOperation op)
        {
            switch (op)
            {
                case EnFilterOperation.Equals:
                    return "eq";
                case EnFilterOperation.NotEquals:
                    return "neq";
                case EnFilterOperation.Greater:
                    return "gt";
                case EnFilterOperation.GreaterOrEquals:
                    return "gte";
                case EnFilterOperation.LessThan:
                    return "lt";
                case EnFilterOperation.LessThanOrEquals:
                    return "lte";
                case EnFilterOperation.StartsWith:
                    return "startswith";
                case EnFilterOperation.EndsWith:
                    return "endswith";
                case EnFilterOperation.Contains:
                    return "contains";
                case EnFilterOperation.NotContains:
                    return "doesnotcontain";
                case EnFilterOperation.Empty:
                    return "isempty";
                case EnFilterOperation.NotEmpty:
                    return "isnotempty";
                case EnFilterOperation.IsNull:
                    return "isnull";
                case EnFilterOperation.IsNotNull:
                    return "isnotnull";
            }
            throw new ArgumentException($"Не удалось преобразовать значение типа EnFilterOperation: {op} в строку");
        }

        public static string ToSql(this EnFilterOperation op)
        {
            switch (op)
            {
                case EnFilterOperation.Equals:
                    return " = ";
                case EnFilterOperation.NotEquals:
                    return " != ";
                case EnFilterOperation.Contains:
                case EnFilterOperation.StartsWith:
                case EnFilterOperation.EndsWith:
                    return " LIKE ";
                case EnFilterOperation.Greater:
                    return " > ";
                case EnFilterOperation.LessThan:
                    return " < ";
                case EnFilterOperation.LessThanOrEquals:
                    return " <= ";
                case EnFilterOperation.GreaterOrEquals:
                    return " >= ";
                case EnFilterOperation.Empty:
                    return " = ";
                case EnFilterOperation.NotEmpty:
                    return " != ";
                case EnFilterOperation.IsNull:
                    return " IS NULL ";
                case EnFilterOperation.IsNotNull:
                    return " IS NOT NULL ";
            }
            throw new ArgumentException($"Не удалось преобразовать значение: {op} типа EnFilterOperation в строку");
        }
    }
}
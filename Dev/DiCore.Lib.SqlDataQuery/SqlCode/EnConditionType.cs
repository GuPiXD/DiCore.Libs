using System;

namespace DiCore.Lib.SqlDataQuery.SqlCode
{
    /// <summary>
    /// Тип условия
    /// </summary>
    public enum EnConditionType
    {
        Or,
        And
    }

    public static class EnConditionTypeExtensions
    {
        public static string ToString(this EnConditionType conditionType)
        {
            switch (conditionType)
            {
                case EnConditionType.And:
                    return "AND";
                case EnConditionType.Or:
                    return "OR";

            }
            throw new ArgumentException($"Не удалось преобразовать значение: {conditionType} в значение типа EnConditionType");
        }
    }
}
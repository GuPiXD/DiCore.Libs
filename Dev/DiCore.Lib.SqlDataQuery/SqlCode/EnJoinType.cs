using System;

namespace DiCore.Lib.SqlDataQuery.SqlCode
{
    /// <summary>
    /// Тип объединения таблиц
    /// </summary>
    public enum EnJoinType
    {
        Inner,
        Left,
        Right,
        Full
    }

    public static class EnJoinTypeExtensions
    {
        /// <summary>
        /// Преобразование типа объединения в код SQL
        /// </summary>
        /// <param name="joinType">Тип объединения</param>
        /// <returns>Код SQL</returns>
        public static string ToSql(this EnJoinType joinType)
        {
            switch (joinType)
            {
                case EnJoinType.Inner:
                    return "JOIN";
                case EnJoinType.Left:
                    return "LEFT JOIN";
                case EnJoinType.Right:
                    return "RIGHT JOIN";
                case EnJoinType.Full:
                    return "FULL JOIN";
            }
            throw new ArgumentException($"Не удалось преобразовать значение: {joinType} в значение типа EnJoinType");
        }
    }
}
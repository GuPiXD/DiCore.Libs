namespace DiCore.Lib.SqlDataQuery.SqlCode
{
    /// <summary>
    /// Сопоставление столбцов для фильтрации
    /// </summary>
    public class FilterColumnMapping
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="tableAlias">Псевдоним таблицы</param>
        /// <param name="columnAlias">Псевдоним столбца</param>
        /// <param name="filterTableAlias">Псевдоним таблицы, из которой используется поле для фильтрации</param>
        /// <param name="filterColumnAlias">Псевдоним столбца используемого для фильтрации</param>
        public FilterColumnMapping(string tableAlias, string columnAlias, string filterTableAlias, string filterColumnAlias)
        {
            TableAlias = tableAlias;
            ColumnAlias = columnAlias;
            FilterTableAlias = filterTableAlias;
            FilterColumnAlias = filterColumnAlias;
        }

        public string TableAlias { get; protected set; }
        public string ColumnAlias { get; protected set; }
        public string FilterTableAlias { get; protected set; }
        public string FilterColumnAlias { get; protected set; }
    }
}

namespace DiCore.Lib.SqlDataQuery.SqlCode
{
    /// <summary>
    /// Объединение таблиц БД
    /// </summary>
    public class Join
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="table1Alias">Псевдоним первой таблицы</param>
        /// <param name="column1Alias">Псевдоним столбца первой таблицы</param>
        /// <param name="table2Alias">Псевдоним второй таблицы</param>
        /// <param name="column2Alias">Псевдоним столбца второй таблицы</param>
        /// <param name="type">Тип объединения (по умолчанию Inner)</param>
        public Join(string table1Alias, string column1Alias, string table2Alias, string column2Alias, EnJoinType type = EnJoinType.Inner)
        {
            Type = type;
            Table1Alias = table1Alias;
            Column1Alias = column1Alias;
            Table2Alias = table2Alias;
            Column2Alias = column2Alias;
        }

        public EnJoinType Type { get; protected set; }
        public string Table1Alias { get; protected set; }
        public string Column1Alias { get; protected set; }
        public string Table2Alias { get; protected set; }
        public string Column2Alias { get; protected set; }
    }
}
namespace DiCore.Lib.SqlDataQuery.SqlCode
{
    /// <summary>
    /// Столбец БД
    /// </summary>
    public class Column : BaseColumn
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="tableAlias">Псевдоним таблицы</param>
        /// <param name="name">Столбца</param>
        /// <param name="alias">Псевдоним</param>
        /// <param name="type">Тип</param>
        /// <param name="selectType">Включение столбца в select части запроса</param>
        public Column(string tableAlias, string name, string alias, EnDbDataType type, EnSelectType selectType)
        {
            Alias = alias;
            TableAlias = tableAlias;
            Name = name;
            Type = type;
            SelectType = selectType;
        }

        public string Name { get; protected set; }
    }
}
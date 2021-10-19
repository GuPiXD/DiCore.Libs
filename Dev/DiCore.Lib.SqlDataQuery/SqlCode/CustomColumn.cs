namespace DiCore.Lib.SqlDataQuery.SqlCode
{
    /// <summary>
    /// Пользовательский столбец БД
    /// </summary>
    public class CustomColumn : BaseColumn
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="alias">Псевдоним столбца</param>
        /// <param name="code">SQL код</param>
        /// <param name="tableAlias">Псевдоним таблицы, которая должна присутствовать в разделе FROM для этого столбца</param>
        public CustomColumn(string alias, string code, string tableAlias = null)
        {
            Alias = alias;
            Code = code;
            TableAlias = tableAlias;
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="alias">Псевдоним столбца</param>
        /// <param name="code">SQL код</param>
        /// <param name="type">Тип столбца</param>
        /// <param name="tableAlias">Псевдоним таблицы, которая должна присутствовать в разделе FROM для этого столбца</param>
        public CustomColumn(string alias, string code, EnDbDataType type, string tableAlias = null)
            : this(alias, code, tableAlias)
        {
            Type = type;
        }

        public string Code { get; }

        public override string ToString()
        {
            return Code;
        }
    }
}
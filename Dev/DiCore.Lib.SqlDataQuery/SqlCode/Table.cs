namespace DiCore.Lib.SqlDataQuery.SqlCode
{
    /// <summary>
    /// Таблица БД
    /// </summary>
    public class Table: BaseTable
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="schema">Имя схемы</param>
        /// <param name="name">Имя</param>
        /// <param name="alias">Псевдоним</param>
        public Table(string schema, string name, string alias)
        : base(alias)
        {
            Schema = schema;
            Name = name;
        }

        public string Name { get; protected set; }
        public string Schema { get; protected set; }
    }
}
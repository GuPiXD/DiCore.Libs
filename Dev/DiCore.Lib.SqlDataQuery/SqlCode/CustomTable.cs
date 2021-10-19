namespace DiCore.Lib.SqlDataQuery.SqlCode
{
    /// <summary>
    /// Таблица БД
    /// </summary>
    public class CustomTable: BaseTable
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="code">SQL-код</param>
        /// <param name="alias">Псевдоним</param>
        public CustomTable(string code, string alias)
        : base(alias)
        {
            Code = code;
        }

        public string Code { get; protected set; }
    }
}
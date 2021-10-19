namespace DiCore.Lib.PgSqlAccess.Types.QueryTypes
{
    /// <summary>
    /// Строковое представление типа JSon. Используется для передачи параметра типа json в запрос или функцию БД.
    /// </summary>
    /// <example>
    /// var queryJSon = new JsonQuery(connection, queryText);
    /// queryJSon.ExecuteReader(new { Id = 4, Name = new JSon("{}") });
    ///</example> 
    public struct JSon
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="value">строковое представление значения json</param>
        public JSon(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Строковое представление значения json
        /// </summary>
        public string Value { get; set; }
    }
}
namespace DiCore.Lib.PgSqlAccess.Types.QueryTypes
{
    /// <summary>
    /// Строковое представление типа jsonb. Используется для передачи параметра типа jsonb в запрос или функцию БД.
    /// </summary>
    /// <example>
    /// var queryJSon = new JsonQuery(connection, queryText);
    /// queryJSon.ExecuteReader(new { Id = 4, Name = new JSonb("{}") });
    ///</example> 
    public struct JSonb
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="value">строковое представление значения jsonb</param>
        public JSonb(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Строковое представление значения json
        /// </summary>
        public string Value { get; set; }
    }
}
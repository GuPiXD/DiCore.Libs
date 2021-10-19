using Npgsql;

namespace DiCore.Lib.PgSqlAccess.DataAccessObjects
{
    public class View
    {
        private readonly NpgsqlConnection connection;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="connection">Соединение</param>
        /// <param name="schemaName">Имя схемы БД</param>
        /// <param name="viewName">Имя функции</param>
        public View(NpgsqlConnection connection, string schemaName, string viewName)
        {
            this.connection = connection;
        }
    }
}

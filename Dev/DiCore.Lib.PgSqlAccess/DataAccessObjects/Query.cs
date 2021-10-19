using System;
using System.Data;
using System.Diagnostics;
using DiCore.Lib.PgSqlAccess.Helpers;
using Npgsql;

namespace DiCore.Lib.PgSqlAccess.DataAccessObjects
{
    /// <summary>
    /// Запрос к БД
    /// </summary>
    [Obsolete("Используйте PgQuery")]
    public class Query: IDisposable
    {
        /// <summary>
        /// Текст запроса
        /// </summary>
        public string QueryText { get; }

        /// <summary>
        /// Команда
        /// </summary>
        protected readonly NpgsqlCommand command;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="connection">Соединение</param>
        /// <param name="queryText">Текст запроса</param>
        /// <param name="npgsqlTransaction">Транзакция</param>
        /// <param name="commandTimeout">Время ожидания выполнения команды</param>
        public Query(NpgsqlConnection connection, string queryText, NpgsqlTransaction npgsqlTransaction, int? commandTimeout = null)
        {
            QueryText = queryText;
            command = new NpgsqlCommand(queryText)
            {
                Connection = connection,
                CommandType = CommandType.Text
            };
            if (commandTimeout != null)
                command.CommandTimeout = commandTimeout.Value;
            command.Transaction = npgsqlTransaction;
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="connection">Соединение</param>
        /// <param name="queryText">Текст запроса</param>
        /// <param name="commandTimeout">Время ожидания выполнения команды</param>
        public Query(NpgsqlConnection connection, string queryText, int? commandTimeout = null)
            :this(connection, queryText, null, commandTimeout)
        {
            if (connection == null)
                throw new ArgumentException("Строка соединения(connection) не может быть равна null");
            if (String.IsNullOrEmpty(queryText))
                throw new ArgumentException("Текст запроса(queryText) не может быть пустым или равным null");
        }

        /// <summary>
        /// Выполнение запроса
        /// </summary>
        /// <returns></returns>
        public void Run()
        {
#if DEBUG
            var stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
            command.ExecuteNonQuery();
#if DEBUG
            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            Debug.WriteLine(ts.TotalMilliseconds, $"Query {Helper.WrapText(QueryText)} Run(milliseconds): ");
#endif
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                command.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

using System;
using System.Diagnostics;
using DiCore.Lib.PgSqlAccess.Helpers;
using Npgsql;

namespace DiCore.Lib.PgSqlAccess.DataAccessObjects
{
    /// <summary>
    /// Запрос к БД (в входными параметрами)
    /// </summary>
    /// <typeparam name="TInputType"></typeparam>
    [Obsolete("Используйте PgQuery")]
    public class QueryInput<TInputType> : Query
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="connection">Соединение</param>
        /// <param name="queryText">Текст запроса</param>
        /// <param name="npgsqlTransaction">Транзакция</param>
        /// <param name="commandTimeout">Таймаут</param>
        public QueryInput(NpgsqlConnection connection, string queryText,
            NpgsqlTransaction npgsqlTransaction, int? commandTimeout = null) :
            base(connection, queryText, npgsqlTransaction, commandTimeout)
        {
#if DEBUG
            var stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
            bool usingOrder;
            var inputProperties = Helper.GetProperties<TInputType>(out usingOrder);
            Helper.CreateParameters(command, inputProperties);
            ClassToNpgsqlCommandValues = new ClassToNpgsqlCommandValues<TInputType>();
            command.Prepare();
#if DEBUG
            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            Debug.WriteLine(ts.TotalMilliseconds, $"Query {Helper.WrapText(QueryText)} Ctor Run(milliseconds): ");
#endif
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="connection">Соединение</param>
        /// <param name="queryText">Текст запроса</param>
        /// <param name="commandTimeout">Время ожидания выполнения команды</param>
        public QueryInput(NpgsqlConnection connection, string queryText, int? commandTimeout = null) : 
            this(connection, queryText, null, commandTimeout)
        {
        }

        private ClassToNpgsqlCommandValues<TInputType> ClassToNpgsqlCommandValues { get; set; }

        /// <summary>
        /// Выполнение запроса
        /// </summary>
        /// <returns></returns>
        public void Run(TInputType input)
        {
#if DEBUG
            var stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
            ClassToNpgsqlCommandValues.SetParameterValues(command, input);
            command.ExecuteNonQuery();
#if DEBUG
            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            Debug.WriteLine(ts.TotalMilliseconds, $"Query {Helper.WrapText(QueryText)} Run(milliseconds): ");
#endif
        }
    }
}

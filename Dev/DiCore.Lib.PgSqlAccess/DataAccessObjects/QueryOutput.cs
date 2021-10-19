using System;
using System.Collections.Generic;
using System.Diagnostics;
using DiCore.Lib.PgSqlAccess.Helpers;
using Npgsql;

namespace DiCore.Lib.PgSqlAccess.DataAccessObjects
{
    /// <summary>
    /// Запрос к БД (с выходными параметрами)
    /// </summary>
    /// <typeparam name="TOutputType"></typeparam>
    [Obsolete("Используйте PgQuery")]
    public class Query<TOutputType> : Query
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="connection">Соединение</param>
        /// <param name="queryText">Текст запроса</param>
        /// <param name="commandTimeout">Время ожидания выполнения команды</param>
        public Query(NpgsqlConnection connection, string queryText, int? commandTimeout = null) :
            base(connection, queryText, commandTimeout)
        {
            command.Prepare();
            RecordToObject = new RecordToObject<TOutputType>();
        }

        private RecordToObject<TOutputType> RecordToObject { get; set; }

        /// <summary>
        /// Выполнение запроса (с результатом в виде одной строки)
        /// </summary>
        /// <returns></returns>
        public TOutputType RunOne()
        {
#if DEBUG
            var stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
            var dataReader = command.ExecuteReader();
            dataReader.Read();
            var record = new object[dataReader.FieldCount];
            dataReader.GetValues(record);
            var result = RecordToObject.GetResult(record);
            dataReader.Close();
#if DEBUG
            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            Debug.WriteLine(ts.TotalMilliseconds, $"Query {Helper.WrapText(QueryText)}(milliseconds): ");
#endif
            return result;
        }

        /// <summary>
        /// Выполнение запроса (с результатом в виде набора строк)
        /// </summary>
        /// <returns></returns>
        public List<TOutputType> RunMany()
        {
#if DEBUG
            var stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
            var result = new List<TOutputType>();
            NpgsqlDataReader dataReader;
            using (command)
            {
                dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    var record = new object[dataReader.FieldCount];
                    dataReader.GetValues(record);
                    result.Add(RecordToObject.GetResult(record));
                }
            }
            dataReader.Close();
#if DEBUG
            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            Debug.WriteLine(ts.TotalMilliseconds, $"Query {Helper.WrapText(QueryText)}(milliseconds): ");
#endif
            return result;
        }
    }
}

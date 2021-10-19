using System;
using System.Collections.Generic;
using System.Diagnostics;
using DiCore.Lib.PgSqlAccess.Helpers;
using Npgsql;

namespace DiCore.Lib.PgSqlAccess.DataAccessObjects
{
    /// <summary>
    /// Запрос к БД (с входными и выходными параметрами)
    /// </summary>
    /// <typeparam name="TInputType"></typeparam>
    /// <typeparam name="TOutputType"></typeparam>
    [Obsolete("Используйте PgQuery")]
    public class Query<TInputType, TOutputType> : Query
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="connection">Соединение</param>
        /// <param name="queryText">Текст запроса</param>
        /// <param name="commandTimeout">Время ожидания выполнения команды</param>
        public Query(NpgsqlConnection connection, string queryText, int? commandTimeout = null) : 
            this(connection, queryText, null, commandTimeout)
        {
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="connection">Соединение</param>
        /// <param name="queryText">Текст запроса</param>
        /// <param name="npgsqlTransaction">Транзакция</param>
        /// <param name="commandTimeout">Таймаут</param>
        public Query(NpgsqlConnection connection, string queryText, 
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
            RecordToObject = new RecordToObject<TOutputType>();
            command.Prepare();
#if DEBUG
            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            Debug.WriteLine(ts.TotalMilliseconds, $"Query {Helper.WrapText(QueryText)} Ctor RunMany(milliseconds): ");
#endif
        }

        private ClassToNpgsqlCommandValues<TInputType> ClassToNpgsqlCommandValues { get; set; }

        private RecordToObject<TOutputType> RecordToObject { get; set; }

        /// <summary>
        /// Выполнение запроса (с результатом в виде одной строки)
        /// </summary>
        /// <returns></returns>
        public bool RunOne(TInputType input, out TOutputType result)
        {
#if DEBUG
            var stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
            var outputType = Nullable.GetUnderlyingType(typeof(TOutputType));
            if (outputType == null)
                outputType = typeof(TOutputType);

            ClassToNpgsqlCommandValues.SetParameterValues(command, input);
            NpgsqlDataReader dataReader = command.ExecuteReader();
            var isPresent = dataReader.Read();
            if (!isPresent)
            {
                result = RecordToObject.GetResult(null);
                return false;
            }

            if (outputType.IsPrimitive || outputType == typeof (Guid) ||
                outputType == typeof (string) || outputType == typeof (DateTime))
            {
                var res = dataReader.GetValue(0);
                if (res == DBNull.Value)
                    result = RecordToObject.GetResult(null);
                else
                    result = (TOutputType)res;
            }
            else
            {
                var record = new object[dataReader.FieldCount];
                dataReader.GetValues(record);
                result = RecordToObject.GetResult(record);
            }

            dataReader.Close();
#if DEBUG
            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            Debug.WriteLine(ts.TotalMilliseconds, $"Query {Helper.WrapText(QueryText)} RunOne(milliseconds): ");
#endif
            return true;
        }

        /// <summary>
        /// Выполнение запроса (с результатом в виде одной строки)
        /// </summary>
        /// <returns></returns>
        public List<TOutputType> RunMany(TInputType input)
        {
#if DEBUG
            var stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
            var result = new List<TOutputType>();
            ClassToNpgsqlCommandValues.SetParameterValues(command, input);
            NpgsqlDataReader dataReader = command.ExecuteReader();
            if (typeof (TOutputType).IsPrimitive || typeof (TOutputType) == typeof (Guid) ||
                typeof(TOutputType) == typeof(string) || typeof(TOutputType) == typeof(DateTime) ||
                typeof(TOutputType) == typeof(Guid?) || typeof(TOutputType) == typeof(DateTime?))
            {
                while (dataReader.Read())
                    result.Add((TOutputType)dataReader.GetValue(0));
            }
            else
            {
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
            Debug.WriteLine(ts.TotalMilliseconds, $"Query {Helper.WrapText(QueryText)} RunMany(milliseconds): ");
#endif
            return result;
        }
    }
}

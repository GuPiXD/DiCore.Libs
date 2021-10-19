using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DiCore.Lib.PgSqlAccess.Helpers;
using Npgsql;

namespace DiCore.Lib.PgSqlAccess.DataAccessObjects
{
    /// <summary>
    /// Функция БД (с входными и выходными параметрами)
    /// </summary>
    /// <typeparam name="TInputType"></typeparam>
    /// <typeparam name="TOutputType"></typeparam>
    public class StoredProcedure<TInputType, TOutputType> : StoredProcedure
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="connection">Соединение</param>
        /// <param name="schemaName">Имя схемы БД</param>
        /// <param name="procedureName">Имя функции</param>
        /// <param name="commandTimeout">Время ожидания выполнения команды</param>
        [Obsolete("Используйте PgStoredProcedure")]
        public StoredProcedure(NpgsqlConnection connection, string schemaName, string procedureName,
            int? commandTimeout = null) :
            this(connection, schemaName, procedureName, null, commandTimeout)
        {
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="connection">Соединение</param>
        /// <param name="schemaName">Имя схемы БД</param>
        /// <param name="procedureName">Имя функции</param>
        /// <param name="npgsqlTransaction"></param>
        /// <param name="commandTimeout"></param>
        public StoredProcedure(NpgsqlConnection connection, string schemaName, string procedureName,
            NpgsqlTransaction npgsqlTransaction, int? commandTimeout = null) :
            base(connection, schemaName, procedureName, npgsqlTransaction, commandTimeout)
        {
#if DEBUG
            var stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
            bool usingOrder;
            var inputProperties = Helper.GetProperties<TInputType>(out usingOrder);
            //Helper.CreateParameters(command, inputProperties, true);
            Helper.CreatePositionalParameters(command, inputProperties);
            ClassToNpgsqlCommandValues = new ClassToNpgsqlCommandValues<TInputType>(true);
            RecordToObject = new RecordToObject<TOutputType>();
            command.Prepare();
#if DEBUG
            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            Debug.WriteLine(ts.TotalMilliseconds, $"StoredProcedure {Name} Ctor Run(milliseconds): ");
#endif
        }

        private ClassToNpgsqlCommandValues<TInputType> ClassToNpgsqlCommandValues { get; set; }

        private RecordToObject<TOutputType> RecordToObject { get; set; }

        /// <summary>
        /// Запуск функции (с результатом в виде одной строки)
        /// </summary>
        /// <returns></returns>
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

            //ClassToNpgsqlCommandValues.SetParameterValues(command, input);
            ClassToNpgsqlCommandValues.SetPositionalParameterValues(command, input);
            NpgsqlDataReader dataReader = command.ExecuteReader();
            var isPresent = dataReader.Read();
            if (!isPresent)
            {
                result = RecordToObject.GetResult(null);
                return false;
            }

            if (outputType.IsPrimitive || outputType == typeof(Guid) ||
                outputType == typeof(string) || outputType == typeof(DateTime))
            {
                var res = dataReader.GetValue(0);
                if (res == DBNull.Value)
                    result = RecordToObject.GetResult(null);
                else
                    result = (TOutputType) res;
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
            Debug.WriteLine(ts.TotalMilliseconds, $"StoredProcedure {Name} RunOne(milliseconds): ");
#endif
            return true;
        }

        /// <summary>
        /// Запуск функции (с результатом в виде одной строки)
        /// </summary>
        /// <returns></returns>
        public async Task<TOutputType> RunOneAsync(TInputType input)
        {
#if DEBUG
            var stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
            var outputType = Nullable.GetUnderlyingType(typeof(TOutputType)) ?? typeof(TOutputType);

            //ClassToNpgsqlCommandValues.SetParameterValues(command, input);
            ClassToNpgsqlCommandValues.SetPositionalParameterValues(command, input);
            var result = default(TOutputType);

            using (var dataReader = await command.ExecuteReaderAsync().ConfigureAwait(false))
            {
                var isPresent = await dataReader.ReadAsync().ConfigureAwait(false);

                if (!isPresent)
                {
                    return result;
                }

                if (outputType.IsPrimitive || outputType == typeof(Guid) ||
                    outputType == typeof(string) || outputType == typeof(DateTime))
                {
                    var res = dataReader.GetValue(0);
                    if (res == DBNull.Value)
                        result = RecordToObject.GetResult(null);
                    else
                        result = (TOutputType) res;
                }
                else
                {
                    var record = new object[dataReader.FieldCount];
                    dataReader.GetValues(record);
                    result = RecordToObject.GetResult(record);
                }

                dataReader.Close();
            }
#if DEBUG
            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            Debug.WriteLine(ts.TotalMilliseconds, $"StoredProcedure {Name} RunOne(milliseconds): ");
#endif
            return result;
        }

        public List<TOutputType> RunMany(TInputType input)
        {
#if DEBUG
            var stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
            var result = RunManyAsync(input).Result.ToList();
#if DEBUG
            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            Debug.WriteLine(ts.TotalMilliseconds, $"StoredProcedure {Name} RunMany(milliseconds): ");
#endif
            return result;
        }

        public async Task<IEnumerable<TOutputType>> RunManyAsync(TInputType input)
        {
#if DEBUG
            var stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
            //ClassToNpgsqlCommandValues.SetParameterValues(command, input);
            ClassToNpgsqlCommandValues.SetPositionalParameterValues(command, input);
            var dataReader = await command.ExecuteReaderAsync().ConfigureAwait(false);

#if DEBUG
            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            Debug.WriteLine(ts.TotalMilliseconds,
                $"StoredProcedure {Name} RunManyAsync(milliseconds) execute reader: ");
#endif

            if (typeof(TOutputType).IsPrimitive || typeof(TOutputType) == typeof(Guid) ||
                typeof(TOutputType) == typeof(string) || typeof(TOutputType) == typeof(DateTime) ||
                typeof(TOutputType) == typeof(Guid?) || typeof(TOutputType) == typeof(DateTime?))
            {
                return dataReader.ExecuteReaderSync(dr => (TOutputType) dr.GetValue(0));
            }

            return dataReader.ExecuteReaderSync(dr => RecordToObject.DataReaderGetResult(dr));
        }
    }
}
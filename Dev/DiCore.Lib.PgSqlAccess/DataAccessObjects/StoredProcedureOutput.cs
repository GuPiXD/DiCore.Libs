using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using DiCore.Lib.PgSqlAccess.Helpers;
using Npgsql;

namespace DiCore.Lib.PgSqlAccess.DataAccessObjects
{
    /// <summary>
    /// Функция БД (с выходными параметрами)
    /// </summary>
    /// <typeparam name="TOutputType"></typeparam>
    [Obsolete("Используйте PgStoredProcedure")]
    public class StoredProcedure<TOutputType> : StoredProcedure
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="connection">Соединение</param>
        /// <param name="schemaName">Имя схемы БД</param>
        /// <param name="procedureName">Имя функции</param>
        /// <param name="commandTimeout">Время ожидания выполнения команды</param>
        public StoredProcedure(NpgsqlConnection connection, string schemaName, string procedureName, int? commandTimeout = null) : 
            base(connection, schemaName, procedureName, commandTimeout)
        {
            command.Prepare();
            RecordToObject = new RecordToObject<TOutputType>();
        }

        private RecordToObject<TOutputType> RecordToObject { get; }

        /// <summary>
        /// Запуск функции (с результатом в виде одной строки)
        /// </summary>
        /// <returns></returns>
        public TOutputType RunOne()
        {
            return RunOneAsync().Result;
        }

        public async Task<TOutputType> RunOneAsync()
        {
#if DEBUG
            var stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
            TOutputType result;
            using (var dataReader = await command.ExecuteReaderAsync().ConfigureAwait(false))
            {
                var r = await dataReader.ReadAsync().ConfigureAwait(false);
                if (r == false) return default;

                var record = new object[dataReader.FieldCount];
                dataReader.GetValues(record);
                result = RecordToObject.GetResult(record);
                dataReader.Close();
            }
#if DEBUG
            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            Debug.WriteLine(ts.TotalMilliseconds, $"StoredProcedure {Name} RunOne(milliseconds): ");
#endif
            return result;
        }

        /// <summary>
        /// Запуск функции (с результатом в виде набора строк)
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
            Debug.WriteLine(ts.TotalMilliseconds, $"StoredProcedure {Name} RunMany(milliseconds): ");
#endif
            return result;
        }

        /// <summary>
        /// Запуск функции (с результатом в виде набора строк)
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<TOutputType>> RunManyAsync()
        {
#if DEBUG
            var stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
            var dataReader = await command.ExecuteReaderAsync().ConfigureAwait(false);

#if DEBUG
            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            Debug.WriteLine(ts.TotalMilliseconds, $"StoredProcedure {Name} RunManyAsync(milliseconds) execute reader: ");
#endif
            return dataReader.ExecuteReaderSync(dr => RecordToObject.DataReaderGetResult(dr));
        }
    }
}

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DiCore.Lib.PgSqlAccess.Helpers;
using Npgsql;

namespace DiCore.Lib.PgSqlAccess.DataAccessObjects
{
    /// <summary>
    /// Функция БД (в входными параметрами)
    /// </summary>
    /// <typeparam name="TInputType"></typeparam>
    [Obsolete("Используйте PgStoredProcedure")]
    public class StoredProcedureInput<TInputType> : StoredProcedure
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="connection">Соединение</param>
        /// <param name="schemaName">Имя схемы БД</param>
        /// <param name="procedureName">Имя функции</param>
        /// <param name="npgsqlTransaction">Транзакция</param>
        /// <param name="commandTimeout"></param>
        public StoredProcedureInput(NpgsqlConnection connection, string schemaName, string procedureName,
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
            command.Prepare();
#if DEBUG
            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            Debug.WriteLine(ts.TotalMilliseconds, $"StoredProcedure {Name} Ctor Run(milliseconds): ");
#endif
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="connection">Соединение</param>
        /// <param name="schemaName">Имя схемы БД</param>
        /// <param name="procedureName">Имя функции</param>
        /// <param name="commandTimeout">Время ожидания выполнения команды</param>
        public StoredProcedureInput(NpgsqlConnection connection, string schemaName, string procedureName,
            int? commandTimeout = null) :
            this(connection, schemaName, procedureName, null, commandTimeout)
        {
        }

        private ClassToNpgsqlCommandValues<TInputType> ClassToNpgsqlCommandValues { get; set; }

        /// <summary>
        /// Запуск функции
        /// </summary>
        /// <returns></returns>
        public void Run(TInputType input)
        {
            var t = RunAsync(input);
            t.Wait();
        }

        /// <summary>
        /// Запуск функции
        /// </summary>
        /// <returns></returns>
        public async Task RunAsync(TInputType input)
        {
#if DEBUG
            var stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
            //ClassToNpgsqlCommandValues.SetParameterValues(command, input);
            ClassToNpgsqlCommandValues.SetPositionalParameterValues(command, input);
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
#if DEBUG
            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            Debug.WriteLine(ts.TotalMilliseconds, $"StoredProcedure {Name} RunAsync(milliseconds): ");
#endif
        }
    }
}
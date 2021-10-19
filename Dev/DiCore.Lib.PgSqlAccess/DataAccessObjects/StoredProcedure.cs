using System;
using System.Data;
using System.Diagnostics;
using Npgsql;

namespace DiCore.Lib.PgSqlAccess.DataAccessObjects
{
    /// <summary>
    /// Функция БД
    /// </summary>
    [Obsolete("Используйте PgStoredProcedure")]
    public class StoredProcedure: IDisposable
    {
        /// <summary>
        /// Имя функции
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Команда
        /// </summary>
        protected readonly NpgsqlCommand command;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="connection">Соединение</param>
        /// <param name="schemaName">Имя схемы БД</param>
        /// <param name="procedureName">Имя функции</param>
        /// <param name="npgsqlTransaction">Транзакция</param>
        /// <param name="commandTimeout">Время ожидания выполнения команды</param>
        public StoredProcedure(NpgsqlConnection connection, string schemaName, string procedureName, 
            NpgsqlTransaction npgsqlTransaction, int? commandTimeout = null)
        {
            Name = $"\"{schemaName}\".\"{procedureName}\"";
            
            command = new NpgsqlCommand(Name)
            {
                Connection = connection,
                CommandType = CommandType.StoredProcedure,
                
            };
            if (commandTimeout != null)
                command.CommandTimeout = commandTimeout.Value;
            command.Transaction = npgsqlTransaction;
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="connection">Соединение</param>
        /// <param name="schemaName">Имя схемы БД</param>
        /// <param name="procedureName">Имя функции</param>
        /// <param name="commandTimeout">Время ожидания выполнения команды</param>
        public StoredProcedure(NpgsqlConnection connection, string schemaName, string procedureName, 
            int? commandTimeout = null)
            :this(connection, schemaName, procedureName, null, commandTimeout)
        {
            if (connection == null)
                throw new ArgumentException("Строка соединения(connection) не может быть равна null");
            if (String.IsNullOrEmpty(schemaName))
                throw new ArgumentException("Имя схемы(schemaName) не может быть пустым или равным null");
            if (String.IsNullOrEmpty(procedureName))
                throw new ArgumentException("Имя функции(procedureName) не может быть пустым или равным null");
        }

        /// <summary>
        /// Запуск функции
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
            Debug.WriteLine(ts.TotalMilliseconds, $"StoredProcedure {Name} Run(milliseconds): ");
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

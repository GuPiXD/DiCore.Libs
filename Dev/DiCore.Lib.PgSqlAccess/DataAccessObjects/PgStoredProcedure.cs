using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DiCore.Lib.PgSqlAccess.Helpers;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace DiCore.Lib.PgSqlAccess.DataAccessObjects
{
    /// <summary>
    /// Функция БД (поддерживает анонимный входной объект и выходной результат в формате JObject(json))
    /// </summary>
    public class PgStoredProcedure : StoredProcedure
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="connection">Соединение</param>
        /// <param name="schemaName">Имя схемы БД</param>
        /// <param name="procedureName">Имя функции</param>
        /// <param name="commandTimeout">Время ожидания выполнения команды</param>
        public PgStoredProcedure(NpgsqlConnection connection, string schemaName, string procedureName,
            int commandTimeout = 120) :
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
        public PgStoredProcedure(NpgsqlConnection connection, string schemaName, string procedureName,
            NpgsqlTransaction npgsqlTransaction, int commandTimeout = 120) :
            base(connection, schemaName, procedureName, npgsqlTransaction, commandTimeout)
        {
        }

        /// <summary>
        /// Выполнение функции БД
        /// (на выходе массив строк в json, даже если строка одна)
        /// </summary>
        /// <param name="input">Класс с параметрами, передаваемыми в функцию
        /// (поддерживаются анонимные объекты, например new {Id=6, Name="Test"})</param>
        /// <param name="aggregateColumnAlias">Псевдоним агрегатного столбца в выходном объекте 
        /// (он будет вынесен отдельно от набора данных, например общее количество записей - totalCount).</param>
        /// <param name="dataPropertyName">Наименование свойства с данными внутри выходного json
        /// (по умолчанию равен "data").</param>
        /// <returns>Json объект типа JObject из пространства имён Newtonsoft.Json.Linq 
        /// (для получения строкового представления объекта можно использовать метод ToString().
        /// Метод ToString() поддерживает различное форматирование</returns>
        /// <example>
        /// Пример простого входного типа: new {Id=6, Name="Test"}
        /// Пример передачи в функцию поля типа json и массива: new { JsonbField = new JSonb("{}"), ArrayField = new[] { 4, 3, 4 } }
        /// (Классы, предназначенные для передачи некоторых типов параметров расположены в пространстве имён 
        /// DiCore.Lib.PgSqlAccess.Types.QueryTypes)
        /// </example>
        public JObject ExecuteReader(object input = null, string aggregateColumnAlias = null,
            string dataPropertyName = "data")
        {
#if DEBUG
            var stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
            ObjectToNpgsqlCommandValues.SetParameterValues(command, input, true);
            JObject result;
            using (var dataReader = command.ExecuteReader())
            {
                result = JSonHelper.JSonFromDataReader(dataReader, aggregateColumnAlias, dataPropertyName);
                dataReader.Close();
            }
#if DEBUG
            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            Debug.WriteLine(ts.TotalMilliseconds, $"PgStoredProcedure {Name} ExecuteReader(milliseconds): ");
#endif
            return result;
        }

        /// <summary>
        /// Выполнение функции БД
        /// </summary>
        /// <param name="input">Класс с параметрами, передаваемыми в функцию
        /// (поддерживаются анонимные объекты, например new {Id=6, Name="Test"})</param>
        /// <returns>Json объект типа JObject из пространства имён Newtonsoft.Json.Linq 
        /// (для получения строкового представления объекта можно использовать метод ToString().
        /// Метод ToString() поддерживает различное форматирование</returns>
        /// <example>
        /// Пример простого входного типа: new {Id=6, Name="Test"}
        /// Пример передачи в функцию поля типа json и массива: new { JsonbField = new JSonb("{}"), ArrayField = new[] { 4, 3, 4 } }
        /// (Классы, предназначенные для передачи некоторых типов параметров расположены в пространстве имён 
        /// DiCore.Lib.PgSqlAccess.Types.QueryTypes)
        /// </example>
        [Obsolete]
        public JObject ExecuteReaderOne(object input = null)
        {
#if DEBUG
            var stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
            ObjectToNpgsqlCommandValues.SetParameterValues(command, input, true);
            JObject result;
            using (var dataReader = command.ExecuteReader())
            {
                result = JSonHelper.JSonFromDataReader(dataReader);
                dataReader.Close();
            }
#if DEBUG
            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            Debug.WriteLine(ts.TotalMilliseconds, $"PgStoredProcedure {Name} ExecuteReaderOne(milliseconds): ");
#endif
            return result;
        }

        /// <summary>
        /// Выполнение функции БД
        /// </summary>
        /// <param name="input">Класс с параметрами, передаваемыми в функцию
        /// (поддерживаются анонимные объекты, например new {Id=6, Name="Test"})</param>
        /// <returns>Json объект типа JObject из пространства имён Newtonsoft.Json.Linq 
        /// (для получения строкового представления объекта можно использовать метод ToString().
        /// Метод ToString() поддерживает различное форматирование</returns>
        /// <example>
        /// Пример простого входного типа: new {Id=6, Name="Test"}
        /// Пример передачи в функцию поля типа json и массива: new { JsonbField = new JSonb("{}"), ArrayField = new[] { 4, 3, 4 } }
        /// (Классы, предназначенные для передачи некоторых типов параметров расположены в пространстве имён 
        /// DiCore.Lib.PgSqlAccess.Types.QueryTypes)
        /// </example>
        public bool ExecuteReaderOne(out JObject result, object input = null)
        {
#if DEBUG
            var stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
            ObjectToNpgsqlCommandValues.SetParameterValues(command, input, true);
            bool present;
            using (var dataReader = command.ExecuteReader())
            {
                present = JSonHelper.JSonFromDataReader(dataReader, out result);
                dataReader.Close();
            }
#if DEBUG
            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            Debug.WriteLine(ts.TotalMilliseconds, $"PgStoredProcedure {Name} ExecuteReaderOne(milliseconds): ");
#endif
            return present;
        }

        /// <summary>
        /// Текущий метод устарел, необходимо использовать перегрузку IEnumerable<TOutput> ExecuteReaderCursor. Для сохранения обратной совместимости возможно применение ExecuteReaderCursor().ToList()
        /// Выполнение функции БД
        /// </summary>
        /// <typeparam name="TOutput">Тип выходного значения</typeparam>
        /// <param name="input">Класс с параметрами, передаваемыми в функцию
        /// (поддерживаются анонимные объекты, например new {Id=6, Name="Test"})</param>
        /// <param name="byNames">Сопоставлять выходные параметры по именам (количество параметров может не совпадать)</param>
        /// <returns></returns>
        /// <example>
        /// Пример простого входного типа: new {Id=6, Name="Test"}
        /// Пример передачи в функцию поля типа json и массива: new { JsonbField = new JSonb("{}"), ArrayField = new[] { 4, 3, 4 } }
        /// (Классы, предназначенные для передачи некоторых типов параметров расположены в пространстве имён 
        /// DiCore.Lib.PgSqlAccess.Types.QueryTypes)
        /// </example>
        [Obsolete]
        public List<TOutput> ExecuteReader<TOutput>(object input = null, bool byNames = false)
        {
            return ExecuteReaderCursor<TOutput>(input, byNames).ToList();
        }

        /// <summary>        
        /// Выполнение функции БД
        /// Метод устарел и будет удален. Необходимо использовать асинхронную перегрузку.
        /// Для обратной совместимости можно использовать синхронный вызов ExecuteReaderCursorAsync().Result
        /// </summary>
        /// <typeparam name="TOutput">Тип выходного значения</typeparam>
        /// <param name="input">Класс с параметрами, передаваемыми в функцию
        /// (поддерживаются анонимные объекты, например new {Id=6, Name="Test"})</param>
        /// <param name="byNames">Сопоставлять выходные параметры по именам (количество параметров может не совпадать)</param>
        /// <returns></returns>
        /// <example>
        /// Пример простого входного типа: new {Id=6, Name="Test"}
        /// Пример передачи в функцию поля типа json и массива: new { JsonbField = new JSonb("{}"), ArrayField = new[] { 4, 3, 4 } }
        /// (Классы, предназначенные для передачи некоторых типов параметров расположены в пространстве имён 
        /// DiCore.Lib.PgSqlAccess.Types.QueryTypes)
        /// </example>
        [Obsolete]
        public IEnumerable<TOutput> ExecuteReaderCursor<TOutput>(object input = null, bool byNames = false)
        {
            return command.ExecuteReaderCursor<TOutput>(input, byNames);
        }

        /// <summary>
        /// Выполнение запроса к БД
        /// </summary>
        /// <typeparam name="TOutput">Тип выходного значения</typeparam>
        /// <param name="input">Класс с параметрами, передаваемыми в функцию 
        /// (поддерживаются анонимные объекты, например new {Id=6, Name="Test"})</param>
        /// <param name="byNames">Сопоставлять выходные параметры по именам (количество параметров может не совпадать)</param>
        /// <example>
        /// Пример простого входного типа: new {Id=6, Name="Test"}
        /// Пример передачи в функцию поля типа json и массива: new { JsonbField = new JSonb("{}"), ArrayField = new[] { 4, 3, 4 } }
        /// (Классы, предназначенные для передачи некоторых типов параметров расположены в пространстве имён 
        /// DiCore.Lib.PgSqlAccess.Types.QueryTypes)
        /// </example>
        /// <returns></returns>
        public async Task<IEnumerable<TOutput>> ExecuteReaderCursorAsync<TOutput>(object input = null,
            bool byNames = false, bool toLowerCase = false)
        {
            return await command.ExecuteReaderCursorAsync<TOutput>(input, byNames, toLowerCase).ConfigureAwait(false);
        }

        /// <summary>
        /// Выполнение функции БД
        /// </summary>
        /// <typeparam name="TOutput">Тип выходного значения</typeparam>
        /// <param name="input">Класс с параметрами, передаваемыми в функцию
        /// (поддерживаются анонимные объекты, например new {Id=6, Name="Test"})</param>
        /// <param name="byNames">Сопоставлять выходные параметры по именам (количество параметров может не совпадать)</param>
        /// <returns></returns>
        /// <example>
        /// Пример простого входного типа: new {Id=6, Name="Test"}
        /// Пример передачи в функцию поля типа json и массива: new { JsonbField = new JSonb("{}"), ArrayField = new[] { 4, 3, 4 } }
        /// (Классы, предназначенные для передачи некоторых типов параметров расположены в пространстве имён 
        /// DiCore.Lib.PgSqlAccess.Types.QueryTypes)
        /// </example>
        public bool ExecuteReaderOne<TOutput>(out TOutput result, object input = null, bool byNames = false)
        {
#if DEBUG
            var stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
            var recordToObject = new RecordToObject<TOutput>();
            ObjectToNpgsqlCommandValues.SetParameterValues(command, input, true);
            using (var dataReader = command.ExecuteReader())
            {
                if (!dataReader.Read())
                {
                    result = default;
#if DEBUG
                    stopWatch.Stop();
                    var ts1 = stopWatch.Elapsed;
                    Debug.WriteLine(ts1.TotalMilliseconds, $"PgQuery {Helper.WrapText(command.CommandText)} ExecuteReaderOne<TOutput>(milliseconds): ");
#endif
                    return false;
                }

                var record = new object[dataReader.FieldCount];
                dataReader.GetValues(record);
                if (!byNames)
                {
                    recordToObject = new RecordToObject<TOutput>();
                    result = recordToObject.GetResult(record);
                }
                else
                {
                    var propertyInfos = Helper.GetProperties<TOutput>(out bool usingOrder, true);
                    var fieldPropNamesMatching =
                        Helper.GetDbFieldPropNamesMatching(dataReader, propertyInfos, out List<string> excludeProps);
                    recordToObject = new RecordToObject<TOutput>(propertyInfos, usingOrder, excludeProps,
                        fieldPropNamesMatching);

                    result = recordToObject.GetResult(record);
                }

                dataReader.Close();
            }
#if DEBUG
            stopWatch.Stop();
            var ts2 = stopWatch.Elapsed;
            Debug.WriteLine(ts2.TotalMilliseconds, $"PgStoredProcedure {Name} ExecuteReaderOne<TOutput>(milliseconds): ");
#endif
            return true;
        }

        /// <summary>
        /// Выполнение функции БД
        /// </summary>
        /// <typeparam name="TOutput">Тип выходного значения</typeparam>
        /// <param name="input">Класс с параметрами, передаваемыми в функцию
        /// (поддерживаются анонимные объекты, например new {Id=6, Name="Test"})</param>
        /// <param name="byNames">Сопоставлять выходные параметры по именам (количество параметров может не совпадать)</param>
        /// <returns></returns>
        /// <example>
        /// Пример простого входного типа: new {Id=6, Name="Test"}
        /// Пример передачи в функцию поля типа json и массива: new { JsonbField = new JSonb("{}"), ArrayField = new[] { 4, 3, 4 } }
        /// (Классы, предназначенные для передачи некоторых типов параметров расположены в пространстве имён 
        /// DiCore.Lib.PgSqlAccess.Types.QueryTypes)
        /// </example>
        public async Task<TOutput> ExecuteReaderOneAsync<TOutput>(object input = null, bool byNames = false)
        {
#if DEBUG
            var stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
            var recordToObject = new RecordToObject<TOutput>();
            ObjectToNpgsqlCommandValues.SetParameterValues(command, input, true);
            TOutput result;

            using (var dataReader = await command.ExecuteReaderAsync().ConfigureAwait(false))
            {
                var r = await dataReader.ReadAsync().ConfigureAwait(false);
                if (r == false)
                {
#if DEBUG
                    stopWatch.Stop();
                    var ts1 = stopWatch.Elapsed;
                    Debug.WriteLine(ts1.TotalMilliseconds, $"PgQuery {Helper.WrapText(command.CommandText)} ExecuteReaderOne<TOutput>(milliseconds): ");
#endif
                    return default;
                }

                var record = new object[dataReader.FieldCount];
                dataReader.GetValues(record);
                if (!byNames)
                {
                    recordToObject = new RecordToObject<TOutput>();
                    result = recordToObject.GetResult(record);
                }
                else
                {
                    var propertyInfos = Helper.GetProperties<TOutput>(out bool usingOrder, true);
                    var fieldPropNamesMatching =
                        Helper.GetDbFieldPropNamesMatching(dataReader, propertyInfos, out List<string> excludeProps);
                    recordToObject = new RecordToObject<TOutput>(propertyInfos, usingOrder, excludeProps,
                        fieldPropNamesMatching);

                    result = recordToObject.GetResult(record);
                }

                dataReader.Close();
            }
#if DEBUG
            stopWatch.Stop();
            var ts2 = stopWatch.Elapsed;
            Debug.WriteLine(ts2.TotalMilliseconds, $"PgStoredProcedure {Name} ExecuteReaderOne<TOutput>(milliseconds): ");
#endif
            return result;
        }

        /// <summary>
        /// Выполнение функции БД
        /// </summary>
        /// <typeparam name="TOutput">Тип выходного значения</typeparam>
        /// <param name="input">Класс с параметрами, передаваемыми в функцию
        /// (поддерживаются анонимные объекты, например new {Id=6, Name="Test"})</param>
        /// <returns></returns>
        /// <example>
        /// Пример простого входного типа: new {Id=6, Name="Test"}
        /// Пример передачи в функцию поля типа json и массива: new { JsonbField = new JSonb("{}"), ArrayField = new[] { 4, 3, 4 } }
        /// (Классы, предназначенные для передачи некоторых типов параметров расположены в пространстве имён 
        /// DiCore.Lib.PgSqlAccess.Types.QueryTypes)
        /// </example>
        [Obsolete]
        public TOutput ExecuteReaderOne<TOutput>(object input = null)
        {
#if DEBUG
            var stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
            var recordToObject = new RecordToObject<TOutput>();
            ObjectToNpgsqlCommandValues.SetParameterValues(command, input, true);
            TOutput result;
            using (var dataReader = command.ExecuteReader())
            {
                dataReader.Read();
                var record = new object[dataReader.FieldCount];
                dataReader.GetValues(record);
                result = recordToObject.GetResult(record);
                dataReader.Close();
            }
#if DEBUG
            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            Debug.WriteLine(ts.TotalMilliseconds, $"PgStoredProcedure {Name} ExecuteReaderOne<TOutput>(milliseconds): ");
#endif
            return result;
        }


        /// <summary>
        /// Выполнение функции БД с одиночным выходным значением (int, string, ....)
        /// </summary>
        /// <typeparam name="TOutput">Тип выходного значения</typeparam>
        /// <param name="input">Класс с параметрами, передаваемыми в функцию (поддерживаются анонимные объекты, 
        /// например new {Id=6, Name="Test"})</param>
        /// <returns>Возвращаемое значение</returns>
        /// <example>
        /// Пример простого входного типа: new {Id=6, Name="Test"}
        /// Пример передачи в функцию поля типа json и массива: new { JsonbField = new JSonb("{}"), ArrayField = new[] { 4, 3, 4 } }
        /// (Классы, предназначенные для передачи некоторых типов параметров расположены в пространстве имён 
        /// DiCore.Lib.PgSqlAccess.Types.QueryTypes)
        /// </example>
        public TOutput ExecuteScalar<TOutput>(object input = null)
        {
            return ExecuteScalarAsync<TOutput>(input).Result;
        }

        /// <summary>
        /// Выполнение функции БД с одиночным выходным значением (int, string, ....)
        /// </summary>
        /// <typeparam name="TOutput">Тип выходного значения</typeparam>
        /// <param name="input">Класс с параметрами, передаваемыми в функцию (поддерживаются анонимные объекты, 
        /// например new {Id=6, Name="Test"})</param>
        /// <returns>Возвращаемое значение</returns>
        /// <example>
        /// Пример простого входного типа: new {Id=6, Name="Test"}
        /// Пример передачи в функцию поля типа json и массива: new { JsonbField = new JSonb("{}"), ArrayField = new[] { 4, 3, 4 } }
        /// (Классы, предназначенные для передачи некоторых типов параметров расположены в пространстве имён 
        /// DiCore.Lib.PgSqlAccess.Types.QueryTypes)
        /// </example>
        public async Task<TOutput> ExecuteScalarAsync<TOutput>(object input = null)
        {
#if DEBUG
            var stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
            ObjectToNpgsqlCommandValues.SetParameterValues(command, input, true);
            var objResult = await command.ExecuteScalarAsync().ConfigureAwait(false);
            var result = (TOutput) objResult;
#if DEBUG
            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            Debug.WriteLine(ts.TotalMilliseconds, $"PgStoredProcedure {Name} ExecuteScalarAsync<TOutput>(milliseconds): ");
#endif
            return result;
        }

        /// <summary>
        /// Выполнение функции БД
        /// </summary>
        /// <returns>Количество обработанных строк</returns>
        public int ExecuteNonQuery(object input = null)
        {
            return ExecuteNonQueryAsync(input).Result;
        }

        /// <summary>
        /// Выполнение функции БД
        /// </summary>
        /// <returns>Количество обработанных строк</returns>
        public async Task<int> ExecuteNonQueryAsync(object input = null)
        {
#if DEBUG
            var stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
            ObjectToNpgsqlCommandValues.SetParameterValues(command, input, true);
            var result = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
#if DEBUG
            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            Debug.WriteLine(ts.TotalMilliseconds, $"PgStoredProcedure ExecuteNonQuery(milliseconds): ");
#endif
            return result;
        }
    }
}
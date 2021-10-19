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
    /// Запрос к БД (поддерживает анонимный входной объект и выходной результат в формате JObject(json))
    /// </summary>
    public class PgQuery: Query
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="connection">Соединение</param>
        /// <param name="queryText">Текст запроса</param>
        /// <param name="commandTimeout">Время ожидания выполнения команды</param>
        public PgQuery(NpgsqlConnection connection, string queryText, int commandTimeout = 120) :
            base(connection, queryText, commandTimeout)
        {
        }

        /// <summary>
        /// Выполнение запроса к БД
        /// (на выходе массив строк в json, даже если строка одна)
        /// </summary>
        /// <param name="input">Класс с параметрами, передаваемыми в запрос (поддерживаются анонимные объекты)</param>
        /// <param name="aggregateColumnAlias">Псевдоним агрегатного столбца в выходном объекте 
        /// (он будет вынесен отдельно от набора данных, например общее количество записей - totalCount).</param>
        /// <param name="dataPropertyName">Наименование свойства с данными внутри выходного json
        /// (по умолчанию равен "data").</param>
        /// <returns>Json объект типа JObject из пространства имён Newtonsoft.Json.Linq 
        /// (для получения строкового представления объекта можно использовать метод ToString().
        /// Метод ToString() поддерживает различное форматирование</returns>
        /// <example>
        /// Пример простого входного типа: new {Id=6, Name="Test"}
        /// Пример передачи в запрос поля типа json и массива: new { JsonbField = new JSonb("{}"), ArrayField = new[] { 4, 3, 4 } }
        /// (Классы, предназначенные для передачи некоторых типов параметров расположены в пространстве имён 
        /// DiCore.Lib.PgSqlAccess.Types.QueryTypes)
        /// </example>
        public JObject ExecuteReader(object input = null, string aggregateColumnAlias = null, string dataPropertyName = "data")
        {
#if DEBUG
            var stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
            ObjectToNpgsqlCommandValues.SetParameterValues(command, input);
            JObject result;
            using (var dataReader = command.ExecuteReader())
            {
                result = JSonHelper.JSonFromDataReader(dataReader, aggregateColumnAlias, dataPropertyName);
                dataReader.Close();
            }
#if DEBUG
            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            Debug.WriteLine(ts.TotalMilliseconds, $"PgQuery {Helper.WrapText(command.CommandText)} ExecuteReader(milliseconds): ");
#endif
            return result;
        }

        /// <summary>
        /// Выполнение запроса к БД
        /// </summary>
        /// <param name="input">Класс с параметрами, передаваемыми в запрос (поддерживаются анонимные объекты)</param>
        /// <returns>Json объект типа JObject из пространства имён Newtonsoft.Json.Linq 
        /// (для получения строкового представления объекта можно использовать метод ToString().
        /// Метод ToString() поддерживает различное форматирование</returns>
        /// <example>
        /// Пример простого входного типа: new {Id=6, Name="Test"}
        /// Пример передачи в запрос поля типа json и массива: new { JsonbField = new JSonb("{}"), ArrayField = new[] { 4, 3, 4 } }
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
            ObjectToNpgsqlCommandValues.SetParameterValues(command, input);
            JObject result;
            using (var dataReader = command.ExecuteReader())
            {
                result = JSonHelper.JSonFromDataReader(dataReader);
                dataReader.Close();
            }
#if DEBUG
            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            Debug.WriteLine(ts.TotalMilliseconds, $"PgQuery {Helper.WrapText(command.CommandText)} ExecuteReaderOne(milliseconds): ");
#endif
            return result;
        }

        /// <summary>
        /// Выполнение запроса к БД
        /// </summary>
        /// <param name="result">Результат запроса в формате Json (объект типа JObject из пространства имён Newtonsoft.Json.Linq. 
        /// Для получения строкового представления объекта можно использовать метод ToString(). 
        /// Метод ToString() поддерживает различное форматирование.)</param>
        /// <param name="input">Класс с параметрами, передаваемыми в запрос (поддерживаются анонимные объекты)</param>
        /// 
        /// <example>
        /// Пример простого входного типа: new {Id=6, Name="Test"}
        /// Пример передачи в запрос поля типа json и массива: new { JsonbField = new JSonb("{}"), ArrayField = new[] { 4, 3, 4 } }
        /// (Классы, предназначенные для передачи некоторых типов параметров расположены в пространстве имён 
        /// DiCore.Lib.PgSqlAccess.Types.QueryTypes)
        /// </example>
        /// <returns> 
        /// Признак того, что запрос вернул результат
        /// </returns> 
        public bool ExecuteReaderOne(out JObject result, object input = null)
        {
#if DEBUG
            var stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
            ObjectToNpgsqlCommandValues.SetParameterValues(command, input);
            bool present;
            using (var dataReader = command.ExecuteReader())
            {
                present = JSonHelper.JSonFromDataReader(dataReader, out result);
                dataReader.Close();
            }
#if DEBUG
            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            Debug.WriteLine(ts.TotalMilliseconds, $"PgQuery {Helper.WrapText(command.CommandText)} ExecuteReaderOne(milliseconds): ");
#endif
            return present;
        }

        /// <summary>
        /// Текущий метод устарел, необходимо использовать перегрузку IEnumerable<TOutput> ExecuteReaderCursor. Для сохранения обратной совместимости возможно применение ExecuteReaderCursor().ToList()
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
        [Obsolete]
        public List<TOutput> ExecuteReader<TOutput>(object input = null, bool byNames = false)
        {
            return ExecuteReaderCursor<TOutput>(input, byNames).ToList();
        }

        /// <summary>
        /// Выполнение запроса к БД.
        /// Метод устарел и будет удален. Необходимо использовать асинхронную перегрузку.
        /// Для обратной совместимости можно использовать синхронный вызов ExecuteReaderCursorAsync().Result
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
        public async Task<IEnumerable<TOutput>> ExecuteReaderCursorAsync<TOutput>(object input = null, bool byNames = false)
        {
            return await command.ExecuteReaderCursorAsync<TOutput>(input, byNames).ConfigureAwait(false);
        }

        /// <summary>
        /// Выполнение запроса к БД. Метод устарел и будет удален в следующих версиях пакета. Необходимо использовать перегрузку  public bool ExecuteReaderOne<TOutput>(out TOutput result, object input = null, bool byNames = false)
        /// </summary>
        /// <typeparam name="TOutput">Тип выходного значения</typeparam>
        /// <param name="input">Класс с параметрами, передаваемыми в функцию
        /// (поддерживаются анонимные объекты, например new {Id=6, Name="Test"})</param>
        /// <returns>Результат выполнения запроса</returns>
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
            ObjectToNpgsqlCommandValues.SetParameterValues(command, input);
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
            Debug.WriteLine(ts.TotalMilliseconds, $"PgQuery {Helper.WrapText(command.CommandText)} ExecuteReaderOne<TOutput>(milliseconds): ");
#endif
            return result;
        }

        /// <summary>
        /// Выполнение запроса к БД
        /// </summary>
        /// <typeparam name="TOutput">Тип выходного значения</typeparam>
        /// <param name="result">Результат выполнения запроса</param>
        /// <param name="input">Класс с параметрами, передаваемыми в функцию
        /// (поддерживаются анонимные объекты, например new {Id=6, Name="Test"})</param>
        /// <param name="byNames">Сопоставлять выходные параметры по именам (количество параметров может не совпадать)</param>
        /// <returns>Признак того, что запрос вернул результат</returns>
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
            ObjectToNpgsqlCommandValues.SetParameterValues(command, input);
            using (var dataReader = command.ExecuteReader())
            {
                if (!dataReader.Read())
                {
                    result = default(TOutput);
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
                    var propertyInfos = Helper.GetProperties<TOutput>(out bool usingOrder, true, true);
                    var fieldPropNamesMatching = Helper.GetDbFieldPropNamesMatching(dataReader, propertyInfos, out List<string> excludeProps);
                    recordToObject = new RecordToObject<TOutput>(propertyInfos, usingOrder, excludeProps, fieldPropNamesMatching);
                    result = recordToObject.GetResult(record);
                }
               
                dataReader.Close();
            }
#if DEBUG
            stopWatch.Stop();
            var ts2 = stopWatch.Elapsed;
            Debug.WriteLine(ts2.TotalMilliseconds, $"PgQuery {Helper.WrapText(command.CommandText)} ExecuteReaderOne<TOutput>(milliseconds): ");
#endif
            return true;
        }


        /// <summary>
        /// Выполнение запроса к БД с одиночным выходным значением (int, string, ....)
        /// </summary>
        /// <typeparam name="TOutput">Тип выходного значения</typeparam>
        /// <param name="input">Класс с параметрами, передаваемыми в запрос (поддерживаются анонимные объекты, 
        /// например new {Id=6, Name="Test"})</param>
        /// <returns>Возвращаемое значение</returns>
        /// <example>
        /// Пример простого входного типа: new {Id=6, Name="Test"}
        /// Пример передачи в запрос поля типа json и массива: new { JsonbField = new JSonb("{}"), ArrayField = new[] { 4, 3, 4 } }
        /// (Классы, предназначенные для передачи некоторых типов параметров расположены в пространстве имён 
        /// DiCore.Lib.PgSqlAccess.Types.QueryTypes)
        /// </example>
        public TOutput ExecuteScalar<TOutput>(object input = null)
        {
#if DEBUG
            var stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
            ObjectToNpgsqlCommandValues.SetParameterValues(command, input);
            var objResult = command.ExecuteScalar();
            var result = (TOutput)objResult;
#if DEBUG
            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            Debug.WriteLine(ts.TotalMilliseconds, $"PgQuery {Helper.WrapText(command.CommandText)} ExecuteScalar<TOutput>(milliseconds): ");
#endif
            return result;
        }

        /// <summary>
        /// Выполнение запроса к БД
        /// </summary>
        /// <returns>Количество обработанных строк</returns>
        public int ExecuteNonQuery(object input = null)
        {
#if DEBUG
            var stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
            ObjectToNpgsqlCommandValues.SetParameterValues(command, input);
            int result =  command.ExecuteNonQuery();
#if DEBUG
            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            Debug.WriteLine(ts.TotalMilliseconds, $"PgQuery ExecuteNonQuery(milliseconds): ");
#endif
            return result;
        }

    }
}

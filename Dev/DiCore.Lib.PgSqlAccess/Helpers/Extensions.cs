using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using DiCore.Lib.PgSqlAccess.DataAccessObjects;
using Npgsql;

namespace DiCore.Lib.PgSqlAccess.Helpers
{
    internal static class Extensions
    {
        public static IEnumerable<T> ExecuteReaderSync<T>(this IDataReader reader, Func<IDataReader, T> func)
        {
#if DEBUG
            var stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
            using (reader)
            {
                while (reader.Read())
                {
                    yield return func(reader);
                }

                reader.Close();
            }

#if DEBUG
            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            Debug.WriteLine(ts.TotalMilliseconds, "ExecuteReaderSync<TOutput>(milliseconds): ");
#endif  
        }

        public static async Task<IEnumerable<TOutput>> ExecuteReaderCursorAsync<TOutput>(this NpgsqlCommand command, object input = null, 
            bool byNames = false, bool toLowerCase = false)
        {
            ObjectToNpgsqlCommandValues.SetParameterValues(command, input, toLowerCase);

            var dataReader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            RecordToObject<TOutput> recordToObject;

            if (byNames)
            {
                var propertyInfos = Helper.GetProperties<TOutput>(out bool usingOrder, true, true);
                var fieldPropNamesMatching =
                    Helper.GetDbFieldPropNamesMatching(dataReader, propertyInfos,
                        out List<string> excludeProps);
                recordToObject = new RecordToObject<TOutput>(propertyInfos, usingOrder, excludeProps,
                    fieldPropNamesMatching);
            }
            else
            {
                recordToObject = new RecordToObject<TOutput>();
            }

            return ExecuteReaderSync(dataReader, dr => recordToObject.DataReaderGetResult(dr));
        }

        public static IEnumerable<TOutput> ExecuteReaderCursor<TOutput>(this NpgsqlCommand command, object input = null, bool byNames = false)
        {
#if DEBUG
            var stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
            ObjectToNpgsqlCommandValues.SetParameterValues(command, input);
            using (var dataReader = command.ExecuteReader())
            {
                RecordToObject<TOutput> recordToObject;
                if (byNames)
                {
                    var propertyInfos = Helper.GetProperties<TOutput>(out bool usingOrder, true, true);
                    var fieldPropNamesMatching = Helper.GetDbFieldPropNamesMatching(dataReader, propertyInfos, out List<string> excludeProps);
                    recordToObject = new RecordToObject<TOutput>(propertyInfos, usingOrder, excludeProps, fieldPropNamesMatching);
                }
                else
                {
                    recordToObject = new RecordToObject<TOutput>();
                }

                while (dataReader.Read())
                {
                    yield return recordToObject.DataReaderGetResult(dataReader);
                }
                dataReader.Close();
            }   
            
#if DEBUG
            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            Debug.WriteLine(ts.TotalMilliseconds, $"PgQuery {Helper.WrapText(command.CommandText)} ExecuteReaderCursor<TOutput>(milliseconds): ");
#endif    
        }
    }
}

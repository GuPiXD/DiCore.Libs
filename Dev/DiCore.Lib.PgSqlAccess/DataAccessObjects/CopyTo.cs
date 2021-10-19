using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DiCore.Lib.FastPropertyAccess;
using DiCore.Lib.PgSqlAccess.Types;
using Npgsql;
using Helper = DiCore.Lib.PgSqlAccess.Helpers.Helper;

namespace DiCore.Lib.PgSqlAccess.DataAccessObjects
{
    public class CopyTo<TInputType>
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="connection">Соединение</param>
        /// <param name="schemaName">Имя схемы БД</param>
        /// <param name="tableName">Имя таблицы для вставки</param>
        public CopyTo(NpgsqlConnection connection, string schemaName, string tableName)
        {
            Connection = connection;
            bool usingOrder;
            InputProperties = Helper.GetProperties<TInputType>(out usingOrder);
            FastPropertyAccess = new FastPropertyAccess<TInputType>(EnPropertyUsingMode.Get);
            ImportCommandText = $"COPY BINARY {schemaName}.\"{tableName}\" FROM STDIN ";
        }

        private string ImportCommandText { get; set; }
        private IFastPropertyAccess<TInputType> FastPropertyAccess { get; set; }
        private NpgsqlConnection Connection { get; set; }
        private List<PropertyDescription> InputProperties; 

        public void Run(IEnumerable<TInputType> inputList)
        {
#if DEBUG
            var stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
            try
            {
                Connection.Open();                
                using (var writer = Connection.BeginBinaryImport(ImportCommandText))
                {
                    foreach (var input in inputList)
                    {
                        //if (InputPropertyNames.Count > 0)
                        writer.StartRow();                        
                        foreach (var inputProperty in InputProperties)
                        {
                            var value = FastPropertyAccess.Get(input, inputProperty.Name);
                            if (value != null)
                                writer.Write(value, inputProperty.Type);
                            else
                                writer.WriteNull();
                        }                      
                    }
                }
            }
            finally
            {
                Connection.Close();
            }
#if DEBUG
            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            Debug.WriteLine(ts.TotalMilliseconds, "StoredProcedure.Run(milliseconds): ");
#endif
        }

    }
}

using System;
using DiCore.Lib.PgSqlAccess.DataAccessObjects;
using Npgsql;
using NpgsqlTypes;

namespace DiCore.Lib.PgSqlAccess.Tests.Old
{
    public static class Shared
    {
        static Shared()
        {
            Connection = new NpgsqlConnection(ConnectionString);

            SqlMapper.AddTypeMapper(new JsonTypeMapper<MainModel.MainJsonbValue>());
        }

        /// <summary>
        /// Имя сервера БД, используемого для выполнение тестов
        /// </summary>
        public const string Server = "sds01-depgsql01";

        /// <summary>
        /// Логин для доступа к БД
        /// </summary>
        public const string Login = "postgres";

        /// <summary>
        /// Пароль для доступа к БД
        /// </summary>
        public const string Password = "123qweQWE";

        /// <summary>
        /// Имя временной БД для выполнения тестов
        /// </summary>
        public const string DatabaseName = "pgsqlaccesstest";

        /// <summary>
        /// Строка соединения 
        /// </summary>
        public static readonly string ConnectionStringPostgres =
            $"Server={Server};Port=5432;Database=postgres;User Id={Login};Password={Password};";

        public static readonly string ConnectionString =
            $"Server={Server};Port=5432;Database={DatabaseName};User Id={Login};Password={Password};";

        public static NpgsqlConnection Connection { get; }

        /// <summary>
        /// Удаление БД с предварительным отключением всех процессов
        /// </summary>
        /// <param name="connection">Соединение</param>
        /// <param name="dbName">Имя БД</param>
        public static void DropDatabase(NpgsqlConnection connection, string dbName)
        {
            var terminateConnectionsCommand = new NpgsqlCommand(
                $@"SELECT pg_terminate_backend(pg_stat_activity.pid) FROM pg_stat_activity 
                        WHERE pg_stat_activity.datname = '{
                    dbName}' AND pid <> pg_backend_pid(); ",
                connection);
            terminateConnectionsCommand.ExecuteScalar();

            var dropDatabaseCommand = new NpgsqlCommand($"DROP DATABASE IF EXISTS {dbName}", connection);
            dropDatabaseCommand.ExecuteScalar();
        }

        /// <summary>
        /// Создание БД
        /// </summary>
        /// <param name="connection">Соединение</param>
        /// <param name="dbName">Имя БД</param>
        public static void CreateDatabase(NpgsqlConnection connection, string dbName)
        {
            using (var command = new NpgsqlCommand($"CREATE DATABASE {dbName}", connection))
                command.ExecuteScalar();
        }

        public static void CreateTableMain(NpgsqlConnection connection)
        {
            using (var command = new NpgsqlCommand(
                "CREATE TABLE public.\"Test1\"(\"Id\" serial NOT NULL, \"TextValue\" text, \"IntegerValue\" integer, " +
                "\"RealValue\" real, \"JsonbValue\" jsonb, \"BooleanValue\" boolean, \"UuidValue\" uuid, " +
                "\"BigIntValue\" bigint, \"TimestampValue\" timestamp without time zone, \"SmallIntValue\" smallint, " +
                "\"ByteaValue\" bytea, CONSTRAINT \"PK_Test1Id\" PRIMARY KEY(\"Id\")) WITH(OIDS = FALSE); " +
                "ALTER TABLE \"Test1\" OWNER TO postgres;",
                connection))
            {
                command.ExecuteScalar();
            }
        }

        public static void CreateTableForeignKeyToMainTable(NpgsqlConnection connection)
        {
            using (var command = new NpgsqlCommand(
                "CREATE TABLE public.\"Test2\" (\"Id\" serial NOT NULL, \"Test1Id\" integer, " +
                "CONSTRAINT \"PK_Table2Id\" PRIMARY KEY(\"Id\"), CONSTRAINT \"FK_Test2Test1Id\" " +
                "FOREIGN KEY(\"Test1Id\") REFERENCES \"Test1\"(\"Id\") MATCH FULL ON UPDATE NO ACTION ON DELETE NO ACTION) " +
                "WITH(OIDS = FALSE); ALTER TABLE \"Test2\" OWNER TO postgres;",
                connection))
            {
                command.ExecuteScalar();
            }
        }

        public static void CreateDbObjects(NpgsqlConnection connection)
        {
            CreateTable(connection, "TableJsonb", "jsonb");
            CreateInsertSp(connection, "TableJsonb", "jsonb");

            CreateTable(connection, "TableInt", "integer");
            CreateInsertSp(connection, "TableInt", "integer");
            CreateInsertSpWithoutReturn(connection, "TableInt", "integer");

            CreateTable(connection, "TableText", "text");
            CreateInsertSp(connection, "TableText", "text");

            CreateTable(connection, "TableBoolean", "boolean");
            CreateInsertSp(connection, "TableBoolean", "boolean");

            CreateTable(connection, "TableUuid", "uuid");
            CreateInsertSp(connection, "TableUuid", "uuid");

            CreateTable(connection, "TableTimestamp", "timestamp without time zone");
            CreateInsertSp(connection, "TableTimestamp", "timestamp without time zone");

            CreateTableWithManyColumns(connection, "TableWithManyColumns");
            FillTableWithManyColumns(connection, "TableWithManyColumns");
            CreateSpTableWithManyColumns(connection, "TableWithManyColumns");
        }


        public static void CreateTable(NpgsqlConnection connection, string tableName, string type)
        {
            using (var command = new NpgsqlCommand(
                $"CREATE TABLE public.\"{tableName}\"(\"Id\" serial NOT NULL, \"Value\" {type}, " +
                $"CONSTRAINT \"PK_{tableName}Id\" PRIMARY KEY(\"Id\")) WITH(OIDS = FALSE); " +
                $"ALTER TABLE \"{tableName}\" OWNER TO postgres;",
                connection))
            {
                command.ExecuteScalar();
            }
        }

        public static void CreateInsertSp(NpgsqlConnection connection, string tableName, string type)
        {
            using (var command = new NpgsqlCommand(
                $"CREATE OR REPLACE FUNCTION public.\"{tableName}Insert\"(OUT \"Id\" integer, IN \"value\" {type}) " +
                $"RETURNS integer AS " +
                $"$BODY$ INSERT INTO public.\"{tableName}\" (\"Value\") VALUES(value) RETURNING \"Id\";  $BODY$ " +
                $"LANGUAGE sql VOLATILE COST 1; " +
                $"ALTER FUNCTION public.\"{tableName}Insert\"({type}) OWNER TO postgres;",
                connection))
            {
                command.ExecuteScalar();
            }
        }

        public static void CreateInsertSpWithoutReturn(NpgsqlConnection connection, string tableName, string type)
        {
            using (var command = new NpgsqlCommand(
                $"CREATE OR REPLACE FUNCTION public.\"{tableName}InsertWithoutRet\"(IN \"value\" {type}) " +
                $"RETURNS integer AS " +
                $"$BODY$ INSERT INTO public.\"{tableName}\" (\"Value\") VALUES(value) RETURNING \"Id\";  $BODY$ " +
                $"LANGUAGE sql VOLATILE COST 1; " +
                $"ALTER FUNCTION public.\"{tableName}InsertWithoutRet\"({type}) OWNER TO postgres;",
                connection))
            {
                command.ExecuteScalar();
            }
        }

        public static void CreateGetByIdSp(NpgsqlConnection connection, string tableName, string type)
        {
            using (var command = new NpgsqlCommand(
                $"CREATE OR REPLACE FUNCTION public.\"Get{tableName}ById\"(OUT \"Value\" {type}, IN \"id\" integer) " +
                $"RETURNS integer AS " +
                $"$BODY$ INSERT INTO public.\"{tableName}\" (\"Value\") VALUES(value) RETURNING \"Id\";  $BODY$ " +
                $"LANGUAGE sql VOLATILE COST 1; " +
                $"ALTER FUNCTION public.\"{tableName}Insert\"({type}) OWNER TO postgres;",
                connection))
            {
                command.ExecuteScalar();
            }
        }

        public static void InsertInitDataToTableMain(NpgsqlConnection connection)
        {
            InsertRowToMainTable(connection, 1, "stringValue1", 1, 0.1f, "{ \"id\": \"1\", \"value\": \"value1\"}", false, 
                Guid.NewGuid(), 1, DateTime.MinValue, 1, new byte[] {0x00, 0x11});
            InsertRowToMainTable(connection, 2, "stringValue2", 2, 0.2f, "{ \"id\": \"2\", \"value\": \"value2\"}", true,
                Guid.NewGuid(), 2, DateTime.MaxValue, 2, new byte[] { 0x20, 0x21 });
        }

        public static void InsertInitDataToTableForeignKey(NpgsqlConnection connection)
        {
            InsertRowToForeignKeyTable(connection, 1, 1);
            InsertRowToForeignKeyTable(connection, 2, 2);
        }


        private static void InsertRowToMainTable(NpgsqlConnection connection, int id, string strVal, int intVal, 
            float realVal, string jsonbVal, bool boolVal, Guid guidVal, long bigIntVal, DateTime dateTimeVal, short smIntVal, byte[] byteaVal)
        {

            using (var command = new NpgsqlCommand(
                "INSERT INTO \"Test1\"(\"Id\", \"TextValue\", \"IntegerValue\", \"RealValue\", \"JsonbValue\", \"BooleanValue\", " +
                "\"UuidValue\", \"BigIntValue\", \"TimestampValue\", \"SmallIntValue\", \"ByteaValue\") VALUES(@idParam, @strParam, " +
                "@intParam, @realParam, @jsonbParam::jsonb, @boolParam, @guidParam, @bigIntParam, @dateTimeParam, @smIntParam, @byteaParam);",
                connection))
            {
                command.Parameters.Add(new NpgsqlParameter("idParam", NpgsqlDbType.Integer) { Value = id });
                command.Parameters.Add(new NpgsqlParameter("strParam", NpgsqlDbType.Text) { Value = strVal });
                command.Parameters.Add(new NpgsqlParameter("intParam", NpgsqlDbType.Integer) { Value = intVal });
                command.Parameters.Add(new NpgsqlParameter("realParam", NpgsqlDbType.Real) { Value = realVal }); 
                command.Parameters.Add(new NpgsqlParameter("jsonbParam", NpgsqlDbType.Jsonb) { Value = jsonbVal });
                command.Parameters.Add(new NpgsqlParameter("boolParam", NpgsqlDbType.Boolean) { Value = boolVal });
                command.Parameters.Add(new NpgsqlParameter("guidParam", NpgsqlDbType.Uuid) { Value = guidVal });
                command.Parameters.Add(new NpgsqlParameter("bigIntParam", NpgsqlDbType.Bigint) { Value = bigIntVal });
                command.Parameters.Add(new NpgsqlParameter("dateTimeParam", NpgsqlDbType.Timestamp) { Value = dateTimeVal });
                command.Parameters.Add(new NpgsqlParameter("smIntParam", NpgsqlDbType.Smallint) { Value = smIntVal });
                command.Parameters.Add(new NpgsqlParameter("byteaParam", NpgsqlDbType.Bytea) { Value = byteaVal });
                command.ExecuteScalar();
            }
        }

        private static void InsertRowToForeignKeyTable(NpgsqlConnection connection, int id, int foreignKeyValue)
        {

            using (var command = new NpgsqlCommand(
                "INSERT INTO \"Test2\"(\"Id\", \"Test1Id\") VALUES(@idParam, @fkParam);",
                connection))
            {
                command.Parameters.Add(new NpgsqlParameter("idParam", NpgsqlDbType.Integer) { Value = id });
                command.Parameters.Add(new NpgsqlParameter("fkParam", NpgsqlDbType.Integer) { Value = foreignKeyValue });
                command.ExecuteScalar();
            }
        }


        public static void CreateStoredProcedures(NpgsqlConnection connection)
        {
            CreateStoredProcedureTest1Insert(connection);
        }

        private static void CreateStoredProcedureTest1Insert(NpgsqlConnection connection)
        {
            using (var command = new NpgsqlCommand(
                "CREATE OR REPLACE FUNCTION public.\"Test1Insert\"(OUT \"Id\" integer, IN \"textValue\" text, " +
                "IN \"integerValue\" integer, IN \"realValue\" real, IN \"jsonbValue\" jsonb, IN \"booleanValue\" boolean, " +
                "IN \"uuidValue\" uuid, IN \"bigIntValue\" bigint, IN \"timestampValue\" timestamp without time zone, " +
                "IN \"smallIntValue\" smallint, IN \"byteaValue\" bytea) " +
                "RETURNS integer AS " +
                "$BODY$ " +
                "INSERT INTO public.\"Test1\" (\"TextValue\", \"IntegerValue\", \"RealValue\", \"JsonbValue\", \"BooleanValue\", " +
                "\"UuidValue\", \"BigIntValue\", \"TimestampValue\", \"SmallIntValue\", \"ByteaValue\")  " +
                "VALUES(\"textValue\", \"integerValue\", \"realValue\", \"jsonbValue\", \"booleanValue\", " +
                "\"uuidValue\", \"bigIntValue\", \"timestampValue\", \"smallIntValue\", \"byteaValue\") " +
                "RETURNING \"Id\";  $BODY$ " +
                "LANGUAGE sql VOLATILE " +
                "COST 1; " +
                "ALTER FUNCTION public.\"Test1Insert\"(text, integer, real, jsonb, boolean, uuid, bigint, " +
                "timestamp without time zone, smallint, bytea) OWNER TO postgres;",
                connection))
            {
                command.ExecuteScalar();
            }
        }

        public static void CreateTableWithManyColumns(NpgsqlConnection connection, string tableName)
        {
            using (var command = new NpgsqlCommand(
                $"CREATE TABLE public.\"{tableName}\"(" +
                $"\"Id\" serial NOT NULL, " +
                $"\"Value1\" integer, " +
                $"\"Value2\" integer, " +
                $"\"Value3\" integer, " +
                $"\"Value4\" integer, " +
                $"CONSTRAINT \"PK_{tableName}Id\" PRIMARY KEY(\"Id\")) WITH(OIDS = FALSE); " +
                $"ALTER TABLE \"{tableName}\" OWNER TO postgres;",
                connection))
            {
                command.ExecuteScalar();
            }
        }

        public static void FillTableWithManyColumns(NpgsqlConnection connection, string tableName)
        {
            using (var command = new NpgsqlCommand(
                $"INSERT INTO \"{tableName}\" " +
                $"(\"Value1\", \"Value2\", \"Value3\", \"Value4\") VALUES " +
                $"(11, 12, 13, 14)", connection))
            {
                command.ExecuteScalar();
            }

            using (var command = new NpgsqlCommand(
                $"INSERT INTO \"{tableName}\" " +
                $"(\"Value1\", \"Value2\", \"Value3\", \"Value4\") VALUES " +
                $"(21, 22, 23, 24)", connection))
            {
                command.ExecuteScalar();
            }

            using (var command = new NpgsqlCommand(
                $"INSERT INTO \"{tableName}\" " +
                $"(\"Value1\", \"Value2\", \"Value3\", \"Value4\") VALUES " +
                $"(31, 32, 33, 34)", connection))
            {
                command.ExecuteScalar();
            }
        }
        
        public static void CreateSpTableWithManyColumns(NpgsqlConnection connection, string tableName)
        {
            using (var command = new NpgsqlCommand(
                $"CREATE OR REPLACE FUNCTION public.\"SelectFullModel\"(" + 
                $"OUT \"Id\" integer, OUT \"Value1\" integer, OUT \"Value2\" integer, OUT \"Value3\" integer, OUT \"Value4\" integer) " +
                $"RETURNS SETOF record AS " +
                $"$BODY$" +
                $"SELECT \"Id\", \"Value1\", \"Value2\", \"Value3\", \"Value4\" FROM public.\"{tableName}\"" +
                $"$BODY$ " +
                $"LANGUAGE sql VOLATILE " +
                $"COST 1 ROWS 3; " +
                $"ALTER FUNCTION public.\"SelectFullModel\"() " +
                $"OWNER TO postgres;" +
                $""
                , connection))
            {
                command.ExecuteScalar();
            }

            
            using (var command = new NpgsqlCommand(
                $"CREATE OR REPLACE FUNCTION public.\"SelectFullModelReorder\"(" + 
                $"OUT \"Id\" integer, OUT \"Value1\" integer, OUT \"Value3\" integer, OUT \"Value4\" integer, OUT \"Value2\" integer) " +
                $"RETURNS SETOF record AS " +
                $"$BODY$" +
                $"SELECT \"Id\", \"Value1\", \"Value3\", \"Value4\", \"Value2\" FROM public.\"{tableName}\"" +
                $"$BODY$ " +
                $"LANGUAGE sql VOLATILE " +
                $"COST 1 ROWS 3; " +
                $"ALTER FUNCTION public.\"SelectFullModelReorder\"() " +
                $"OWNER TO postgres;" +
                $""
                , connection))
            {
                command.ExecuteScalar();
            }
            
            using (var command = new NpgsqlCommand(
                $"CREATE OR REPLACE FUNCTION public.\"SelectDontFullModel\"(" + 
                $"OUT \"Id\" integer, OUT \"Value1\" integer, OUT \"Value3\" integer) " +
                $"RETURNS SETOF record AS " +
                $"$BODY$" +
                $"SELECT \"Id\", \"Value1\", \"Value3\" FROM public.\"{tableName}\"" +
                $"$BODY$ " +
                $"LANGUAGE sql VOLATILE " +
                $"COST 1 ROWS 3; " +
                $"ALTER FUNCTION public.\"SelectDontFullModel\"() " +
                $"OWNER TO postgres;" +
                $""
                , connection))
            {
                command.ExecuteScalar();
            }
            
            using (var command = new NpgsqlCommand(
                $"CREATE OR REPLACE FUNCTION public.\"SelectFullOneRecordModel\"(" + 
                $"OUT \"Id\" integer, OUT \"Value1\" integer, OUT \"Value2\" integer, OUT \"Value3\" integer, OUT \"Value4\" integer) " +
                $"RETURNS record AS " +
                $"$BODY$" +
                $"SELECT \"Id\", \"Value1\", \"Value2\", \"Value3\", \"Value4\" FROM \"{tableName}\" LIMIT 1" +
                $"$BODY$ " +
                $"LANGUAGE sql VOLATILE " +
                $"COST 1; " +
                $"ALTER FUNCTION public.\"SelectFullOneRecordModel\"() " +
                $"OWNER TO postgres;" +
                $""
                , connection))
            {
                command.ExecuteScalar();
            }
        }
    }
}

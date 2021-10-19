using System;
using Npgsql;
using NUnit.Framework;

namespace DiCore.Lib.PgSqlAccess.Tests.Old
{
    [SetUpFixture]
    public class InitTests
    {
        [OneTimeSetUp]
        public void Init()
        {
            using (var connection = new NpgsqlConnection(Shared.ConnectionStringPostgres))
            {
                connection.Open();
                try
                {
                    Shared.DropDatabase(connection, Shared.DatabaseName);
                    Shared.CreateDatabase(connection, Shared.DatabaseName);
                }
                catch (Exception e)
                {
                    Assert.Fail($"Ошибка при создании тестовой БД: {e.Message} {e.Data}");
                }
                finally
                {
                    connection.Close();
                }
            }
            using (var connection = new NpgsqlConnection(Shared.ConnectionString))
            {
                connection.Open();
                try
                {
                    Shared.CreateDbObjects(connection);
                    Shared.CreateTableMain(connection);
                    Shared.InsertInitDataToTableMain(connection);
                    //Shared.CreateTableForeignKeyToMainTable(connection);
                    //Shared.CreateStoredProcedures(connection);
                    //Shared.InsertInitDataToTableForeignKey(connection);
                }
                catch (Exception e)
                {
                    Assert.Fail($"Ошибка при создании объектов тестовой БД: {e.Message} {e.Data}");
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            using (var connection = new NpgsqlConnection(Shared.ConnectionStringPostgres))
            {
                connection.Open();
                try
                {
                    Shared.DropDatabase(connection, Shared.DatabaseName);
                }
                catch (Exception e)
                {
                    Assert.Fail($"Ошибка при удалении тестовой БД: {e.Message} {e.Data}");
                }
                finally
                {
                    connection.Close();
                }
            }
        }
    }
}

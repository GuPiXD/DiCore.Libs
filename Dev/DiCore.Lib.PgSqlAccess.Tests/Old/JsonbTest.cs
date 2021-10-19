using System;
using DiCore.Lib.PgSqlAccess.DataAccessObjects;
using DiCore.Lib.PgSqlAccess.Tests.Old.Input;
using Npgsql;
using NUnit.Framework;

namespace DiCore.Lib.PgSqlAccess.Tests.Old
{
    [TestFixture]
    public class JsonbTest
    {       
        [Test]
        public void PgStoredProcedureJsonbInsertTest()
        {
            var result = -1;
            using (var connection = new NpgsqlConnection(Shared.ConnectionString))
            {
                connection.Open();
                try
                {
                    var sp = new PgStoredProcedure(connection, "public", "TableJsonbInsert");
                    result = sp.ExecuteScalar<int>(new TestJsonbInsertInput() {Value = "{\"a\": 2, \"b\": [\"c\", \"d\"]}"});
                }
                catch (Exception e)
                {
                    Assert.Fail(e.Message);
                }
                finally
                {
                    connection.Close();
                }

                Assert.AreNotEqual(result, -1);
            }
        }        
    }
}

using System;
using DiCore.Lib.PgSqlAccess.DataAccessObjects;
using DiCore.Lib.PgSqlAccess.Test.SharedModel;
using Npgsql;
using NUnit.Framework;

namespace DiCore.Lib.PgSqlAccess.Tests.Old
{
    [TestFixture]
    public class TestQueryOutput
    {
        [Test]
        public void TestGet()
        {
            using (var connection =
                new NpgsqlConnection(Shared.ConnectionString))
            {
                connection.Open();
                try
                {
                    var sp = new Query<IntStringModel>(connection, "SELECT \"Id\", \"TextValue\"  FROM \"Test1\"");
                    var res = sp.RunMany();
                }
                catch (Exception e)
                {
                    Assert.Fail(e.Message);
                }
                finally
                {
                    connection.Close();
                }

                Assert.Pass();
            }
        }
    }
}

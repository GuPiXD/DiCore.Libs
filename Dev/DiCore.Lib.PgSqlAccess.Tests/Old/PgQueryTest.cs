using DiCore.Lib.PgSqlAccess.DataAccessObjects;
using DiCore.Lib.PgSqlAccess.Test.SharedModel;
using Npgsql;
using NUnit.Framework;

namespace DiCore.Lib.PgSqlAccess.Tests.Old
{
    [TestFixture]
    public class PgQueryTest
    {
        #region ExecuteReader

        [Test]
        public void ExecuteReaderGetAllColumnsTest()
        {
            using (var connection = new NpgsqlConnection(Shared.ConnectionString))
            {
                connection.Open();

                var query = new PgQuery(connection, "SELECT \"Id\", \"Value1\", \"Value2\", \"Value3\", \"Value4\" FROM \"TableWithManyColumns\"");
                var res = query.ExecuteReader<FullModel>();
                Assert.IsTrue(res.Count >= 3);
                Assert.AreEqual(res[0].Value1, 11); Assert.AreEqual(res[0].Value2, 12);
                Assert.AreEqual(res[0].Value3, 13); Assert.AreEqual(res[0].Value4, 14);
                
                connection.Close();
            }
        }

        [Test]
        public void ExecuteReaderGetByNamesAllColumnsTest()
        {
            using (var connection = new NpgsqlConnection(Shared.ConnectionString))
            {
                connection.Open();

                var query = new PgQuery(connection, "SELECT \"Id\", \"Value1\", \"Value2\", \"Value3\", \"Value4\" FROM \"TableWithManyColumns\"");
                var res = query.ExecuteReader<FullModel>();
                Assert.IsTrue(res.Count >= 3);
                Assert.AreEqual(res[0].Value1, 11); Assert.AreEqual(res[0].Value2, 12);
                Assert.AreEqual(res[0].Value3, 13); Assert.AreEqual(res[0].Value4, 14);
                
                connection.Close();
            }
        }

        [Test]
        public void ExecuteReaderGetByNamesTooMuchColumnsTest()
        {
            using (var connection = new NpgsqlConnection(Shared.ConnectionString))
            {
                connection.Open();
                
                var query = new PgQuery(connection, "SELECT \"Id\", \"Value1\", \"Value2\", \"Value3\", \"Value4\" FROM \"TableWithManyColumns\"");
                var res = query.ExecuteReader<FullModelTM>(byNames:true);
                Assert.IsTrue(res.Count >= 3);
                Assert.AreEqual(res[0].Value1, 11); Assert.AreEqual(res[0].Value2, 12);
                Assert.AreEqual(res[0].Value3, 13); Assert.AreEqual(res[0].Value4, 14);
                Assert.AreEqual(res[0].Value5, 0); Assert.AreEqual(res[0].Value6, 0);

                connection.Close();
            }
        }

        [Test]
        public void ExecuteReaderGetByNamesTooMuchReorderColumnsTest()
        {
            using (var connection = new NpgsqlConnection(Shared.ConnectionString))
            {
                connection.Open();

                var query = new PgQuery(connection, "SELECT \"Id\", \"Value1\", \"Value3\", \"Value4\", \"Value2\" FROM \"TableWithManyColumns\"");
                var res = query.ExecuteReader<FullModelTM>(byNames:true);
                Assert.IsTrue(res.Count >= 3);
                Assert.AreEqual(res[0].Value1, 11); Assert.AreEqual(res[0].Value2, 12);
                Assert.AreEqual(res[0].Value3, 13); Assert.AreEqual(res[0].Value4, 14);
                Assert.AreEqual(res[0].Value5, 0); Assert.AreEqual(res[0].Value6, 0);
                
                connection.Close();
            }
        }

        [Test]
        public void ExecuteReaderGetByNamesDontAllReorderColumnsTest()
        {
            using (var connection = new NpgsqlConnection(Shared.ConnectionString))
            {
                connection.Open();

                var query = new PgQuery(connection, "SELECT \"Id\", \"Value1\", \"Value3\" FROM \"TableWithManyColumns\"");
                var res = query.ExecuteReader<FullModelLR>(byNames:true);
                Assert.IsTrue(res.Count >= 3);
                Assert.AreEqual(res[0].Value1, 11); Assert.AreEqual(res[0].Value3, 13);
                
                connection.Close();
            }
        }

        #endregion

        #region ExecuteReaderOne
        
        [Test]
        public void ExecuteReaderOneGetAllColumnsTest()
        {
            using (var connection = new NpgsqlConnection(Shared.ConnectionString))
            {
                connection.Open();

                var query = new PgQuery(connection, "SELECT \"Id\", \"Value1\", \"Value2\", \"Value3\", \"Value4\" FROM \"TableWithManyColumns\" LIMIT 1");
                var isRun = query.ExecuteReaderOne<FullModel>(out FullModel res);
                Assert.IsTrue(isRun);
                Assert.AreEqual(res.Value1, 11); Assert.AreEqual(res.Value2, 12);
                Assert.AreEqual(res.Value3, 13); Assert.AreEqual(res.Value4, 14);
                
                connection.Close();
            }
        }

        [Test]
        public void ExecuteReaderOneGetByNamesAllColumnsTest()
        {
            using (var connection = new NpgsqlConnection(Shared.ConnectionString))
            {
                connection.Open();

                var query = new PgQuery(connection, "SELECT \"Id\", \"Value1\", \"Value2\", \"Value3\", \"Value4\" FROM \"TableWithManyColumns\"");
                var isRun = query.ExecuteReaderOne<FullModel>(out FullModel res);
                Assert.IsTrue(isRun);
                Assert.AreEqual(res.Value1, 11); Assert.AreEqual(res.Value2, 12);
                Assert.AreEqual(res.Value3, 13); Assert.AreEqual(res.Value4, 14);
                
                connection.Close();
            }
        }

        [Test]
        public void ExecuteReaderOneGetByNamesTooMuchColumnsTest()
        {
            using (var connection = new NpgsqlConnection(Shared.ConnectionString))
            {
                connection.Open();
                
                var query = new PgQuery(connection, "SELECT \"Id\", \"Value1\", \"Value2\", \"Value3\", \"Value4\" FROM \"TableWithManyColumns\"");
                var isRun = query.ExecuteReaderOne<FullModelTM>(out FullModelTM res, byNames: true);
                Assert.IsTrue(isRun);
                Assert.AreEqual(res.Value1, 11); Assert.AreEqual(res.Value2, 12);
                Assert.AreEqual(res.Value3, 13); Assert.AreEqual(res.Value4, 14);
                Assert.AreEqual(res.Value5, 0); Assert.AreEqual(res.Value6, 0);

                connection.Close();
            }
        }

        [Test]
        public void ExecuteReaderOneGetByNamesTooMuchReorderColumnsTest()
        {
            using (var connection = new NpgsqlConnection(Shared.ConnectionString))
            {
                connection.Open();

                var query = new PgQuery(connection, "SELECT \"Id\", \"Value1\", \"Value3\", \"Value4\", \"Value2\" FROM \"TableWithManyColumns\"");
                var isRun = query.ExecuteReaderOne<FullModelTM>(out FullModelTM res, byNames: true);
                Assert.IsTrue(isRun);
                Assert.AreEqual(res.Value1, 11); Assert.AreEqual(res.Value2, 12);
                Assert.AreEqual(res.Value3, 13); Assert.AreEqual(res.Value4, 14);
                Assert.AreEqual(res.Value5, 0); Assert.AreEqual(res.Value6, 0);
                
                connection.Close();
            }
        }

        [Test]
        public void ExecuteReaderOneGetByNamesDontAllReorderColumnsTest()
        {
            using (var connection = new NpgsqlConnection(Shared.ConnectionString))
            {
                connection.Open();

                var query = new PgQuery(connection, "SELECT \"Id\", \"Value1\", \"Value3\" FROM \"TableWithManyColumns\"");
                var isRun = query.ExecuteReaderOne<FullModelLR>(out FullModelLR res, byNames: true);
                Assert.IsTrue(isRun);
                Assert.AreEqual(res.Value1, 11); Assert.AreEqual(res.Value3, 13);
                
                connection.Close();
            }
        }

        #endregion

        [Test]
        public void ExecuteScalarGetTest()
        {
            using (var connection = new NpgsqlConnection(Shared.ConnectionString))
            {
                connection.Open();

                var query = new PgQuery(connection, "SELECT \"Value1\" FROM \"TableWithManyColumns\" WHERE \"Id\"=1");
                var res = query.ExecuteScalar<int>();
                Assert.AreEqual(res, 11);
                
                connection.Close();
            }
        }

        [Test]
        public void ExecuteNonQueryTest()
        {
            using (var connection = new NpgsqlConnection(Shared.ConnectionString))
            {
                connection.Open();

                var query = new PgQuery(connection, 
                    "INSERT INTO \"TableWithManyColumns\" (\"Id\", \"Value1\", \"Value2\", \"Value3\", \"Value4\") VALUES (4, 41, 42, 43, 44)");
                var res = query.ExecuteNonQuery();
                Assert.AreEqual(res, 1);
                
                connection.Close();
            }
        }



        [Test]
        public void ExecuteReaderOneForEmptyTableTest()
        {
            using (var connection = new NpgsqlConnection(Shared.ConnectionString))
            {
                connection.Open();

                var query = new PgQuery(connection, "SELECT \"Value\" FROM \"TableInt\"");
                var isRun = query.ExecuteReaderOne<IntModel>(out IntModel res, byNames: true);
                Assert.IsFalse(isRun);
                
                connection.Close();
            }
        }

        [Test]
        public void ExecuteReaderJsonObjectTest()
        {
            using (var connection = new NpgsqlConnection(Shared.ConnectionString))
            {
                connection.Open();

                var query = new PgQuery(connection, "SELECT \"Id\", \"JsonbValue\" FROM \"Test1\"");
                var isRun = query.ExecuteReaderOne<MainModel>(out MainModel res, byNames: true);
                Assert.IsTrue(isRun);
                Assert.IsNotNull(res);
                
                connection.Close();
            }
        }

        [Test]
        public void InsertJsonObjectTest()
        {
            using (var connection = new NpgsqlConnection(Shared.ConnectionString))
            {
                connection.Open();

                var query = new PgQuery(connection, "INSERT INTO  \"Test1\" (\"Id\", \"JsonbValue\") VALUES(@id, @jsonbValue)");
                var value = new MainModel()
                {
                    Id = 3, JsonbValue = new MainModel.MainJsonbValue()
                    {
                        Id = 200,
                        Value = "Test value 200"
                    }
                };
                var res = query.ExecuteNonQuery(value);
                Assert.AreEqual(1, res);

                connection.Close();
            }
        }
    }
}

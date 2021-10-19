using System;
using System.Collections.Generic;
using System.Data;
using DiCore.Lib.PgSqlAccess.DataAccessObjects;
using DiCore.Lib.PgSqlAccess.Test.SharedModel;
using Npgsql;
using NUnit.Framework;

namespace DiCore.Lib.PgSqlAccess.Tests.Old
{
    [TestFixture]
    public class PgStoredProcedureTest
    {
        #region ExecuteReader

        [Test]
        public void ExecuteReaderGetAllColumnsTest()
        {
            using (var connection = new NpgsqlConnection(Shared.ConnectionString))
            {
                connection.Open();

                var query = new PgStoredProcedure(connection, "public", "SelectFullModel");
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

                var query = new PgStoredProcedure(connection, "public", "SelectFullModel");
                var res = query.ExecuteReader<FullModel>();
                Assert.IsTrue(res.Count >= 3);
                Assert.AreEqual(res[0].Value1, 11); Assert.AreEqual(res[0].Value2, 12);
                Assert.AreEqual(res[0].Value3, 13); Assert.AreEqual(res[0].Value4, 14);
                
                connection.Close();
            }
        }

        [Test]
        public void ExecuteReaderGetByNamesDontAllColumnsTest()
        {
            using (var connection = new NpgsqlConnection(Shared.ConnectionString))
            {
                connection.Open();

                var query = new PgStoredProcedure(connection, "public", "SelectFullModel");
                var res = query.ExecuteReader<FullModelL>(byNames: true);
                Assert.IsTrue(res.Count >= 3);
                Assert.AreEqual(res[0].Value1, 11); Assert.AreEqual(res[0].Value2, 12);

                connection.Close();
            }
        }

        [Test]
        public void ExecuteReaderGetByNamesTooMuchColumnsTest()
        {
            using (var connection = new NpgsqlConnection(Shared.ConnectionString))
            {
                connection.Open();
                
                var query = new PgStoredProcedure(connection, "public", "SelectFullModel");
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

                var query = new PgStoredProcedure(connection, "public", "SelectFullModelReorder");
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

                var query = new PgStoredProcedure(connection, "public", "SelectDontFullModel");
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

                var query = new PgStoredProcedure(connection, "public", "SelectFullOneRecordModel");
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

                var query = new PgStoredProcedure(connection, "public", "SelectFullModel");
                var isRun = query.ExecuteReaderOne<FullModel>(out FullModel res, byNames: true);
                Assert.IsTrue(isRun);
                Assert.AreEqual(res.Value1, 11); Assert.AreEqual(res.Value2, 12);
                Assert.AreEqual(res.Value3, 13); Assert.AreEqual(res.Value4, 14);
                
                connection.Close();
            }
        }

        [Test]
        public void ExecuteReaderOneGetByNamesDontAllColumnsTest()
        {
            using (var connection = new NpgsqlConnection(Shared.ConnectionString))
            {
                connection.Open();

                var query = new PgStoredProcedure(connection, "public", "SelectFullModel");
                var isRun = query.ExecuteReaderOne<FullModelL>(out FullModelL res, byNames: true);
                Assert.IsTrue(isRun);
                Assert.AreEqual(res.Value1, 11); Assert.AreEqual(res.Value2, 12);

                connection.Close();
            }
        }

        [Test]
        public void ExecuteReaderOneGetByNamesTooMuchColumnsTest()
        {
            using (var connection = new NpgsqlConnection(Shared.ConnectionString))
            {
                connection.Open();
                
                var query = new PgStoredProcedure(connection, "public", "SelectFullModel");
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

                var query = new PgStoredProcedure(connection, "public", "SelectFullModelReorder");
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

                var query = new PgStoredProcedure(connection, "public", "SelectDontFullModel");
                var isRun = query.ExecuteReaderOne<FullModelLR>(out FullModelLR res, byNames: true);
                Assert.IsTrue(isRun);
                Assert.AreEqual(res.Value1, 11); Assert.AreEqual(res.Value3, 13);
                
                connection.Close();
            }
        }

        [Test]
        public void ExecuteScalarNullableUuidInsert()
        {
            using (var connection = new NpgsqlConnection(Shared.ConnectionString))
            {
                connection.Open();
                Guid? val = null;
                var query = new PgStoredProcedure(connection, "public", "TableUuidInsert");
                var d = query.ExecuteScalar<int>(new { Value = val});
                Assert.AreEqual(d, 1);

                connection.Close();
            }
        }
        #endregion
    }
}

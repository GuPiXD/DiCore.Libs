using System;
using System.Collections.Generic;
using System.Text;
using DiCore.Lib.PgSqlAccess.DataAccessObjects;
using DiCore.Lib.TestModels.Models;
using Npgsql;
using NUnit.Framework;

namespace DiCore.Lib.PgSqlAccess.Tests
{
    [TestFixture]
    public class StoredProcedureTest
    {
        private string schema = "59566961-8246-4bb8-8321-2276a26c553a";
        private string connectionString;
        private NpgsqlConnection connection;

        [SetUp]
        public void Init()
        {
            var csb = new NpgsqlConnectionStringBuilder
            {
                Host = "vds01-tetemp-31",
                Database = "ModelTest",
                Username = "postgres",
                Password = "123qweQWE"
            };

            connectionString = csb.ConnectionString;
            connection = new NpgsqlConnection(connectionString);
            connection.Open();
        }

        [TearDown]
        public void Clear()
        {
            connection.Close();
            connection.Dispose();
        }
    }
}

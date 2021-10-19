using System.Collections.Generic;
using DiCore.Lib.PgSqlAccess.DataAccessObjects;
using DiCore.Lib.TestModels.Models;
using Npgsql;
using NUnit.Framework;

namespace DiCore.Lib.PgSqlAccess.Tests
{
    [TestFixture]
    public class PgCopyToUnitTest
    {
        private string connectionStringToDcPrimary = "Server=vds01-tetemp-31;Port=5432;database=DCPrimary;User Id=postgres;Password=123qweQWE;Convert Infinity DateTime=true; Command Timeout=300;";
        private string connectionStringToSandbox = "Host=vds01-tetemp-31;Port=5432;Username=postgres;Password=123qweQWE;Database=TitanDbTest;Convert Infinity DateTime=True;Timeout=15";
        private string sessionSchemaName = "1ee75a88-4b97-45c3-a614-86186351eb84";

        private IEnumerable<ArtifactClass> GetArtifactClass()
        {
            var connection = new NpgsqlConnection(connectionStringToDcPrimary);
            connection.Open();

            var queryText = @"SELECT * FROM ""inh"".""ArtifactClass""";
            var query = new PgQuery(connection, queryText);
            var additionalArtifact = query.ExecuteReaderCursor<ArtifactClass>();
            return additionalArtifact;
        }

        //[Test]
        public void ImportArtifactClass()
        {
            using (var connection = new NpgsqlConnection(connectionStringToSandbox))
            {
                connection.Open();

                PgCopyTo<ArtifactClass> pgCopyTo = new PgCopyTo<ArtifactClass>(connection, sessionSchemaName, "ArtifactClass");
                pgCopyTo.Run(GetArtifactClass());

            }
        }
    }
}
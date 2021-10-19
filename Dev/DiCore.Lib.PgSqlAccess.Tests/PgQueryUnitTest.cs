using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiCore.Lib.PgSqlAccess.DataAccessObjects;
using DiCore.Lib.SqlDataQuery.Builders;
using DiCore.Lib.SqlDataQuery.QueryParameter;
using DiCore.Lib.TestModels.Models;
using Npgsql;
using NUnit.Framework;

namespace DiCore.Lib.PgSqlAccess.Tests
{
    [TestFixture]
    public class PgQueryUnitTest
    {
        private string schema = "56940a98-fdf1-488d-a6e8-bb62df14bac1";
        private string connectionString;
        private NpgsqlConnection connection;

        //[SetUp]
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

        //[TearDown]
        public void Clear()
        {
            connection.Close();
            connection.Dispose();
        }

        //[Test]
        public void ExecuteReaderCursorTest()
        {
            var query = $"SELECT \"Id\", \"Name\" FROM \"{schema}\".\"SectionType\" LIMIT 100";

            using (var connection = new NpgsqlConnection(connectionString))
            using (var pgQuery = new PgQuery(connection, query))
            {
                connection.Open();
                var res = pgQuery.ExecuteReaderCursor<SectionType>();

                Assert.IsNotNull(res);
            }
        }

        //[Test]
        public void GetSectionsTest()
        {
            var result = GetSectionsAsync(
                QueryParametersBuilder
                    .Create<Section>()
                    .Where(x => x.Distance + x.Length < 100)
                    .Skip(0)
                    .Take(100)
                    .Build()).Result.ToArray();

            Assert.IsNotNull(result);
        }

        //[Test]
        public void GetArtifactTest()
        {
            var sql = QueryCreatorBuilder.Create<Artifact>(schema)
                .MapEntity()
                .Build(QueryParametersBuilder.Create<Artifact>().Where(x => x.BaseObjectId != null).Take(10));

            var q = new PgQuery(connection, sql);
            var result = q.ExecuteReaderCursor<Artifact>(byNames: true).ToArray();

            Assert.AreEqual(result.Length, 10);
        }

        //[Test]
        public async Task GetArtifactAsyncTest()
        {
            var sql = QueryCreatorBuilder.Create<Artifact>(schema)
                .MapEntity()
                .Build(QueryParametersBuilder.Create<Artifact>().Where(x => x.BaseObjectId != null).Take(10));

            var q = new PgQuery(connection, sql);
            var result = await q.ExecuteReaderCursorAsync<Artifact>(byNames: true);

            var count = result.Count();

            Assert.AreEqual(count, 10);
        }

        //[Test]
        public async Task GetArtifactWithAddodDataTest()
        {
            var sql = QueryCreatorBuilder.Create<Artifact>(schema)
                .Map(x => x)

                .MapJoin(x => x.ArtifactAddonMeasured.Id, x => x.Id)
                .MapJoin(x => x.DiagnosticMethod.Id)
                .MapJoin(x => x.ArtifactAddonMeasured.SurfaceLocation.Id)
                .MapJoin(x => x.Class.Id, x => x.ClassId)

                .Build(QueryParametersBuilder.Create<Artifact>().Take(1));


            var q = new PgQuery(connection, sql);
            var result = await q.ExecuteReaderCursorAsync<Artifact>(byNames: true);
            var artifact = result.FirstOrDefault();

            Assert.IsNotNull(artifact);
            Assert.IsNotNull(artifact.ArtifactAddonMeasured);
            Assert.IsNotNull(artifact.ArtifactAddonMeasured.SurfaceLocation);
            Assert.IsNotNull(artifact.Class);
            Assert.IsNotNull(artifact.DiagnosticMethod);
        }

        //[Test]
        public void FillComplexTypeTest()
        {
            var qc = QueryCreatorBuilder.Create<DiagnosticTarget>(schema)
                .Map(x => x.Id)
                .Map(x => x.CustomerContractorId)
                .Map(x => x.StartDate)
                .Map(x => x.EndDate)
                .Map(x => x.PerformerContractorId)
                .Map(x => x.Comment)
                .Map(x => x.RunCode)
                .Map(x => x.ReportNumber)
                .Map(x => x.StartRouteId)
                .Map(x => x.StartRouteDistance)
                .Map(x => x.EndRouteId)
                .Map(x => x.EndRouteDistance)
                .Map(x => x.PigId)
                .Map(x => x.Diameter)
                .Map(x => x.DiagnosticMethod)                
                .MapJoin(x => x.Diameter.Id, x => x.DiameterId)
                .MapJoin(x => x.DiagnosticMethod.Id)
                .Build();

            var sql = qc.Create(QueryParametersBuilder.Create<DiagnosticTarget>().Take(1).Build());
            var q = new PgQuery(connection, sql);
            var result = q.ExecuteReaderOne<DiagnosticTarget>(out var target, byNames: true);

            Assert.IsTrue(result);
            Assert.IsNotNull(target);
            Assert.IsNotNull(target.Diameter);
            Assert.IsNotNull(target.DiagnosticMethod);
        }

        //[Test]
        public void AddCustomColumnTest()
        {
            var qc = QueryCreatorBuilder
                .Create<DiagnosticTarget>(schema)
                .MapEntity();
        }

        private Task<IEnumerable<Section>> GetSectionsAsync(QueryParameters parameters)
        {
            return Task.Run(() =>
            {
                var qc = QueryCreatorBuilder.Create<Section>(schema).MapEntity().Build();
                var sql = qc.Create(parameters);                              
                var q = new PgQuery(connection, sql);
                var reader = q.ExecuteReaderCursor<Section>(byNames: true);

                return reader;
            });
        }        
    }
}
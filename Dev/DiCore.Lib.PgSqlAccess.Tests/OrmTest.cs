using DiCore.Lib.PgSqlAccess.Orm;
using DiCore.Lib.PgSqlAccess.Tests.Orm;
using DiCore.Lib.PgSqlAccess.Tests.Orm.QueryCreators;
using Cut = DiCore.Lib.TestModels.Models.Cut;
using Simple = DiCore.Lib.TestModels.Models.Simple;
using JObject = DiCore.Lib.TestModels.Models.JObject;
using Special = DiCore.Lib.TestModels.Models.Special;
using Generic = DiCore.Lib.TestModels.Models.Generic;
using Input = DiCore.Lib.TestModels.Models.Input;
using Json = DiCore.Lib.TestModels.Models.Json;
using DiCore.Lib.TestModels.Models;
using Npgsql;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using DiCore.Lib.PgSqlAccess.Orm.Query;
using DiCore.Lib.PgSqlAccess.DataAccessObjects;
using System.Transactions;
using System.Threading.Tasks;

namespace DiCore.Lib.PgSqlAccess.Tests
{
    public class OrmTest
    {
        private IDataAdapter dataAdapter;
        private IConnectionFactory connectionFactory;
        private QueryConstructorManager queryConstructorManager;

        [SetUp]
        public void Setup()
        {
            var csb = new NpgsqlConnectionStringBuilder
            {
                Host = "vds01-tetemp-31",
                Database = "ModelTest",
                Username = "postgres",
                Password = "123qweQWE"
            };
            connectionFactory = new ConnectionFactory(csb.ConnectionString);
            queryConstructorManager = new QueryConstructorManager();
            InitQueryCreators();
            dataAdapter = new DataAdapter(connectionFactory, queryConstructorManager);
            dataAdapter.Schema = "49456aca-8fa3-4bbf-b392-daabab5013cb";
        }

        private void InitQueryCreators()
        {
            queryConstructorManager.Add<Simple.Artifact>(new QueryCreatorArtifactSimple());
            queryConstructorManager.Add<JObject.Token>(new QueryCreatorTokenJObject());
            queryConstructorManager.Add<Special.Token>(new QueryCreatorTokenSpecial());
            queryConstructorManager.Add(new QueryConstructorConfiguratorTokenId());
        }

        private IEnumerable<T> GetAll<T>()
        {
            var query = dataAdapter.CreateQuery<T>();

            var result = query.ExecuteAsync().Result;
            return result;
        }

        [Test]
        public void CheckSelectDictionaries()
        {
            var sectionTypes = GetAll<SectionType>().ToList();
            Assert.IsNotEmpty(sectionTypes);

            var pipeTypes = GetAll<PipeType>().ToList();
            Assert.IsNotEmpty(pipeTypes);

            var eventTypes = GetAll<EventType>().ToList();
            Assert.IsNotEmpty(eventTypes);

            var constructiveDiameters = GetAll<ConstructiveDiameter>().ToList();
            Assert.IsNotEmpty(constructiveDiameters);

            var diagnosticMethods = GetAll<DiagnosticMethod>().ToList();
            Assert.IsNotEmpty(diagnosticMethods);

            var coordinateSystems = GetAll<CoordinateSystem>().ToList();
            Assert.IsNotEmpty(coordinateSystems);

            var jsonSchemas = GetAll<JsonSchema>().ToList();
            Assert.IsNotEmpty(jsonSchemas);

            var artifactClasses = GetAll<ArtifactClass>().ToList();
            Assert.IsNotEmpty(artifactClasses);

            var POFArtifactTypes = GetAll<POFArtifactType>().ToList();
            Assert.IsNotEmpty(POFArtifactTypes);

            var pipelineCrossTypes = GetAll<PipelineCrossType>().ToList();
            Assert.IsNotEmpty(pipelineCrossTypes);

            var waterCrossTypes = GetAll<WaterCrossType>().ToList();
            Assert.IsNotEmpty(waterCrossTypes);

            var crossClasses = GetAll<CrossClass>().ToList();
            Assert.IsNotEmpty(crossClasses);
        }

        [Test]
        public void CheckSelectTokenSimple()
        {
            var tokens = dataAdapter.CreateQuery<Simple.Token>().ExecuteAsync().Result.ToList();

            Assert.NotNull(tokens);
            Assert.IsNotEmpty(tokens);

        }

        [Test]
        public void CheckSelectTokenJobject()
        {
            var token = dataAdapter
                .CreateQuery<JObject.Token>()
                .ExecuteAsync()
                .Result
                .FirstOrDefault();

            Assert.NotNull(token);
            Assert.IsFalse(token.Id == Guid.Empty);
        }

        [Test]
        public void CheckSelectTokenSpecial()
        {
            var token = dataAdapter.CreateQuery<Special.Token>()
                .ExecuteAsync()
                .Result
                .FirstOrDefault();

            Assert.NotNull(token);
            Assert.IsFalse(token.Id == Guid.Empty);
        }

        [Test]
        public void InsertTokenTest()
        {
            var tokenId = InsertToken().Result;
            Assert.IsNotNull(tokenId);
        }

        private async Task<Guid?> InsertToken()
        {
            var dpe = new Input.DataProcessEventInsert<Special.DataProcessEventJson>()
            {
                CreatorId = Guid.NewGuid(),
                DateCreate = DateTime.Now,
                ParentId = null,
                EventTypeId = 2,
                Parameters = null,
                Description = ""
            };

            var token = new Input.TokenInsert<Json.TokenDataJson>()
            {
                Data = new Json.TokenDataJson()
                {
                    SourceDiagnosticTargetId = null,
                    SourceSessionId = null
                }
            };

            var storedProcedure = new[] {token};

            var change = dataAdapter
                .CreateChange(storedProcedure)
                .SetJson(x => x.Data)
                .ChangeBeforeExecuteJson<Guid, Special.DataProcessEventJson >(dpe, (t, id) => t.DataProcessEventId = id)
                .UseTransaction();

            var tokenIds = change.ExecuteScalarAsync<Guid>().Result;

            var tokenId = tokenIds.FirstOrDefault();

            var insertedToken = (await dataAdapter
                .CreateQuery<Special.Token>()
                .Where(x => x.Id == tokenId)
                .Take(1)
                .ExecuteAsync()).FirstOrDefault();

            return insertedToken?.Id;
        }

        [Test]
        public void CheckMore100NewConnection()
        {
            foreach (var query in Enumerable.Repeat(dataAdapter.CreateQuery<JObject.Token>(), 1000))
            {
                var result = query.ExecuteAsync().Result.FirstOrDefault();
                Assert.IsNotNull(result);
            }
        }

        [Test]
        public void CheckInQuery()
        {
            var count = 100;

            var ids = dataAdapter
                .CreateQuery<ArtifactClass>()
                .Take(count)
                .ExecuteAsync()
                .Result
                .Select(x => x.Id)
                .ToList();

            Assert.IsTrue(ids.Count == count);

            var result = dataAdapter
                .CreateQuery<ArtifactClass>()
                .In(x => x.Id, ids)
                .ExecuteAsync()
                .Result
                .ToList();

            Assert.IsTrue(result.Count == count);
        }

        [Test]
        public void CheckInNullableQuery()
        {
            var count = 3;

            var codes = dataAdapter
                .CreateQuery<ArtifactClass>()
                .Take(count)
                .ExecuteAsync()
                .Result
                .Select(x => x.Code)
                .ToList();

            Assert.IsTrue(codes.Count == count);

            var result = dataAdapter
                .CreateQuery<ArtifactClass>()
                .NotIn(x => x.Code, codes)
                .ExecuteAsync()
                .Result
                .ToList();

            var equilResult = result.Where(x => x.Code != codes[0] && x.Code != codes[1] && x.Code != codes[2]).ToList();

            Assert.IsTrue(result.Count == equilResult.Count);
        }

        [Test]
        public void CheсkByResultType()
        {
            var query = dataAdapter.CreateQuery<Simple.Token>();

            var result = query.ExecuteByTypeResultAsync<Cut.TokenId>().Result.ToList();

            Assert.IsNotEmpty(result);
        }
    }
}

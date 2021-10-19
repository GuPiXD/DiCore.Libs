using System;
using DiCore.Lib.SqlDataQuery.Builders;
using DiCore.Lib.SqlDataQuery.QueryParameter;
using DiCore.Lib.TestModels.Models;
using NUnit.Framework;

namespace Titan.DataAccess.Repository.Tests
{    
    [TestFixture]
    public class QueryBuildersUnitTest
    {
        private string schema = "01783753-3ea2-45f0-9615-235a6f3f7c8f";

        [Test]
        public void MapTest()
        {           
            var qc = QueryCreatorBuilder.Create<Section>(schema)
                .Map(x => x.Number)
                .Map(x => x.Id)
                .Map(x => x.Length)                
                .Map(x => x.Distance)                
                .Map(x => x.SectionType.Id)
                .Map(x => x.SectionType.Name)
                .Map(x => x.PipeType.Id)

                .MapJoin(x => x.SectionType.Id)
                .MapJoin(x => x.PipeType.Id)

                .Build();


            var text = qc.Create(QueryParameters.Empty);
            Assert.IsFalse(String.IsNullOrEmpty(text));            
        }

        [Test]
        public void ComplexTypeMapTest()
        {
            var qc = QueryCreatorBuilder.Create<Section>(schema)
                .Map(x => x.Number)
                .Map(x => x.Id)
                .Map(x => x.Length)                
                .Map(x => x.Distance)                
                .Map(x => x.SectionType.Id)
                .Map(x => x.SectionType.Name)
                .Map(x => x.PipeType.Id)

                .MapJoin(x => x.SectionType.Id)
                .MapJoin(x => x.PipeType.Id)

                .Build();

            var text1 = qc.Create(QueryParameters.Empty);

            qc = QueryCreatorBuilder.Create<Section>(schema)
                .Map(x => x.Number)
                .Map(x => x.Id)
                .Map(x => x.Length)                
                .Map(x => x.Distance)                
                .Map(x => x.SectionType)
                .Map(x => x.PipeType.Id)

                .MapJoin(x => x.SectionType.Id)
                .MapJoin(x => x.PipeType.Id)

                .Build();

            var text2 = qc.Create(QueryParameters.Empty);

            Assert.AreEqual(text1, text2);
        }

        [Test]
        public void ComplexTypeMap2Test()
        {
            var qc = QueryCreatorBuilder.Create<Artifact>(schema)
                .Map(x => x.Id)
                .Map(x => x.ArtifactAddonMeasured)
                .Map(x => x.Description)
                .Map(x => x.Number)
                .Map(x => x.Class)
                .Map(x => x.BaseObjectId)
                .Map(x => x.DiagnosticObjectId)
                .Map(x => x.DiagnosticMethod)
                .Map(x => x.Data)

                .MapJoin(x => x.ArtifactAddonMeasured.Id, x => x.Id)
                .MapJoin(x => x.DiagnosticMethod.Id)
                .MapJoin(x => x.ArtifactAddonMeasured.SurfaceLocation.Id)
                .MapJoin(x => x.Class.Id, x => x.ClassId)

                .Build();

            var text = qc.Create(QueryParameters.Empty);
            Assert.IsFalse(String.IsNullOrEmpty(text));
        }

        [Test]
        public void ComplexTypeMap3Test()
        {
            var qc = QueryCreatorBuilder.Create<Artifact>(schema)
                .Map(x => x)

                .MapJoin(x => x.ArtifactAddonMeasured.Id, x => x.Id)
                .MapJoin(x => x.DiagnosticMethod.Id)
                .MapJoin(x => x.ArtifactAddonMeasured.SurfaceLocation.Id)
                .MapJoin(x => x.Class.Id, x => x.ClassId)

                .Build();

            var text = qc.Create(QueryParameters.Empty);
            Assert.IsFalse(String.IsNullOrEmpty(text));
        }

        [Test]
        public void MapEntityTest()
        {            
            var qc = QueryCreatorBuilder
                .Create<Section>(schema)
                .MapEntity()
                .Build();

            var text = qc.Create(QueryParameters.Empty);
            Assert.IsFalse(String.IsNullOrEmpty(text));
        }  

        [Test]
        public void MapComplexEntityTest()
        {
            var qc = QueryCreatorBuilder
                .Create<Artifact>(schema)
                .MapEntity()
                .Build(QueryParametersBuilder.Create<Artifact>().Where(x => x.BaseObjectId != null));

            Assert.IsFalse(String.IsNullOrEmpty(qc));
        }  
        
        [Test]
        public void WhereClauseTest()
        {            
            var qc = QueryCreatorBuilder
                .Create<Section>(schema)
                .MapEntity()
                .Build();

            var qp = QueryParametersBuilder
                .Create<Section>()
                .Where(x => x.Distance + x.Length < 1000)
                .Where(x => x.Distance + x.Length * 1000 < 20)
                .Where(x => (x.Distance + x.Length) * 10 < x.Number * 78)
                .Build();

            var text = qc.Create(qp);
            Assert.IsFalse(String.IsNullOrEmpty(text));
        }

        [Test]
        public void OrWhereClauseTest()
        {
            var qc = QueryCreatorBuilder
                .Create<ArtifactAddonMeasured>(schema)
                .MapEntity()
                .Build();

            var qp = QueryParametersBuilder
                .Create<ArtifactAddonMeasured>()
                .Where(x => x.SurfaceLocation.Id == 0 || x.SurfaceLocation.Id == 2)
                .Take(1000)
                .Build();

            var text = qc.Create(qp);
            Assert.IsFalse(String.IsNullOrEmpty(text));
        }

        [Test]
        public void BoolPropertyTest()
        {
            var qc = QueryCreatorBuilder.Create<DiagnosticTarget>(schema)
                .Map(x => x.Comment)
                .Map(x => x.CustomerContractorId)
                .Map(x => x.Diameter.DiameterMm)
                .MapJoin(x => x.Diameter.Id, x => x.DiameterId)
                .Build();
            
            var text = qc.Create(QueryParameters.Empty);
            Assert.IsFalse(String.IsNullOrEmpty(text));
        }

        [Test]
        public void DateTimeClauseTest()
        {
            var dtStart = DateTime.Now.AddMonths(-1);
            var dtNow = DateTime.Now;
            var qp = QueryParametersBuilder
                .Create<DiagnosticTarget>()
                .Where(x => x.StartDate >= dtStart)
                .Where(x => x.StartDate <= dtNow)
                .Take(100)
                .Build();

            var qc = QueryCreatorBuilder.Create<DiagnosticTarget>(schema)
                .Map(x => x.Comment)
                .Map(x => x.CustomerContractorId)
                .Map(x => x.Diameter.DiameterMm)
                .MapJoin(x => x.Diameter.Id, x => x.DiameterId)
                .Build();

            var text = qc.Create(qp);
            Assert.IsFalse(String.IsNullOrEmpty(text));
        }

        [Test]
        public void DistinctColumnTest()
        {
            var qc = QueryCreatorBuilder.Create<Section>(schema)
                .Map(x => x.Number, distinct: true)
                .Map(x => x.Id)
                .Map(x => x.Length)                
                .Map(x => x.Distance)                
                .Map(x => x.SectionType)
                .Map(x => x.PipeType.Id)

                .MapJoin(x => x.SectionType.Id)
                .MapJoin(x => x.PipeType.Id)

                .Build();

            var text = qc.Create(QueryParameters.Empty);
            Assert.IsFalse(String.IsNullOrEmpty(text));
        }

        [Test]
        public void DiagnosticDataTest()
        {
            var qc = QueryCreatorBuilder.Create<DiagnosticData>(schema)
                .MapEntity()
                .Build();
            var qp = QueryParametersBuilder.Create<DiagnosticData>()
                .Where(x => x.DiagnosticTarget.StartDate >= new DateTime(2017, 05, 17))
                .Where(x => x.DiagnosticTarget.StartDate <= new DateTime(2019, 05, 18))
                .OrderByDescending(x => x.DiagnosticTarget.StartDate)
                .Build();

            var text = qc.Create(qp);
            Assert.IsFalse(String.IsNullOrEmpty(text));
        }

        [Test]
        public void AsNameTest()
        {
            var qc = QueryCreatorBuilder
                .Create<Section>(schema)
                .MapEntity()
                .Build(QueryParameters.Empty);

            var qc1 = QueryCreatorBuilder
                .Create<Section>(schema)
                .MapEntity()
                .Build(QueryParameters.Empty);

            Assert.AreEqual(qc, qc1);
        }

        [Test]
        public void AsJson()
        {
            var qc = QueryCreatorBuilder
                .Create<Artifact>(schema)
                .MapJson(x => x.ArtifactAddonMeasured)
                .Build(QueryParameters.Empty);

            Assert.IsFalse(String.IsNullOrEmpty(qc));
        }

        [Test]
        public void MultiMapTest()
        {
            var qc = QueryCreatorBuilder
                .Create<SectionWithArtifacts>(schema, nameof(Section))
                .MapEntity()
                .Build(QueryParameters.Empty);

            Assert.Pass();
        }

        [Test]
        public void SortTest()
        {
            var qc = QueryCreatorBuilder
                .Create<DiagnosticTarget>(schema)
                .MapEntity()
                .Build();

            var qp = QueryParametersBuilder.Create<DiagnosticTarget>()
                .OrderByDescending(x => x.EndDate)
                .Build();

            Assert.AreEqual(qp.Sortings.Count, 1);

            qp.Sortings[0].NullPosition = enSortNullPosition.First;
            var sql = qc.Create(qp);

            Assert.IsFalse(String.IsNullOrEmpty(sql));
        }

        [Test]
        public void WhereWithComplexTypeTest()
        {
            var qc = QueryCreatorBuilder.Create<Artifact>(schema)
                .Map()

                .MapJoin(x => x.ArtifactAddonMeasured.Id, x => x.Id)
                .MapJoin(x => x.DiagnosticMethod.Id)
                .MapJoin(x => x.ArtifactAddonMeasured.SurfaceLocation.Id)
                .MapJoin(x => x.Class.Id, x => x.ClassId)

                .Build();

            var qp = QueryParametersBuilder
                .Create<Artifact>()
                .Where(x => x.ArtifactAddonMeasured.SurfaceLocation.Id == 0)
                .Take(100)
                .Build();

            var sql = qc.Create(qp);

            Assert.IsFalse(String.IsNullOrEmpty(sql));
        }

        [Test]
        public void MapGenericEntityTest()
        {
            var qc = QueryCreatorBuilder
                .Create<JsonModelWrapper<Section, Artifact>>(schema, mainTableName: "main_table_name")
                .MapJson(x => x.Value)
                .MapJson(x => x.EventParameters.Value)
                .MapJoin(x => x.EventParameters.Id)
                .MapEntity()
                .Build()
                .Create(QueryParameters.Empty);

            Assert.IsFalse(String.IsNullOrEmpty(qc));
            Assert.IsTrue(qc.IndexOf("main_table_name", StringComparison.OrdinalIgnoreCase) != -1);
        }  
    }
}

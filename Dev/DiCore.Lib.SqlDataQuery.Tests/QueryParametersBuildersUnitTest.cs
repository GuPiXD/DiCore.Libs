using System;
using System.Linq;
using DiCore.Lib.SqlDataQuery.Builders;
using DiCore.Lib.TestModels.Models;
using NUnit.Framework;

namespace DiCore.Lib.SqlDataQuery.Tests
{
    [TestFixture]
    public class QueryParametersBuildersUnitTest
    {                
        [Test]
        public void UnaryEqualExpressionTest()
        {            
            var id = new Guid("5372C66F-20B6-4CB6-B6C4-84564A3FB359");

            var qp = QueryParametersBuilder
                .Create<Section>()
                .Where(x => x.Id == id)
                .Build();

            Assert.IsTrue(qp.Filters.Count > 0);
        }

        [Test]
        public void BinaryOrExpressionTest()
        {
            var qp = QueryParametersBuilder
                .Create<Section>()
                .Where(x => x.Distance > 100 || x.Length < 10)
                .Build();

            Assert.IsTrue(qp.Filters?.Count > 0);
        }

        [Test]
        public void TwoBinaryExpressionTest()
        {
            var qp = QueryParametersBuilder
                .Create<Section>()
                .Where(x => (x.Distance > 100.5 || x.Length < 10) && x.Altitude > 2)
                .Where(x => x.SectionType.Id == (int)SectionTypes.Pipe)
                .Build();

            Assert.IsTrue(qp.Filters.Count == 3);
            Assert.IsTrue(qp.Filters[0].Filters.Count == 2);
        }

        [Test]
        public void TwoBinaryExpression2Test()
        {
            var qp = QueryParametersBuilder
                .Create<Section>()
                .Where(x => x.Distance > 100.5 || x.Length < 10 || x.Altitude > 2)
                .Build();

            Assert.IsTrue(qp.Filters.Count == 1);
            Assert.IsTrue(qp.Filters[0].Filters.Count == 3);
        }

        [Test]
        public void SkipTakeTest()
        {
            var qp = QueryParametersBuilder
                .Create<Section>()
                .Skip(100)
                .Take(100)
                .Build();

            Assert.AreEqual(qp.Skip, 100);
            Assert.AreEqual(qp.Take, 100);
        }

        [Test]
        public void WhereWithComplexTypeTest()
        {
            var qp = QueryParametersBuilder
                .Create<Artifact>()
                .Where(x => x.ArtifactAddonMeasured.SurfaceLocation.Id == 0)
                .Take(100)
                .Build();

            Assert.IsNotNull(qp.Filters.SelectMany(f => f.Filters)
                .FirstOrDefault(f => f.ToString().IndexOf("\"ArtifactAddonMeasured#SurfaceLocation\".\"Id\"", StringComparison.OrdinalIgnoreCase) != -1));
        }

        [Test]
        public void OrderWithComplexTypeTest()
        {
            var qp = QueryParametersBuilder
                .Create<DiagnosticData>()
                .OrderByDescending(x => x.DiagnosticTarget.DiagnosticMethod.Id)
                .Take(100)
                .Build();

            Assert.IsNotNull(qp.Sortings.FirstOrDefault(s => s.Column == "DiagnosticTarget#DiagnosticMethod#Id"));
        }

        [Test]
        public void IsNullClauseTest()
        {
            var qp = QueryParametersBuilder
                .Create<Section>()
                .Where(x => x.Altitude == null)
                .Build();

            Assert.IsTrue(qp.Filters.Count > 0);
        }

        [Test]
        public void PagingTest()
        {
            var qp = QueryParametersBuilder
                .Create<Section>()
                .Page(1, 200)
                .Build();

            Assert.AreEqual(qp.Page - 1, 1);
            Assert.AreEqual(qp.PageSize, 200);
        }

        [Test]
        public void OrderByTest()
        {
            var qp = QueryParametersBuilder
                .Create<Section>()
                .OrderBy(x => x.SectionType.Name)
                .Build();

            Assert.IsTrue(qp.Sortings.Count > 0);
            Assert.IsNotNull(qp.Sortings.FirstOrDefault(s => s.Column == "SectionType#Name"));

            qp = QueryParametersBuilder
                .Create<Section>()
                .OrderByDescending(x => x.Distance)
                .Build();

            Assert.IsTrue(qp.Sortings.Count > 0);
            Assert.IsNotNull(qp.Sortings.FirstOrDefault(s => s.Column == "Distance"));
        }

        [Test]
        public void ClauseOperationQueryTest()
        {           
            var qp = QueryParametersBuilder.Create<Section>()
                .Where(x => x.Distance + x.Length < 1000)
                .Build();
            
            Assert.IsTrue(qp.Filters.Count > 0);
        }   

        [Test]
        public void ClauseWithRefTest()
        {
            var section = new Section() {Length = 100};
            var qp = QueryParametersBuilder.Create<Section>()
                .Where(x => x.Distance + x.Length < section.Length)
                .Build();
            
            Assert.IsTrue(qp.Filters.Count > 0);
        }   
        
        [Test]
        public void LikeClauseQueryTest()
        {           
            var qp = QueryParametersBuilder.Create<Section>()
                .Where(x => x.Data.Contains("test"))
                .Build();
            
            Assert.IsTrue(qp.Filters.Count > 0);
        } 
        
        [Test]
        public void LikeCaseInsensitiveClauseQueryTest()
        {           
            var qp = QueryParametersBuilder.Create<Section>()
                .Where(x => x.Data.ToLowerInvariant().Contains("test"))
                .Build();
            
            Assert.IsTrue(qp.Filters.Count > 0);
        }

        [Test]
        public void LikeClauseWithRefTest()
        {
            var t = "2";
            var qp = QueryParametersBuilder
                .Create<DiagnosticTarget>()
                .Where(x => x.ReportNumber.Contains(t))
                .Build();

            Assert.IsTrue(qp.Filters.Count > 0);       
        }

        [Test]
        public void LikeClauseWithRefTest2()
        {
            var v = new {Value = "2"};
            var qp = QueryParametersBuilder
                .Create<DiagnosticTarget>()
                .Where(x => x.ReportNumber.Contains(v.Value))
                .Build();

            Assert.IsTrue(qp.Filters.Count > 0);        
        }
    }
}

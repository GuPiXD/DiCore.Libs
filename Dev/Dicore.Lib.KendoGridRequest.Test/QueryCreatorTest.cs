using System;
using System.Collections.Generic;
using DiCore.Lib.KendoGridRequest;
using DiCore.Lib.KendoGridRequest.KendoGrid;
using DiCore.Lib.KendoGridRequest.SqlCode;
using DiCore.Lib.PgSqlAccess.DataAccessObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace Dicore.Lib.KendoGridRequest.Test
{
    [TestClass]
    public class QueryCreatorTest
    {
        [TestMethod]
        public void QueryWithEmptyConstraintsTest()
        {
            var queryDesc = new QueryDescription();
            var table = queryDesc.AddTable("public", "Test", true);
            table.AddColumn("Id");
            table.AddColumn("ArtifactLength");
            table.AddColumn("ArtifactNumber");

            var input = new JObject();
            input.Add("Visible", false);
            input.Add("Configuration", null);
            var requestConstr = new RequestConstraints(input);
            var queryCreator = new QueryCreator(queryDesc, requestConstr);
            var requestParams = new RequestParameters(null);
            var queryText = queryCreator.Create(requestParams);
        }

        [TestMethod]
        public void QueryWithConstraintsTest()
        {
            var queryDesc = new QueryDescription();
            var table = queryDesc.AddTable("public", "Test", true);
            table.AddColumn("Id");
            table.AddColumn("ArtifactLength");
            table.AddColumn("ArtifactNumber");

            var conf = JObject.Parse("{ \"Id\": { \"present\": false, \"visible\": false }, \"ArtifactLength\": { \"present\": true, \"visible\": true}, \"ArtifactNumber\": {\"present\": true, \"visible\": false }}");
            var input = new JObject();
            input.Add("Visible", true);
            input.Add("Configuration", conf);
            var requestConstr = new RequestConstraints(input);
            var queryCreator = new QueryCreator(queryDesc, requestConstr);
            var requestParams = new RequestParameters(null);
            var queryText = queryCreator.Create(requestParams);
        }

        [TestMethod]
        public void QueryWithDontAllConstraintsTest()
        {
            var queryDesc = new QueryDescription();
            var table = queryDesc.AddTable("public", "Test", true);
            table.AddColumn("Id");
            table.AddColumn("ArtifactLength");
            table.AddColumn("ArtifactNumber");

            var conf = JObject.Parse("{ \"Id\": { \"present\": false, \"visible\": false }}");
            var input = new JObject();
            input.Add("Visible", true);
            input.Add("Configuration", conf);
            var requestConstr = new RequestConstraints(input);
            var queryCreator = new QueryCreator(queryDesc, requestConstr);
            var requestParams = new RequestParameters(null);
            var queryText = queryCreator.Create(requestParams);
        }

        [TestMethod]
        public void TestTest()
        {
            var query = "SELECT COUNT(diagnosticobject.\"Id\") OVER() AS \"totalCount\", \"section\".\"Id\" AS \"Id\", \"section\".\"Length\" AS \"Length\", \"section\".\"AverageWallThickness\" AS \"AverageWallThickness\", \"section\".\"AxialWeldStartAngle\" AS \"AxialWeldStartAngle\", \"section\".\"AxialWeldEndAngle\" AS \"AxialWeldEndAngle\", \"section\".\"AxialWeldSecondAngle\" AS \"AxialWeldSecondAngle\", \"section\".\"Altitude\" AS \"Altitude\", \"sectiontype\".\"Name\" AS \"SectionTypeName\", \"pipetype\".\"Id\" AS \"PipeTypeId\", \"pipetype\".\"Name\" AS \"PipeTypeName\", (SELECT concat('Ш:', round(cast((diagobjcoord.\"Coordinate\" -> 'Lat') :: TEXT AS NUMERIC), 6), ', Д:', round(cast((diagobjcoord.\"Coordinate\"-> 'Lon') :: TEXT AS NUMERIC), 6), ', В:', round(cast((diagobjcoord.\"Coordinate\"-> 'Alt') :: TEXT AS NUMERIC), 6)) FROM diagnostic.\"DiagnosticObjectCoordinate\" AS diagobjcoord WHERE diagobjcoord.\"CoordinateSystemId\" = '72b16dc7-f56e-4203-ba60-7b369f444c88' AND diagnosticobject.\"Id\" = diagobjcoord.\"DiagnosticObjectId\") AS \"Coordinate\" FROM diagnostic.\"DiagnosticObject\" AS \"diagnosticobject\" LEFT JOIN diagnostic.\"DiagnosticTarget\" AS \"diagnostictarget\" ON \"diagnostictarget\".\"Id\" = \"diagnosticobject\".\"DiagnosticTargetId\" LEFT JOIN diagnostic.\"Section\" AS \"section\" ON \"section\".\"Id\" = \"diagnosticobject\".\"Id\" LEFT JOIN diagnostic.\"SectionType\" AS \"sectiontype\" ON \"sectiontype\".\"Id\" = \"section\".\"SectionTypeId\" LEFT JOIN dir.\"PipeType\" AS \"pipetype\" ON \"pipetype\".\"Id\" = \"section\".\"PipeTypeId\" WHERE diagnosticobject.\"DiagnosticTargetId\" = '35adbe80-e0ca-44e1-be12-0a17a04358a3'And diagnosticobject.\"vrDeleted\" IS false ORDER BY \"section\".\"Number\" asc OFFSET 0 LIMIT 200;";
            var connStr = "Server=sds01-depgsql01;Port=5432;Database=PCloud;User Id=postgres;Password=123qweQWE;Convert Infinity DateTime=true;";

            List<DiagSection> result;
            var connection = new NpgsqlConnection(connStr);
            connection.Open();
            var pgQuery = new PgQuery(connection, query);
            result = pgQuery.ExecuteReader<DiagSection>(null, true);
        }
    }

    public class DiagSection
    {
        public long totalCount { get; set; }
        public Guid Id { get; set; }
        public double? Distance { get; set; }
        public int? Number { get; set; }
        public float Length { get; set; }
        public float? AverageWallThickness { get; set; }
        public float? AxialWeldStartAngle { get; set; }
        public float? AxialWeldEndAngle { get; set; }
        public float? AxialWeldSecondAngle { get; set; }
        
        public float? Altitude { get; set; }
        public string SectionTypeName { get; set; }
        public int PipeTypeId { get; set; }
        public string PipeTypeName { get; set; }
        public string Coordinate { get; set; }
    }
}

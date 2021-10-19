using System;
using System.Collections.Generic;
using System.Text;
using DiCore.Lib.SqlDataQuery.QueryParameter;
using DiCore.Lib.SqlDataQuery.SqlCode;
using NUnit.Framework;

namespace DiCore.Lib.SqlDataQuery.Tests
{
    [TestFixture]
    public class QueryCreatorUnitTest
    {
        [Test]
        public void FilterWithCustomConditionTest()
        {
            var qd = new QueryDescription();
            var table = qd.AddTable("schema", "table", true);
            table.AddColumn("column1");
            table.AddColumn("column2");
            table.AddColumn("column3");
            qd.AddCustomCondition(EnConditionType.And, "\"column1\" = 1");

            var qp = new QueryParameters();
            qp.Filters.Add(new ColumnFilters()
            {
                Logic = "or",
                Filters = new List<Filter>()
                {
                    new Filter() {Field = "column3", Operator = EnFilterOperation.Equals, Value = "3"},
                    new Filter() {Field = "column2", Operator = EnFilterOperation.Equals, Value = "2"}
                }
            });
            var qc = new QueryCreator(qd);
            var sql = qc.Create(qp);

            Assert.IsTrue(sql.Contains('(') && sql.Contains(')'));
        }

        [Test]
        public void SelectTest()
        {
            var dbObjects = new QueryDescription();
            var langMod = "RU";
            var EnUserPermission_Translator = false;
            var EnUserPermission_Expert = true;
            var ContractorId = new Guid("17141008-7E6A-49AA-BA85-70ABEC650AEE");

            var complexReportTable = dbObjects.AddTable("pc", "ComplexReport", true);
            complexReportTable.AddColumn("Id", EnDbDataType.Uuid);
            complexReportTable.AddColumn("Name");
            complexReportTable.AddColumn("StartDate", EnDbDataType.Timestamp);
            complexReportTable.AddColumn("EndDate", EnDbDataType.Timestamp);
            complexReportTable.AddColumn("CustomerId", EnDbDataType.Uuid, EnSelectType.Inner);
            complexReportTable.AddColumn("ComplexDiagnosticTargetId", EnDbDataType.Uuid);
            complexReportTable.AddColumn("LanguageId", EnDbDataType.Integer, EnSelectType.Inner);
            complexReportTable.AddColumn("Translated", EnDbDataType.Boolean);
            complexReportTable.AddCustomColumn("Owner", "(SELECT \"ShortName\" FROM staff.staff_contractor_contractor as contr WHERE contr.\"Id\" = diagnostictarget.\"CustomerContractorId\")");
            complexReportTable.AddCustomColumn("ContractNumber", "(complexreport.\"ContractInfo\"->>'Number')::text");
            complexReportTable.AddCustomColumn("ContractLength", "(complexreport.\"ContractInfo\"->> 'Length')::double precision");
            complexReportTable.AddCustomColumn("ContractPipelineName", "(complexreport.\"ContractInfo\"->>'PipelineName')::text");
            complexReportTable.AddCustomColumn("ContractRouteName", "(complexreport.\"ContractInfo\"->>'RouteName')::text");
            complexReportTable.AddCustomColumn("CustomerMediator", "(complexreport.\"ContractInfo\"->>'CustomerMediator')::text");
            complexReportTable.AddCustomColumn("OwnerUserOfReport", "(complexreport.\"ContractInfo\"->>'OwnerUserOfReport')::text");
            complexReportTable.AddCustomColumn("ReportAssignment", "(complexreport.\"ContractInfo\"->>'ReportAssignment')::text");
            complexReportTable.AddColumn("PublishDate", EnDbDataType.Timestamp);
            complexReportTable.AddCustomColumn("totalCount", "COUNT(*) OVER()");

            var reportTable = dbObjects.AddTable("diagnostic", "Report");
            reportTable.AddColumn("Id", EnDbDataType.Uuid, EnSelectType.Inner);
            reportTable.AddCustomColumn("Number", "(select dt.value ->> 'Number' as \"Number\" from pc.\"ComplexDiagnosticTarget\" cdt, " +
            "lateral jsonb_array_elements(\"DiagnosticTargets\") dt where cdt.\"Id\" = \"complexreport\".\"ComplexDiagnosticTargetId\" and (dt.value ->> 'Main') :: boolean)");
            reportTable.AddColumn("DiagnosticTargetId", EnDbDataType.Uuid, EnSelectType.Inner);
            reportTable.AddColumn("ReportTypeId", EnDbDataType.Integer, EnSelectType.Inner);

            var diagTargTable = dbObjects.AddTable("diagnostic", "DiagnosticTarget");
            diagTargTable.AddColumn("Id", EnDbDataType.Uuid, EnSelectType.Inner);

            var reportTypeTable = dbObjects.AddTable("diagnostic", "ReportType");
            reportTypeTable.AddColumn("Id", EnDbDataType.Integer, EnSelectType.Inner);
            reportTypeTable.AddColumn("Name" + langMod, "ReportTypeName");

            var innerDiagTargetTable = dbObjects.AddTable("diagnostic", "InnerDiagnosticTarget");
            innerDiagTargetTable.AddColumn("Id", EnDbDataType.Uuid, EnSelectType.Inner);
            innerDiagTargetTable.AddColumn("DiameterId", EnDbDataType.Integer, EnSelectType.Inner);
            innerDiagTargetTable.AddColumn("StartRouteId", EnDbDataType.Uuid, EnSelectType.Inner);

            var routeTable = dbObjects.AddTable("pipeline", "Route");
            routeTable.AddColumn("Id", EnDbDataType.Uuid, EnSelectType.Inner);
            routeTable.AddColumn("BuildDate", EnDbDataType.Timestamp);
            routeTable.AddColumn("MainProductTypeId", EnDbDataType.Integer, EnSelectType.Inner);

            var constructiveDiameterTable = dbObjects.AddTable("dir", "ConstructiveDiameter");
            constructiveDiameterTable.AddColumn("Id", EnDbDataType.Integer, EnSelectType.Inner);
            constructiveDiameterTable.AddColumn("DiameterMm", EnDbDataType.Real);
            constructiveDiameterTable.AddColumn("DiameterInch", EnDbDataType.Real);

            var productTypeTable = dbObjects.AddTable("pipeline", "MainProductType");
            productTypeTable.AddColumn("Id", EnDbDataType.Integer, EnSelectType.Inner);
            productTypeTable.AddColumn("Name" + langMod, "ProductTypeName");

            var languageTable = dbObjects.AddTable("pc", "Language");
            languageTable.AddColumn("Id", EnDbDataType.Integer, EnSelectType.Inner);
            languageTable.AddColumn("ShortName", "LanguageShortName");

            dbObjects.AddJoin("complexreport", "ComplexDiagnosticTargetId", "diagnostictarget", "Id");
            dbObjects.AddJoin("diagnostictarget", "Id", "report", "DiagnosticTargetId");
            dbObjects.AddJoin("report", "ReportTypeId", "reporttype", "Id");
            dbObjects.AddJoin("diagnostictarget", "Id", "innerdiagnostictarget", "Id");
            dbObjects.AddJoin("innerdiagnostictarget", "StartRouteId", "route", "Id");
            dbObjects.AddJoin("innerdiagnostictarget", "DiameterId", "constructivediameter", "Id");
            dbObjects.AddJoin("route", "MainProductTypeId", "mainproducttype", "Id");
            dbObjects.AddJoin("complexreport", "LanguageId", "language", "Id");

            dbObjects.AddFilterColumnMapping("constructivediameter", "DiameterMm", "innerdiagnosticrtarget", "DiameterId");
            dbObjects.AddFilterColumnMapping("mainproducttype", "ProductTypeName", "route", "MainProductTypeId");

            if (EnUserPermission_Translator)
                dbObjects.AddCustomCondition(EnConditionType.And, $"{complexReportTable.Alias}.\"Translated\" is not null");

            dbObjects.AddCustomCondition(EnConditionType.And, "complexreport.\"vrDeleted\" IS false");

            if (!EnUserPermission_Expert && !EnUserPermission_Translator)
            {
                dbObjects.AddCustomCondition(EnConditionType.And, $"{complexReportTable.Alias}.\"CustomerId\" = '{ContractorId}'");
                dbObjects.AddCustomCondition(EnConditionType.And, $"{complexReportTable.Alias}.\"PublishDate\" IS NOT NULL");
            }

            var queryCreator = new QueryCreator(dbObjects);
            var requestParameters = QueryParameters.Empty;

            var queryText = queryCreator.Create(requestParameters);

            Assert.IsTrue(queryText.Contains("ComplexDiagnosticTargetId"));
        }
    }
}

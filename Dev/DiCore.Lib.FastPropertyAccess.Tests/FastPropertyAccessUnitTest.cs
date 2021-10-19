using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using DiCore.Lib.TestModels.Models;
using NUnit.Framework;

namespace DiCore.Lib.FastPropertyAccess.Tests
{
    [TestFixture]
    public class FastPropertyAccessUnitTest
    {
        [Test]
        public void ComplexTypeInstanceTest()
        {
            var props = new Dictionary<string, object>
            {
                {"Id", new Guid("0AD2AA40-9C43-4317-97B3-A93697C57DDB")},
                {"DiagnosticTargetId", new Guid("92A3E850-A982-4EEF-A0E7-A2CEE6B42DDA")},
                {"Distance", 1025.4},
                {"Number", 55},
                {"Length", 10.4},
                {"SectionTypeId", 0},
                {"SectionType#Id", 0},
                {"SectionType#Name", "Секция"},
                {"AverageWallThickness", null},
                {"AxialWeldStartAngle", null},
                {"AxialWeldEndAngle", null},
                {"PipelineSectionId", new Guid("2E4B3A2A-D89F-4F21-BA53-615DD1DCED23")},
                {"PipeTypeId", 0},
                {"PipeType#Id", 0},
                {"PipeType#Name", "Прямошовная"},
                {"Altitude", null},
                {"AxialWeldSecondAngle", null},
                {"Data", String.Empty}
            };
            var fpa = new FastPropertyAccess<Section>(EnPropertyUsingMode.Create);            
            var obj = fpa.CreateObject(props);

            Assert.IsNotNull(obj);
        }

        [Test]
        public void ComplexTypeInstance3Test()
        {
            var props = new Dictionary<string, object>
            {
                {"DiagnosticObject#Id", new Guid("0AD2AA40-9C43-4317-97B3-A93697C57DDB")},
                {"DiagnosticObject#DiagnosticTargetId", new Guid("92A3E850-A982-4EEF-A0E7-A2CEE6B42DDA")},
                {"DiagnosticObject#Distance", 1025.4},
                {"DiagnosticObject#Number", 55},
                {"DiagnosticObject#Length", 10.4},
                {"DiagnosticObject#SectionType#Id", 0},
                {"DiagnosticObject#SectionType#Name", "Секция"},
                {"DiagnosticObject#AverageWallThickness", null},
                {"DiagnosticObject#AxialWeldStartAngle", null},
                {"DiagnosticObject#AxialWeldEndAngle", null},
                {"DiagnosticObject#PipelineSectionId", new Guid("2E4B3A2A-D89F-4F21-BA53-615DD1DCED23")},
                {"DiagnosticObject#PipeType#Id", 0},
                {"DiagnosticObject#PipeType#Name", "Прямошовная"},
                {"DiagnosticObject#Altitude", null},
                {"DiagnosticObject#AxialWeldSecondAngle", null},
                {"DiagnosticObject#Data", String.Empty},
                {"Number", "111"},
                {"BaseObjectId", new Guid("0AD2AA40-9C43-4317-97B3-A93697C57DDB")}
            };
            var fpa = new FastPropertyAccess<Artifact>(EnPropertyUsingMode.Create);            
            var obj = fpa.CreateObject(props);

            Assert.IsNotNull(obj);
        }

        [Test]
        public void ComplexTypeInstanceTest2()
        {
            var props = new Dictionary<string, object>
            {
                {"Id", new Guid("a5c0d502-f734-4720-97e4-eb0c9f0179fa")},
                {"CustomerContractorId", new Guid("69edbb93-2a53-424d-9dc8-f9d9c99468a7")},
                {"StartDate", DateTime.Parse("11.02.2010 0:00:00")},
                {"EndDate", DateTime.Parse("16.02.2010 0:00:00")},
                {"PerformerContractorId", new Guid("da8f15f7-7770-4fb4-ab09-1a14123dd264")},
                {"Comment", ""},
                {"RunCode", 05263},
                {"ReportNumber", "j0193m"},
                {"DiameterId", 17},
                {"Diameter#Id", 17},
                {"Diameter#DiameterMm", 720},
                {"Diameter#DiameterInch", 28},
                {"Diameter#Marking", "720 мм"},
                {"Diameter#IsGOST", true},
                {"Diameter#DiameterForCalc", 700},
                {"StartRouteId", new Guid("6ad65091-c64a-4860-ae1f-f29246df817f")},
                {"StartRouteDistance", 1.729},
                {"EndRouteId", new Guid("6ad65091-c64a-4860-ae1f-f29246df817f")},
                {"EndRouteDistance", 7752.15},
                {"PigId", new Guid("b4140db2-7292-4c1a-b684-22d94c9c6776")},
                {"DiagnosticMethod#Id", 7},
                {"DiagnosticMethod#Name", "MFL"},
                {"DiagnosticMethod#Description", "Магнитный дефектоскоп MFL"},
                {"DiagnosticMethod#DiagnosticTypeId", 1}
            };
            var fpa = new FastPropertyAccess<DiagnosticTarget>(EnPropertyUsingMode.Create);            
            var obj = fpa.CreateObject(props);

            Assert.IsNotNull(obj);
        }
    }
}

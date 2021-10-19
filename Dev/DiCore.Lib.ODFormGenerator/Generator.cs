using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aspose.Cells;

namespace DiCore.Lib.ODFormGenerator
{
    public class Generator
    {

        public void Generate(Stream stream, string sheetName = "Оборудование", List<Directory> directories = null,
            List<GeneratorParameter> parameters = null)
        {
            var workBook = new Workbook();
            workBook.Worksheets.CustomDocumentProperties.Add("version", 1);
            var sheet = workBook.Worksheets[0];
            sheet.Name = sheetName;
            sheet.PreFill();
            if (directories != null)
            {
                foreach (var directory in directories)
                {
                    workBook.AddDirectory(directory);
                }
            }
            if (parameters != null)
            {
                sheet.FillParameters(parameters);
            }
            sheet.FillEmptyData();
            workBook.Save(stream, SaveFormat.Xlsx);

        }

        public void AddSheet(Stream stream, string sheetName, List<GeneratorParameter> parameters = null)
        {
            var workbook = new Workbook(stream);
            var sheet = workbook.Worksheets.Add(sheetName);
            sheet.PreFill();
            if (parameters != null)
            {
                sheet.FillParameters(parameters);
            }
            workbook.Save(stream, SaveFormat.Xlsx);
        }
    }
}

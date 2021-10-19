using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aspose.Cells;

namespace DiCore.Lib.ODFormGenerator
{

    internal static class ExcelHelper
    {
        private const int MaxColumnWidth = 100;
        private const int DataRowOffset = 7;
        
        public static void PreFill(this Worksheet sheet)
        {
            var style = sheet.Cells["A1"].GetStyle(true);
            style.IsTextWrapped = true;
            style.Font.IsBold = true;
            style.SetBorder(BorderType.BottomBorder, CellBorderType.Thin, Color.Black);
            style.SetBorder(BorderType.TopBorder, CellBorderType.Thin, Color.Black);
            style.SetBorder(BorderType.LeftBorder, CellBorderType.Thin, Color.Black);
            style.SetBorder(BorderType.RightBorder, CellBorderType.Thin, Color.Black);
            style.HorizontalAlignment = TextAlignmentType.Left;
            for (var i = 0; i < 7; i++)
            {
                sheet.Cells[i, 0].SetStyle(style);
            }
            //sheet.Cells.Columns[0].ApplyStyle(style, new StyleFlag() {All = true});
            sheet.Cells["A1"].PutValue(Labels.ParameterNumber);
            sheet.Cells["A2"].PutValue(Labels.ParameterId);
            sheet.Cells["A3"].PutValue(Labels.ParameterName);
            sheet.Cells["A4"].PutValue(Labels.ParameterFormat);
            sheet.Cells["A5"].PutValue(Labels.ParameterUnit);
            sheet.Cells["A6"].PutValue(Labels.ParameterRange);
            sheet.Cells["A7"].PutValue(Labels.ParameterComment);

            sheet.Cells.SetColumnWidth(0, 30);
        }

        public static void FillParameters(this Worksheet sheet, List<GeneratorParameter> parameters,
            IEnumerable<Directory> directories = null)
        {
            //var style = sheet.Cells["A1"].GetStyle(true);
            var style = sheet.Cells["B2"].GetStyle();
            style.IsTextWrapped = true;
            style.SetBorder(BorderType.BottomBorder, CellBorderType.Thin, Color.Black);
            style.SetBorder(BorderType.TopBorder, CellBorderType.Thin, Color.Black);
            style.SetBorder(BorderType.LeftBorder, CellBorderType.Thin, Color.Black);
            style.SetBorder(BorderType.RightBorder, CellBorderType.Thin, Color.Black);
            var headerStyle = sheet.Cells["B1"].GetStyle();
            headerStyle.SetBorder(BorderType.BottomBorder, CellBorderType.Thin, Color.Black);
            headerStyle.SetBorder(BorderType.TopBorder, CellBorderType.Thin, Color.Black);
            headerStyle.SetBorder(BorderType.LeftBorder, CellBorderType.Thin, Color.Black);
            headerStyle.SetBorder(BorderType.RightBorder, CellBorderType.Thin, Color.Black);
            headerStyle.HorizontalAlignment = TextAlignmentType.Center;
            headerStyle.Font.IsBold = true;
            for (var i = 0; i < parameters.Count; i++)
            {
                sheet.Cells[0, i + 1].PutValue(i + 1);
                sheet.Cells[1, i + 1].PutValue(parameters[i].Id);
                sheet.Cells[2, i + 1].PutValue(parameters[i].Name);
                sheet.Cells[3, i + 1].PutValue(parameters[i].Format);
                sheet.Cells[4, i + 1].PutValue(parameters[i].Unit);
                sheet.Cells[5, i + 1].PutValue(parameters[i].Range);
                sheet.Cells[6, i + 1].PutValue(parameters[i].Comment);

                sheet.Cells[0, i + 1].SetStyle(headerStyle);

                for (var j = 1; j < 7; j++)
                {
                    sheet.Cells[j, i + 1].SetStyle(style);
                }
                sheet.AutoFitColumn(i + 1);

                if (sheet.Cells.GetColumnWidth(i + 1) > MaxColumnWidth)
                {
                    sheet.Cells.SetColumnWidth(i + 1, MaxColumnWidth);
                }
                if (!string.IsNullOrWhiteSpace(parameters[i].DirectoryName))
                {
                    var validation = sheet.Validations[sheet.Validations.Add()];
                    validation.Type=ValidationType.List;
                    validation.Operator=OperatorType.None;
                    validation.InCellDropDown = true;
                    validation.Formula1 = string.Format("={0}", parameters[i].DirectoryName);
                    validation.ShowError = true;
                    validation.AlertStyle = ValidationAlertType.Stop;
                    validation.ErrorTitle = "Ошибка";
                    validation.ErrorMessage = "Выберите значение из списка";
                    validation.AddArea(new CellArea()
                    {
                        StartColumn = i+1,
                        StartRow = 7,
                        EndColumn = i+1,
                        EndRow = 1000
                    });
                }

            }
        }

        public static void FillEmptyData(this Worksheet sheet)
        {
            var headerStyle = sheet.Cells["A1"].GetStyle();
            var style = sheet.Cells["B2"].GetStyle();
            for (var i = 0; i < 5; i++)
            {
                if (i < 3)
                {
                    sheet.Cells[DataRowOffset + i, 0].PutValue(i + 1);
                }
                else if (i == 3)
                {
                    sheet.Cells[DataRowOffset + i, 0].PutValue("...");
                }
                else
                {
                    sheet.Cells[DataRowOffset + i, 0].PutValue("M");
                }
                sheet.Cells[DataRowOffset+i,0].SetStyle(headerStyle);
                for (var j = 1; j <= sheet.Cells.MaxColumn; j++)
                {
                    sheet.Cells[DataRowOffset + i, j].SetStyle(style);
                }
            }
        }

        public static void AddDirectory(this Workbook workbook, Directory directory)
        {
            var sheet = workbook.Worksheets.Add(directory.Name);
            if (directory.Items == null) return;
            var row = 0;
            foreach (var item in directory.Items)
            {
                sheet.Cells[row, 0].PutValue(item.Key);
                sheet.Cells[row, 1].PutValue(item.Value);
                row++;
            }
            
            sheet.AutoFitColumn(0);
            sheet.AutoFitColumn(1);

            var range = sheet.Cells.CreateRange(0, 1, directory.Items.Count > 0 ? directory.Items.Count : 1, 1);
            range.Name = directory.Name;
            sheet.VisibilityType = VisibilityType.VeryHidden;

        }
    }
}

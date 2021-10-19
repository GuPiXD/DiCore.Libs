using System.Reflection;
using Aspose.Cells;
using DiCore.Lib.Xls.Types.Attributes;

namespace DiCore.Lib.Xls.Types.Helpers
{
    public static class InputFormCellsHelper
    {
        /// <summary>
        /// Вычисление строки, являющейся последней строкой с данными
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="startTableRow"></param>
        /// <param name="columnRange"></param>
        /// <returns></returns>
        public static int GetEndTableRow(Worksheet sheet, int startTableRow, ColumnRangeAttribute columnRange, int startTableColumn)
        {
            var i = startTableRow;
            if (columnRange == null)
                while (sheet.Cells[i, startTableColumn].Type != CellValueType.IsNull)
                    i++;
            else
            {
                while (CheckRowForData(sheet, i, columnRange))
                    i++;
            }

            return i;
        }

        public static bool CheckRowForData(Worksheet sheet, int rowIndex, ColumnRangeAttribute columnRange)
        {
            if (columnRange.Max.Length == 1)
            {
                for (char column = columnRange.Min[0]; column <= columnRange.Max[0]; column++)
                {
                    if (sheet.Cells[column.ToString() + rowIndex].Type != CellValueType.IsNull)
                        return true;
                }
            }
            else
            {
                for (char ch1 = 'A'; ch1 <= columnRange.Max[0]; ch1++)
                {
                    for (char ch2 = columnRange.Min[0]; ch2 <= columnRange.Max[1]; ch2++)
                    {
                        if (sheet.Cells[ch2.ToString() + rowIndex].Type != CellValueType.IsNull)
                            return true;
                    }
                }

            }
            return false;
        }

        public static bool CheckCellValueForNull(Cell cell, bool nullable)
        {
            if (!nullable && cell.Type == CellValueType.IsNull)
                return false;
            return true;
        }

        public static bool CheckCellValueForNull(Cell cell, PropertyInfo propInfo)
        {
            if (!AllowNullAttribute.Exist(propInfo) &&
                cell.Type == CellValueType.IsNull)
            {
                return false;
            }
            return true;
        }
    }
}

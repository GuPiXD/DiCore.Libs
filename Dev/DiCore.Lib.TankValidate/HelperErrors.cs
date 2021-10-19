using System;
using System.Drawing;
using System.IO;
using Aspose.Cells;

namespace DiCore.Lib.TankValidate
{
    internal static class HelperErrors
    {
        /// <summary>
        /// Сохранить спиок ошибок в Excel
        /// </summary>
        /// <returns></returns>
        internal static void ExcelErrorsSave(ReportRecord[] errors, string path)
        {
            var workbookErrors = GetExcelErrors(errors);

            workbookErrors.Save(path);
        }

        /// <summary>
        /// Получить спиок ошибок в Excel
        /// </summary>
        /// <returns></returns>
        internal static MemoryStream GetExcelErrorsMemoryStream(ReportRecord[] errors)
        {
            var workbookErrors = GetExcelErrors(errors);

            var workbookStream = new MemoryStream();
            workbookErrors.Save(workbookStream, Aspose.Cells.SaveFormat.Xlsx);
            workbookStream.Position = 0;
            return workbookStream;
        }


        /// <summary>
        /// Сохранить входной файл с отмеченными ошибками
        /// </summary>
        /// <returns></returns>
        internal static void InputExcelAppendErrorsSave(ReportRecord[] errors, string path, Workbook workbook)
        {
            var workbookErrors = InputExcelAppendErrors(errors, workbook);

            workbookErrors.Save(path);
        }

        /// <summary>
        /// Получить входной файл с отмеченными ошибками
        /// </summary>
        /// <returns></returns>
        internal static MemoryStream GetInputExcelAppendErrorsMemoryStream(ReportRecord[] errors, Workbook workbook)
        {
            var workbookErrors = InputExcelAppendErrors(errors, workbook);

            var workbookStream = new MemoryStream();
            workbookErrors.Save(workbookStream, Aspose.Cells.SaveFormat.Xlsx);
            workbookStream.Position = 0;
            return workbookStream;
        }


        private static Workbook GetExcelErrors(ReportRecord[] errors)
        {
            var workbookErrors = new Workbook();
            var worksheet = workbookErrors.Worksheets[0];

            worksheet.Name = "Ошибки";

            var cells = worksheet.Cells;

            cells[0, 0].Value = "№пп";
            cells[0, 1].Value = "Тип";
            cells[0, 2].Value = "Лист";
            cells[0, 3].Value = "Строка";
            cells[0, 4].Value = "Столбец";
            cells[0, 5].Value = "Значение";
            cells[0, 6].Value = "№ дефекта";
            cells[0, 7].Value = "Описание";

            cells.SetColumnWidthPixel(0, 50); //"№пп";
            cells.SetColumnWidthPixel(1, 180); //"Тип"
            cells.SetColumnWidthPixel(2, 300); //"Лист"
            cells.SetColumnWidthPixel(3, 70); //"Строка"
            cells.SetColumnWidthPixel(4, 80); //"Столбец"
            cells.SetColumnWidthPixel(5, 400); //"Значение"
            cells.SetColumnWidthPixel(6, 100); //"№ дефекта"
            cells.SetColumnWidthPixel(7, 900); //"Описание"


            for (int i = 0, j = 0; i < errors.Length; i++)
            {
                cells[j + 1, 0].Value = (j + 1).ToString();
                cells[j + 1, 1].Value = ((EnErrorType)((ReportRecord)errors[i]).Type).ToText();
                cells[j + 1, 2].Value = ((ReportRecord)errors[i]).Sheet;
                cells[j + 1, 3].Value = ((ReportRecord)errors[i]).Row;
                cells[j + 1, 4].Value = sColIndexToColName(((ReportRecord)errors[i]).Column - 1);
                cells[j + 1, 5].Value = ((ReportRecord)errors[i]).Value;
                cells[j + 1, 6].Value = ((ReportRecord)errors[i]).Defect;
                cells[j + 1, 7].Value = ((ReportRecord)errors[i]).Text;
                j++;
            }

            return workbookErrors;
        }


        private static Workbook InputExcelAppendErrors(ReportRecord[] errors, Workbook workbook)
        {
            for (int i = 0, j = 0; i < errors.Length; i++)
            {
                AddError(workbook, errors[i]);
            }

            return workbook;
        }

        private static void AddError(Workbook workbook, ReportRecord error)
        {
            var worksheet = workbook.Worksheets[error.Sheet];

            var cell = worksheet.Cells[error.Row - 1, error.Column - 1];

            var commentIndex = worksheet.Comments.Add(cell.Name);
            var comment = worksheet.Comments[commentIndex];
            comment.HtmlNote =
                 $"<Font Style=\"FONT-WEIGHT: bold;FONT-FAMILY: Times New Roman;FONT-SIZE: 10pt;COLOR: #000000;TEXT-ALIGN: left\">{error.Text}</ Font > \n";


            comment.AutoSize = true;
            //comment.IsVisible = true;

            var style = cell.GetStyle();
            /*var style = new Style();
            style.Font.Name = "Franklin Gothic Book";
            style.HorizontalAlignment = TextAlignmentType.Center;
            style.VerticalAlignment = TextAlignmentType.Center;
*/
            style.ForegroundColor = Color.Red;
            style.Pattern = BackgroundType.Solid;
            cell.SetStyle(style, true);
        }

        #region Наименование колонки Excel

        /// <summary>
        /// Преобразование индекса столбца в его название. При этом проверяется, 
        /// чтобы индекс столбца входил в диапазон допустимых значений [0, 255].
        /// </summary>
        /// <param name="pnColIndex">Индекс столбца</param>
        /// <returns>Название столбца</returns>
        private static string sColIndexToColName(int pnColIndex)
        {
            try
            {
                // является ли входное число индексом столбца
                if (!bIsColIndex(pnColIndex)) return "";

                // номер столбца - это его индекс + 1
                int lnDivision = 0;
                //int lnResult = Math.DivRem(pnColIndex + 1, 27, out lnDivision);
                int lnResult = Math.DivRem(pnColIndex, 26, out lnDivision); // даже если к индексу прибавить 1 - в алфавите всё равно 26 букв ... A - Z :-)

                // переводим номер столбца в имя с использованием кода символа (код 'А' = 65)
                if (lnResult == 0)
                    //return string.Format("{0}", (char)(lnDivision + 64));
                    return string.Format("{0}", (char)(lnDivision + 65));
                else
                    return string.Format("{0}", (char)(lnResult + 64)) + string.Format("{0}", (char)(lnDivision + 65));
            }
            catch { return ""; }

        }   // sColIndexToColName

        //=========================================================================================
        /// <summary>
        /// Является ли число индексом столбца Excel, то есть входит ли индекс столбца в диапазон 
        /// допустимых значений [0, EXCEL_MAX_COL_AMOUNT] (индекс столбца на 1 меньше его номера).
        /// </summary>
        /// <param name="pnColIndex">Индекс столбца</param>
        /// <returns>True, если входит, false в противном случае</returns>
        private static bool bIsColIndex(int pnColIndex)
        {
            // максимальное количество столбцов в Excel
            const int EXCEL_MAX_COL_AMOUNT = 256;

            return (bIsBetween(pnColIndex, 0, EXCEL_MAX_COL_AMOUNT - 1));

        }   // bIsColIndex


        //=====================================================================
        /// <summary>
        /// Проверить, входит ли целое число в диапазон, включая границы.
        /// Если в значении диапазона стоит null, то ограничения с этой стороны нет.
        /// </summary>
        /// <param name="pnValue">Исходное число</param>
        /// <param name="pnLowerBound">Нижняя граница диапазона</param>
        /// <param name="pnUpperBound">Верхняя граница диапазона</param>
        /// <returns>true, если число входит в диапазон, false в противном случае</returns>
        private static bool bIsBetween(long pnValue, long? pnLeftBorder, long? pnRightBorder)
        {
            // записаны обе границы
            if ((pnLeftBorder != null) && (pnRightBorder != null))
                if ((pnLeftBorder <= pnValue) && (pnRightBorder >= pnValue))
                    return true;

            // записана левая граница диапазона
            if ((pnLeftBorder != null) && (pnRightBorder == null))
                if (pnLeftBorder <= pnValue)
                    return true;

            // записана правая граница диапазона
            if ((pnLeftBorder == null) && (pnRightBorder != null))
                if (pnRightBorder >= pnValue)
                    return true;

            // обе границы не записаны
            if ((pnLeftBorder == null) && (pnRightBorder == null))
                return true;

            return false;
        }

        #endregion Наименование колонки Excel

    }
}
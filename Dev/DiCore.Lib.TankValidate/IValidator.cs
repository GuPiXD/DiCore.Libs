using System.IO;
using Aspose.Cells;

namespace DiCore.Lib.TankValidate
{
    public interface IValidator
    { 
        /// <summary>
        /// Подгрузить входные данные
        /// </summary>
        /// <returns>Ошибки загрузки входных файлов</returns>
        SyntaxError[] LoadFile(MemoryStream fileXml, MemoryStream fileExcel);

        /// <summary>
        /// Выполнить проверку файла
        /// </summary>
        /// <returns>Найденные ошибки во входной форме</returns>
        ReportRecord[] Execute();



        /// <summary>
        /// Получить спиок ошибок в Excel
        /// </summary>
        /// <returns></returns>
        MemoryStream GetExcelErrorsMemoryStream(ReportRecord[] errors);

        /// <summary>
        /// Сохранить спиок ошибок в Excel
        /// </summary>
        /// <returns></returns>
        void ExcelErrorsSave(ReportRecord[] errors, string path);


        /// <summary>
        /// Получить входной файл с отмеченными ошибками
        /// </summary>
        /// <returns></returns>
        MemoryStream GetInputExcelAppendErrorsMemoryStream(ReportRecord[] errors);
        //MemoryStream GetInputExcelAppendErrorsMemoryStream(ReportRecord[] errors, MemoryStream fileExcel);

        /// <summary>
        /// Сохранить входной файл с отмеченными ошибками
        /// </summary>
        /// <returns></returns>
        void InputExcelAppendErrorsSave(ReportRecord[] errors, string path);

    }
}
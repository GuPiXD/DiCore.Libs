namespace DiCore.Lib.Xls.Types
{
    public class InputFormError
    {
        public InputFormError(string sheet, string tableNumber, string cell, string message)
        {
            Sheet = sheet;
            Cell = cell;
            Message = message;
            TableNumber = tableNumber;
        }

        public InputFormError(string sheet, string cell, string message)
            : this(sheet, null, cell, message)
        {

        }

        public InputFormError(string sheet, string message)
            : this(sheet, null, message)
        {

        }

        /// <summary>
        /// Лист в котором обнаружена ошибка
        /// </summary>
        public string Sheet { get; set; }

        /// <summary>
        /// № таблицы
        /// </summary>
        public string TableNumber { get; set; }

        /// <summary>
        /// Ячейка в которой обнаружена ошибка
        /// </summary>
        public string Cell { get; set; }

        /// <summary>
        /// Сообщение об ошибке
        /// </summary>
        public string Message { get; set; }
    }
}

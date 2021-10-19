using System;

namespace DiCore.Lib.TankValidate
{
    public enum EnErrorType
    {
        ErrorData = 0,
        Error = 1,
        Warrning = 2
    }
    

    public static class ErrorTypeConvert
    {
        public static string ToText(this EnErrorType errorType)
        {
            switch (errorType)
            {
                case EnErrorType.ErrorData:
                    return "Ошибка данных";
                case EnErrorType.Error:
                    return "Ошибка";
                case EnErrorType.Warrning:
                    return "Предупреждение";
            }
            throw new ArgumentException($"Не задано преобразование значения {errorType} типа EnErrorType в строку");
        }
    }
}
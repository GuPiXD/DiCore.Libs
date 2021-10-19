using System.Collections.Generic;

namespace DiCore.Lib.Xls.Types.Interfaces
{
    public interface IXlsLoader<T>
    {
        /// <summary>
        /// Загрузка данных из входного файла
        /// </summary>
        /// <returns>Список ошибок</returns>
        bool Load();

        /// <summary>
        /// Получение класса с данными
        /// </summary>
        /// <returns></returns>
        T GetResult();

        /// <summary>
        /// Получение списка ошибок </summary>
        /// <returns></returns>
        List<InputFormError> GetErrors();

        /// <summary>
        /// Имя входного файла
        /// </summary>
        string FileName { get; }

        /// <summary>
        /// Ошибки во входной форме
        /// </summary>
        List<InputFormError> Errors { get; }

        /// <summary>
        /// Класс с данными
        /// </summary>
        T DataObject { get; }
    }
}
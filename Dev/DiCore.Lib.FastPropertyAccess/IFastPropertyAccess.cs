using System.Collections.Generic;

namespace DiCore.Lib.FastPropertyAccess
{
    public interface IFastPropertyAccess<T>
    {
        /// <summary>
        /// Создание объекта
        /// </summary>
        /// <param name="props"></param>
        /// <returns>Экземпляр объекта</returns>
        T CreateObject(Dictionary<string, object> props);

        /// <summary>
        /// Получение значения свойства объекта
        /// </summary>
        /// <param name="instance">Экземпляр объекта</param>
        /// <param name="propertyName">Наименование свойства</param>
        /// <returns>Значение свойства</returns>
        object Get(T instance, string propertyName);

        /// <summary>
        /// Присвоение значения свойству
        /// </summary>
        /// <param name="instance">Экземпляр объекта</param>
        /// <param name="propertyName">Наименование свойства</param>
        /// <param name="value">Значение</param>
        /// <returns>Результат выполнения операции</returns>
        void Set(T instance, string propertyName, object value);

        /// <summary>
        /// Получение списка имен свойств
        /// </summary>
        IEnumerable<string> PropertyNames { get; }
    }
}
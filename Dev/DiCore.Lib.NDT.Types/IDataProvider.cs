using System;

namespace DiCore.Lib.NDT.Types
{
    /// <summary>
    /// Базовый интерфейс провайдеров данных  
    /// </summary>
    public interface IDataProvider: IDisposable
    {
        /// <summary>
        /// Наименование сервиса
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Описание сервиса
        /// </summary>
        string Description { get; }
        /// <summary>
        /// Разработчик сервиса
        /// </summary>
        string Vendor { get; }
        /// <summary>
        /// Дата провайдер открыт
        /// </summary>
        bool IsOpened { get; }
        /// <summary>
        /// Смещение данных с составе дефектоскопа, м
        /// </summary>
        double SectionOffset { get; }

        /// <summary>
        /// Максимальный скан в данных данного типа
        /// </summary>
        int MaxScan { get; }

        /// <summary>
        /// Минимальный скан в данных данного типа
        /// </summary>
        int MinScan { get; }

        short SensorCount { get; }

        /// <summary>
        /// Открытие диагностической сессии
        /// </summary>
        /// <param name="location">Параметры размещения данных</param>
        /// <returns></returns>
        bool Open(DataLocation location);
        /// <summary>
        /// Закрытие диагностической сессии
        /// </summary>
        void Close();
    }
}

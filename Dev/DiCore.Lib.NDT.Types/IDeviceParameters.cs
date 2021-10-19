namespace DiCore.Lib.NDT.Types
{
    /// <summary>
    /// Параметры дефектоскопа
    /// </summary>
    public interface IDeviceParameters
    {
        /// <summary>
        /// Идентификатор носителя датчиков 
        /// </summary>
        int CarrierId { get; set; }

        /// <summary>
        /// Смещение нулевого датчика относительно нулевой точки дефектоскопа вдоль оси прибора, мм
        /// </summary>
        double DeltaX { get; set; }

        /// <summary>
        /// Номер датчика в ВМТ 
        /// </summary>
        int SensorTDC { get; set; }

        bool IsReverse { get; set; }

        float PipeDiameterMm { get; set; }

        /// <summary>
        /// Одометрический фактор, м
        /// </summary>
        double OdoFactor { get; set; }

        /// <summary>
        /// Формирование скана
        /// </summary>
        bool ScanFactor { get; set; }

        /// <summary>
        ///  Длина окружности трубы, мм (внешняя)
        /// </summary>
        float PipeCircle { get; }
    }
}

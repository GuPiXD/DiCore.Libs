using System.Runtime.InteropServices;

namespace DiCore.Lib.NDT.Types
{
    /// <summary>
    /// Структура содержит параметры выравнивания для датчика
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct SensorAligningItem
    {
        /// <summary>
        /// Смещения датчика в мм от нулевой отметки поперек движения носителя
        /// </summary>
        [FieldOffset(0)]
        public float DeltaX;
        /// <summary>
        /// Смещения датчика в мм от нулевой отметки вдоль движения носителя
        /// </summary>
        [FieldOffset(4)]
        public float DeltaY;
        /// <summary>
        /// Задержка опроса в группе последовательно опрашиваемых датчиков
        /// </summary>
        [FieldOffset(8)]
        public int TimeDelay;
        /// <summary>
        /// Смещение до скана содержащего показание данного датчика для построения выравненного скана
        /// </summary>
        [FieldOffset(12)]
        public short DeltaScan;
        /// <summary>
        /// Смещение до скана содержащего показание данного датчика для построения выравненного скана (с учетом доп. смещения)
        /// </summary>
        [FieldOffset(14)]
        public short AdditionalAligmentInputInDeltaScan;
        /// <summary>
        /// Индекс полосы на трубе
        /// </summary>
        [FieldOffset(16)]
        public short IndexOnRing;
        /// <summary>
        /// Направление луча
        /// </summary>
        [FieldOffset(18)]
        public byte SensorRayDirection;
        /// <summary>
        /// Номер оппозитного датчика
        /// </summary>
        [FieldOffset(19)]
        public short OpposedSensorNumber;
    }
}

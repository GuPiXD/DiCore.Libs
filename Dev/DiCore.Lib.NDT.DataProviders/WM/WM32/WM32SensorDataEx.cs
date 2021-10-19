using System.Runtime.InteropServices;

namespace DiCore.Lib.NDT.DataProviders.WM.WM32
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct WM32SensorDataEx
    {
        /// <summary>
        /// Количество эхо сигналов
        /// </summary>     
        public byte Count;
        /// <summary>
        /// Время входного эха, мкс
        /// </summary>  
        public float InputTime;
        /// <summary>
        /// Эхо сигналы
        /// </summary>
        public WM32Echo* Echos;
    }
}
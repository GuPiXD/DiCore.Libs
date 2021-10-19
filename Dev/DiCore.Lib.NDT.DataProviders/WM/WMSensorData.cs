using System.Runtime.InteropServices;
using DiCore.Lib.NDT.DataProviders.WM.WM32;

namespace DiCore.Lib.NDT.DataProviders.WM
{
    [StructLayout(LayoutKind.Explicit, Pack = 4)]
    public struct WMSensorData
    {
        [FieldOffset(0)] public float SO;
        [FieldOffset(4)] public float WT;
      //  [FieldOffset(8)] public float AW;
     //   [FieldOffset(12)] public float AW2;

        public static uint Size = (uint)Marshal.SizeOf<WMSensorData>();
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct WM32SafeSensorDataEx
    {
        /// <summary>
        /// Время входного эха, мкс
        /// </summary>  
        public float InputTime;
        /// <summary>
        /// Эхо сигналы
        /// </summary>
        public WM32Echo[] Echos;
    }
}

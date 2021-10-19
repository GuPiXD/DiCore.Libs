using System.Runtime.InteropServices;

namespace DiCore.Lib.NDT.DataProviders.CDL
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct CDSensorDataEx
    {
        /// <summary>
        /// Количество эхо сигналов
        /// </summary>     
        public byte Count;
        /// <summary>
        /// Эхо сигналы
        /// </summary>
        public CDEcho* Echos;

        public static uint Size = (uint)Marshal.SizeOf<CDSensorDataEx>();
    }
}

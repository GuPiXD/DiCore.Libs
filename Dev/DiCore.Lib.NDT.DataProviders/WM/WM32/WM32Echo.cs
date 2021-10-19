using System.Runtime.InteropServices;

namespace DiCore.Lib.NDT.DataProviders.WM.WM32
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct WM32Echo
    {
        /// <summary>
        /// Амплитуда эхо сигнала, dB
        /// </summary>   
        public float Amplitude;
        /// <summary>
        /// Время прихода эхо сигнала, мкс
        /// </summary>
        public float Time;

        public static readonly WM32Echo Empty;
    }
}
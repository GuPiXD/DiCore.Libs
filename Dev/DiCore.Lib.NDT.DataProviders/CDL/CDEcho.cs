using System;
using System.Runtime.InteropServices;

namespace DiCore.Lib.NDT.DataProviders.CDL
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct CDEcho
    {
        /// <summary>
        /// Амплитуда эхо сигнала, dB
        /// </summary>   
        public float Amplitude;
        /// <summary>
        /// Время прихода эхо сигнала, мкс
        /// </summary>
        public float Time;

        public static uint Size = (uint)Marshal.SizeOf<CDEcho>();

        public static readonly CDEcho Empty;

        public override string ToString()
        {
            return $"{Time:F2}mcs. {Amplitude}dB";
        }
    }
}

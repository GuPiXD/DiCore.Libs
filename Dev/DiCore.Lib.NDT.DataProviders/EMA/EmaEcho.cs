
namespace DiCore.Lib.NDT.DataProviders.EMA
{
    public struct EmaEcho
    {
        /// <summary>
        /// Амплитуда эхо сигнала, dB
        /// </summary>   
        public float Amplitude;
        /// <summary>
        /// Время прихода эхо сигнала, мкс
        /// </summary>
        public float Time;

        public static readonly EmaEcho Empty;
    }
}

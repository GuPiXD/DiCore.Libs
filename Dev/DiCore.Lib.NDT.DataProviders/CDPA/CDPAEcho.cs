namespace DiCore.Lib.NDT.DataProviders.CDPA
{
    public struct CDPAEcho
    {
        /// <summary>
        /// Амплитуда эхо сигнала, dB
        /// </summary>   
        public float Amplitude;
        /// <summary>
        /// Время прихода эхо сигнала, мкс
        /// </summary>
        public float Time;

        public static readonly CDPAEcho Empty;
    }
}
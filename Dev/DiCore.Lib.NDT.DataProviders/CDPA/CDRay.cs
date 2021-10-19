using DiCore.Lib.NDT.DataProviders.CDL;

namespace DiCore.Lib.NDT.DataProviders.CDPA
{
    public unsafe struct CDRay
    {
        /// <summary>
        /// Индекс луча
        /// </summary>     
        public ushort RayId;
        /// <summary>
        /// Количество эхо сигналов
        /// </summary>     
        public ushort EchoCount;
        /// <summary>
        /// Эхо сигналы
        /// </summary>
        public CDEcho* Echos;
    }
}
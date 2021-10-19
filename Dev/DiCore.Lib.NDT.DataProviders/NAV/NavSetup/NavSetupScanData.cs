using System.Runtime.InteropServices;

namespace DiCore.Lib.NDT.DataProviders.NAV.NavSetup
{
    [StructLayout(LayoutKind.Explicit, Pack = 4)]
    public struct NavSetupScanData
    {
        /// <summary>
        /// Проекция линейного ускорения на боковую ось X, 0,00001 м/с2
        /// </summary>
        [FieldOffset(0)]
        public float LinearAccelProjX;
        /// <summary>
        /// Проекция линейного ускорения на продольную ось Y, 0,00001 м/с2
        /// </summary>
        [FieldOffset(4)]
        public float LinearAccelProjY;
        /// <summary>
        /// Проекция линейного ускорения на вертикальную ось Z, 0,00001 м/с2
        /// </summary>
        [FieldOffset(8)]
        public float LinearAccelProjZ;
        /// <summary>
        /// Угловая скорость по боковому гироскопу X, 0,00001 рад/с
        /// </summary>
        [FieldOffset(12)]
        public float AngularSpeedX;
        /// <summary>
        /// Угловая скорость по продольному гироскопу Y, 0,00001 рад/с
        /// </summary>
        [FieldOffset(16)]
        public float AngularSpeedY;
        /// <summary>
        /// Угловая скорость по вертикальному гироскопу Z, 0,00001 рад/с
        /// </summary>
        [FieldOffset(20)]
        public float AngularSpeedZ;
        /// <summary>
        /// Угол крена по акселерометрам, 0,01 град
        /// </summary>
        [FieldOffset(24)]
        public float RollAngleAccel;
        /// <summary>
        /// Угол крена по гироскопам, 0,01 град
        /// </summary>
        [FieldOffset(28)]
        public float RollAngleGyro;
        /// <summary>
        /// Угол тангажа по акселерометрам, 0,01 град
        /// </summary>
        [FieldOffset(32)]
        public float PitchAngleAccel;
        /// <summary>
        /// Угол тангажа по гироскопам, 0,01 град
        /// </summary>
        [FieldOffset(36)]
        public float PitchAngleGyro;
        /// <summary>
        /// Условное время прихода пакетов, 0,001 с
        /// </summary>
        [FieldOffset(40)]
        public float PacketArrivalTime;

        public static readonly NavSetupScanData Empty;
    }
}

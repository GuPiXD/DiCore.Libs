using System.Runtime.InteropServices;

namespace DiCore.Lib.NDT.DataProviders.NAV
{
    [StructLayout(LayoutKind.Explicit, Pack = 4)]
    public struct NavScanData
    {
        /// <summary>
        /// Линейная скорость, м/с
        /// </summary>
        [FieldOffset(0)]
        public float LinearSpeed;
        /// <summary>
        /// Угол крена, град
        /// </summary>
        [FieldOffset(4)]
        public float RollAngle;
        /// <summary>
        /// Тангаж, град
        /// </summary>
        [FieldOffset(8)]
        public float ElevationAngle;
        /// <summary>
        /// Азимут, град
        /// </summary>
        [FieldOffset(12)]
        public float AzimuthAngle;
        /// <summary>
        /// Кривизна в верт. плоскости, 100000/м
        /// </summary>
        [FieldOffset(16)]
        public float VerticalArch;
        /// <summary>
        /// Кривизна в гор. плоскости, 100000/м
        /// </summary>
        [FieldOffset(20)]
        public float HorizontalArch;
        /// <summary>
        /// Модуль кривизны, 100000/м
        /// </summary>
        [FieldOffset(24)]
        public float AbsoluteArch;
        /// <summary>
        /// Направление изгиба, град
        /// </summary>
        [FieldOffset(28)]
        public float ArchDirection;
        /// <summary>
        /// Координата север, м
        /// </summary>
        [FieldOffset(32)]
        public double X;
        /// <summary>
        /// Координата восток, м
        /// </summary>
        [FieldOffset(40)]
        public double Y;
        /// <summary>
        /// Координата высота, м
        /// </summary>
        [FieldOffset(48)]
        public double Z;
        /// <summary>
        /// Геодезическая широта, секунды
        /// </summary>
        [FieldOffset(56)]
        public double GeodeticLatitude;
        /// <summary>
        /// Геодезическая долгота, секунды
        /// </summary>
        [FieldOffset(64)]
        public double GeodeticLongitude;
        /// <summary>
        /// Кривизна в верт. плоскости (малые радиусы), 100000/м
        /// </summary>
        [FieldOffset(72)]
        public float VerticalArchSmall;
        /// <summary>
        /// Кривизна в гор. плоскости (малые радиусы), 100000/м
        /// </summary>
        [FieldOffset(76)]
        public float HorizontalArchSmall;
        /// <summary>
        /// Кривизна в верт. плоскости (большие радиусы), 100000/м
        /// </summary>
        [FieldOffset(80)]
        public float VerticalArchBig;
        /// <summary>
        /// Кривизна в гор. плоскости (большие радиусы), 100000/м
        /// </summary>
        [FieldOffset(84)]
        public float HorizontalArchBig;
        /// <summary>
        /// Модуль кривизны МР, 100000/м
        /// </summary>
        [FieldOffset(88)]
        public float AbsoluteArchSmall;
        /// <summary>
        /// Направление изгиба МР, град
        /// </summary>
        [FieldOffset(92)]
        public float ArchSmallDirection;
        /// <summary>
        /// Модуль кривизны БР, 100000/м
        /// </summary>
        [FieldOffset(96)]
        public float AbsoluteArchBig;
        /// <summary>
        /// Проекция линейного ускорения на боковую ось X, 0,00001 м/с2
        /// </summary>
        [FieldOffset(100)]
        public float LinearAccelProjX;
        /// <summary>
        /// Проекция линейного ускорения на продольную ось Y, 0,00001 м/с2
        /// </summary>
        [FieldOffset(104)]
        public float LinearAccelProjY;
        /// <summary>
        /// Проекция линейного ускорения на вертикальную ось Z, 0,00001 м/с2
        /// </summary>
        [FieldOffset(108)]
        public float LinearAccelProjZ;
        /// <summary>
        /// Угловая скорость по боковому гироскопу X, 0,00001 рад/с
        /// </summary>
        [FieldOffset(112)]
        public float AngularSpeedX;
        /// <summary>
        /// Угловая скорость по продольному гироскопу Y, 0,00001 рад/с
        /// </summary>
        [FieldOffset(116)]
        public float AngularSpeedY;
        /// <summary>
        /// Угловая скорость по вертикальному гироскопу Z, 0,00001 рад/с
        /// </summary>
        [FieldOffset(120)]
        public float AngularSpeedZ;
        /// <summary>
        /// Угол крена по акселерометрам, 0,01 град
        /// </summary>
        [FieldOffset(124)]
        public float RollAngleAccel;
        /// <summary>
        /// Угол крена по гироскопам, 0,01 град
        /// </summary>
        [FieldOffset(128)]
        public float RollAngleGyro;
        /// <summary>
        /// Угол тангажа по акселерометрам, 0,01 град
        /// </summary>
        [FieldOffset(132)]
        public float PitchAngleAccel;
        /// <summary>
        /// Угол тангажа по гироскопам, 0,01 град
        /// </summary>
        [FieldOffset(136)]
        public float PitchAngleGyro;
        /// <summary>
        /// Условное время прихода пакетов, 0,001 с
        /// </summary>
        [FieldOffset(140)]
        public float PacketArrivalTime;

        public static readonly NavScanData Empty;
    }
}

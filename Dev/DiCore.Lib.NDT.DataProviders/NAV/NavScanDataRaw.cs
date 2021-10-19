using System.Runtime.InteropServices;

namespace DiCore.Lib.NDT.DataProviders.NAV
{
    [StructLayout(LayoutKind.Explicit, Pack = 4)]
    public struct NavScanDataRaw
    {
        /// <summary>
        /// Линейная скорость
        /// </summary>
        [FieldOffset(0)]
        public int LinearSpeed;
        /// <summary>
        /// Угол крена
        /// </summary>
        [FieldOffset(4)]
        public int RollAngle;
        /// <summary>
        /// Тангаж
        /// </summary>
        [FieldOffset(8)]
        public int ElevationAngle;
        /// <summary>
        /// Азимут
        /// </summary>
        [FieldOffset(12)]
        public int AzimuthAngle;
        /// <summary>
        /// Кривизна в верт. плоскости
        /// </summary>
        [FieldOffset(16)]
        public int VerticalArch;
        /// <summary>
        /// Кривизна в гор. плоскости
        /// </summary>
        [FieldOffset(20)]
        public int HorizontalArch;
        /// <summary>
        /// Модуль кривизны
        /// </summary>
        [FieldOffset(24)]
        public int AbsoluteArch;
        /// <summary>
        /// Направление изгиба
        /// </summary>
        [FieldOffset(28)]
        public int ArchDirection;
        /// <summary>
        /// Координата север
        /// </summary>
        [FieldOffset(32)]
        public long X;
        /// <summary>
        /// Координата восток
        /// </summary>
        [FieldOffset(40)]
        public long Y;
        /// <summary>
        /// Координата высота
        /// </summary>
        [FieldOffset(48)]
        public int Z;
        /// <summary>
        /// Геодезическая широта
        /// </summary>
        [FieldOffset(52)]
        public int GeodeticLatitude;
        /// <summary>
        /// Геодезическая долгота
        /// </summary>
        [FieldOffset(56)]
        public int GeodeticLongitude;
        /// <summary>
        /// Кривизна в верт. плоскости малые радиусы
        /// </summary>
        [FieldOffset(60)]
        public int VerticalArchSmall;
        /// <summary>
        /// Кривизна в гор. плоскости малые радиусы
        /// </summary>
        [FieldOffset(64)]
        public int HorizontalArchSmall;
        /// <summary>
        /// Кривизна в верт. плоскости большие радиусы
        /// </summary>
        [FieldOffset(68)]
        public int VerticalArchBig;
        /// <summary>
        /// Кривизна в гор. плоскости большие радиусы
        /// </summary>
        [FieldOffset(72)]
        public int HorizontalArchBig;
        /// <summary>
        /// Проекция линейного ускорения на боковую ось X
        /// </summary>
        [FieldOffset(76)]
        public int LinearAccelProjX;
        /// <summary>
        /// Проекция линейного ускорения на продольную ось Y
        /// </summary>
        [FieldOffset(80)]
        public int LinearAccelProjY;
        /// <summary>
        /// Проекция линейного ускорения на вертикальную ось Z
        /// </summary>
        [FieldOffset(84)]
        public int LinearAccelProjZ;
        /// <summary>
        /// Угловая скорость по боковому гироскопу X
        /// </summary>
        [FieldOffset(88)]
        public int AngularSpeedX;
        /// <summary>
        /// Угловая скорость по продольному гироскопу Y
        /// </summary>
        [FieldOffset(92)]
        public int AngularSpeedY;
        /// <summary>
        /// Угловая скорость по вертикальному гироскопу Z
        /// </summary>
        [FieldOffset(96)]
        public int AngularSpeedZ;
        /// <summary>
        /// Угол крена по акселерометрам
        /// </summary>
        [FieldOffset(100)]
        public int RollAngleAccel;
        /// <summary>
        /// Угол крена по гироскопам
        /// </summary>
        [FieldOffset(104)]
        public int RollAngleGyro;
        /// <summary>
        /// Угол тангажа по акселерометрам
        /// </summary>
        [FieldOffset(108)]
        public int ElevationAngleAccel;
        /// <summary>
        /// Угол тангажа по гироскопам
        /// </summary>
        [FieldOffset(112)]
        public int ElevationAngleGyro;
        /// <summary>
        /// Условное время прихода пакетов
        /// </summary>
        [FieldOffset(116)]
        public int PacketArrivalTime;
    }
}

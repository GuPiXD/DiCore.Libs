using DiCore.Lib.NDT.DataProviders.CDL;

namespace DiCore.Lib.NDT.DataProviders.CDM
{
    public class DataHandleCdm: DataHandleCdl
    {
        /// <summary>
        /// Направление  - угол в градусах между вертикальной проекцией датчика на поверхность трубы и осью движения дефектоскопа против часовой стрелки (вид на носитель снаружи)
        /// </summary>
        public float DirectionAngle { get; internal set; }
        /// <summary>
        /// Идентификатор угла направления
        /// </summary>
        public int DirectionAngleCode { get; internal set; }
        /// <summary>
        /// Угол ввода в градусах
        /// </summary>
        public float EntryAngle { get; internal set; }

        public DataHandleCdm(int rowCount, int colCount) : base(rowCount, colCount)
        {
        }
    }
}
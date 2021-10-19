using DiCore.Lib.NDT.Types;

namespace DiCore.Lib.NDT.DataProviders.CDPA
{
    public class DataHandleCDpa : DataHandle<CDPASensorItem>
    {
        /// <summary>
        /// Идентификатор угла направления
        /// </summary>
        public int DirectionAngleCode { get; internal set; }
        /// <summary>
        /// Максимальное количество сигналов от одного датчика в скане
        /// </summary>
        public int MaxCountSignal { get; internal set; }
        public DataHandleCDpa(int rowCount, int colCount) : base(rowCount, colCount)
        {
        }
    }
}
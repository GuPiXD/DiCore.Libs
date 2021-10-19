using DiCore.Lib.NDT.Types;

namespace DiCore.Lib.NDT.DataProviders.CDL
{
    public class DataHandleCdl: DataHandle<CDSensorDataEx>
    {
        /// <summary>
        /// Максимальное количество сигналов от одного датчика в скане
        /// </summary>
        public int MaxCountSignal { get; internal set; }
        public DataHandleCdl(int rowCount, int colCount) : base(rowCount, colCount)
        {
        }
    }
}

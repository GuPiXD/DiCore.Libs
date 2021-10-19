using DiCore.Lib.NDT.Types;

namespace DiCore.Lib.NDT.DataProviders.EMA
{
    public class DataHandleEma : DataHandle<EmaSensorItem>
    {
        /// <summary>
        /// Максимальное количество сигналов от одного датчика в скане
        /// </summary>
        public int MaxCountSignal { get; internal set; }
        public DataHandleEma(int rowCount, int colCount) : base(rowCount, colCount)
        {
        }
    }
}

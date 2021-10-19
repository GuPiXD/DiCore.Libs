using DiCore.Lib.NDT.Types;

namespace DiCore.Lib.NDT.DataProviders.WM
{
    public class DataHandleWm : DataHandle<WMSensorData>
    {
        /// <summary>
        /// Наиболее часто встречаемое значение данных SO (потеря сигнала не учитывается)
        /// </summary>
        public float SoNominal { get; internal set; }

        /// <summary>
        /// Наиболее часто встречаемое значение данных WT (потеря сигнала не учитывается)
        /// </summary>
        public float WtNominal { get; internal set; }

        public DataHandleWm(int rowCount, int colCount) : base(rowCount, colCount)
        {
        }
    }
}

using System;
using DiCore.Lib.NDT.Types;

namespace DiCore.Lib.NDT.CoordinateProvider
{
    internal class CcdScanDeltaManager
    {
        private readonly CorruptedDataInfo corruptedDataInfo;
        private const int ArraySize = 5000;
        private Range<int> loadedCcdScanRange = new Range<int>(Int32.MinValue, Int32.MinValue + 1);
        private DeltaInfo[] deltas;

        public CcdScanDeltaManager(CorruptedDataInfo corruptedDataInfo)
        {
            this.corruptedDataInfo = corruptedDataInfo;
        }

        public DeltaInfo GetDelta(int ccdScan)
        {
            if (loadedCcdScanRange.IsInclude(true, false, ccdScan))
                return deltas[ccdScan - loadedCcdScanRange.Begin];

            if (corruptedDataInfo == null) return new DeltaInfo();
            deltas = corruptedDataInfo.GetCcdScanDeltas(ccdScan, ArraySize);
            loadedCcdScanRange.Begin = ccdScan;
            loadedCcdScanRange.End = ccdScan + ArraySize;

            return deltas[0];
        }
    }
}

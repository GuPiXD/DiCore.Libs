using System.Runtime.InteropServices;

namespace DiCore.Lib.NDT.CoordinateProvider
{
    [StructLayout(LayoutKind.Explicit, Pack = 4)]
    internal struct CcdIndexFileHeader
    {
        public CcdIndexFileHeader(long hashLow, long hashHigh, uint firstOdometer, uint firstTime, uint odometersCount, uint timesCount)
        {
            HashLow = hashLow;
            HashHigh = hashHigh;
            FirstOdometer = firstOdometer;
            FirstTime = firstTime;
            OdometersCount = odometersCount;
            TimesCount = timesCount;
        }

        [FieldOffset(0)]
        public uint FirstOdometer;
        [FieldOffset(4)]
        public uint FirstTime;
        [FieldOffset(8)]
        public uint OdometersCount;
        [FieldOffset(12)]
        public uint TimesCount;
        [FieldOffset(16)]
        public long HashLow;
        [FieldOffset(24)]
        public long HashHigh;

    }
}
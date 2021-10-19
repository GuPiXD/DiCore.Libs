using System.Runtime.InteropServices;

namespace DiCore.Lib.NDT.DataProviders.CDM
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct DataItem
    {
        public readonly byte TimeIndex;
        public readonly byte AmplitudeIndex;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    struct DataItem32
    {
        public readonly ushort AmplitudeIndex;
        public readonly ushort TimeIndex;
    }
}

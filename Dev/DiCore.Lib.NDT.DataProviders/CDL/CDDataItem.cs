using System.Runtime.InteropServices;

namespace DiCore.Lib.NDT.DataProviders.CDL
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct CDDataItem
    {
        public readonly byte TimeIndex;
        public readonly byte AmplitudeIndex;
    }
}

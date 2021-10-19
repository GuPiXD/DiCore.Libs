using System.Runtime.InteropServices;

namespace DiCore.Lib.NDT.DataProviders.EMA
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal struct EmaSensorEchosIndex
    {
        public ushort AmplitudeIndex;
        public ushort TimeIndex;
    }
}

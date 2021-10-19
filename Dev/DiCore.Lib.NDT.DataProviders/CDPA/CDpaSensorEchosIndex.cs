using System.Runtime.InteropServices;

namespace DiCore.Lib.NDT.DataProviders.CDPA
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct CDpaSensorEchosIndex
    {
        public ushort AmplitudeIndex;
        public ushort TimeIndex;
    }
}
using System.Runtime.InteropServices;

namespace DiCore.Lib.NDT.DataProviders.WM
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    internal struct WMEchoRaw
    {
        [FieldOffset(0)]
        public byte TimeCode;
        [FieldOffset(1)]
        public byte AmplitudeCode;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    internal struct WMEcho32Raw
    {
        [FieldOffset(0)]
        public ushort AmplitudeCode;
        [FieldOffset(2)]
        public ushort TimeCode;

    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    internal struct WMEchoRawAligned
    {
        [FieldOffset(0)]
        public short SO;
        [FieldOffset(2)]
        public short WT;
        [FieldOffset(4)]
        public byte AW;
    }
}

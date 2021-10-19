using System.Runtime.InteropServices;

namespace DiCore.Lib.NDT.DataProviders.WM
{
    [StructLayout(LayoutKind.Explicit, Pack = 4)]
    internal struct WMEcho2
    {
        [FieldOffset(0)]
        public float Time;
        [FieldOffset(4)]
        public float Amplitude;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    internal struct WMEcho3
    {
        [FieldOffset(0)]
        public float Time;
        [FieldOffset(4)]
        public int Time100;
        [FieldOffset(8)]
        public float Amplitude;
        [FieldOffset(12)]
        public byte AmplitudeCode;
    }
}

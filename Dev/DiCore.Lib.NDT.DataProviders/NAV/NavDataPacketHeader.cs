using System.Runtime.InteropServices;

namespace DiCore.Lib.NDT.DataProviders.NAV
{
    [StructLayout(LayoutKind.Explicit, Pack = 2)]
    internal struct NavDataPacketHeader
    {
        [FieldOffset(0)]
        public ushort Type;
        [FieldOffset(2)]
        public uint ScanNumber;
        [FieldOffset(6)]
        public ushort Size;
    }
}

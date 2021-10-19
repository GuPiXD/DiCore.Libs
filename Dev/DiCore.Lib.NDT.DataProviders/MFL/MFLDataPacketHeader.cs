using System.Runtime.InteropServices;

namespace DiCore.Lib.NDT.DataProviders.MFL
{
    [StructLayout(LayoutKind.Explicit, Pack = 2)]
    public struct MFLDataPacketHeader
    {
        [FieldOffset(0)]
        public ushort Type;
        [FieldOffset(2)]
        public uint ScanNumber;
        [FieldOffset(6)]
        public ushort Size;
    }
}

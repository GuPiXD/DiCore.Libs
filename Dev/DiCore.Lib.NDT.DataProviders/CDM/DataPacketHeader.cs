using System.Runtime.InteropServices;

namespace DiCore.Lib.NDT.DataProviders.CDM
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public unsafe struct DataPacketHeader
    {
        readonly ushort Type;
        public readonly uint ScanNumber;
        readonly ushort Size;

        public static readonly int RawSize = sizeof(DataPacketHeader);
    }
}

using System.Runtime.InteropServices;

namespace DiCore.Lib.NDT.DataProviders.EMA
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct EmaDataPacketHeader
    {
        public readonly ushort Type;
        public readonly uint ScanNumber;
        public ushort Size;
    }
}

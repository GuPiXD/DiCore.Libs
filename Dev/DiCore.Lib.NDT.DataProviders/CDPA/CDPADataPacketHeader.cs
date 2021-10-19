using System.Runtime.InteropServices;

namespace DiCore.Lib.NDT.DataProviders.CDPA
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct CDpaDataPacketHeader
    {
        readonly ushort Type;
        public readonly uint ScanNumber;
        public ushort Size;
    }
}
using System.Runtime.InteropServices;

namespace DiCore.Lib.NDT.CoordinateProvider
{
    [StructLayout(LayoutKind.Explicit, Pack = 2)]
    public struct CoordinateItem
    {
        [FieldOffset(0)] public uint Scan;
        [FieldOffset(4)] public uint Odometer;
        [FieldOffset(8)] public uint Time;
        [FieldOffset(12)] public ushort Angle;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 2)]
    public struct CoordinateItemFloat
    {
        [FieldOffset(0)] public double Odometer;
        [FieldOffset(8)] public uint Time;
        [FieldOffset(12)] public ushort Angle;
        [FieldOffset(14)] public int RefCCDScan;
    }
}

using System.Runtime.InteropServices;

namespace DiCore.Lib.NDT.CoordinateProvider
{
    [StructLayout(LayoutKind.Explicit, Pack = 4)]
    public struct DistRuleItem
    {
        [FieldOffset(0)] public int Scan;
        [FieldOffset(4)] public double Dist;
        [FieldOffset(12)] public uint Time;

        public static DistRuleItem Empty = new DistRuleItem {Scan = -1, Dist = -1d};
    }
}

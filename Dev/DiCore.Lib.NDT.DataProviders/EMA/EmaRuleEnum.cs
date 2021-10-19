using System;

namespace DiCore.Lib.NDT.DataProviders.EMA
{
    [Flags]
    public enum EmaRuleEnum : sbyte
    {
        Null = 0,
        NR = 1 << 0,
        R1 = 1 << 1,
        R2 = 1 << 2,
        L1 = 1 << 3,
        L2 = 1 << 4,
        N = 1 << 5,
        NC = 1 << 6
    }
}

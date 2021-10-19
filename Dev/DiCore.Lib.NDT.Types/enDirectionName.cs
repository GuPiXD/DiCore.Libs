using System;

namespace DiCore.Lib.NDT.Types
{
    [Flags]
    public enum enDirectionName
    {
        None = 0,
        Cdc = 1 << 1,
        Cds = 1 << 2,
        Cdh = 1 << 3,
        Cdl = 1 << 4,
        Cdg = 1 << 5,
        Cdf = 1 << 6
    }
}
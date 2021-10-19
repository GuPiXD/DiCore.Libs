using System.Runtime.InteropServices;


namespace DiCore.Lib.NDT.DataProviders.EMA
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct EmaSensorDataRaw
    {
        public sbyte RuleId;
        public byte EchoCount;
    }
}

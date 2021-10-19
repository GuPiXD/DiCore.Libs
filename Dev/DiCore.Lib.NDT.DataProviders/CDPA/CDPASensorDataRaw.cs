using System.Runtime.InteropServices;

namespace DiCore.Lib.NDT.DataProviders.CDPA
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct CDPASensorDataRaw
    {
        public ushort RuleId;
        public ushort EchoCount;
    }
}
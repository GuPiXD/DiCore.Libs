using System.Runtime.InteropServices;

namespace DiCore.Lib.NDT.DataProviders.CDPA
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public unsafe struct CDPASensorData
    {
        public ushort RuleId;
        public ushort EchoCount;
        public CDPAEcho* Echos;
    }
}
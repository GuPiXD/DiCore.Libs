using System.Runtime.InteropServices;

namespace DiCore.Lib.NDT.DataProviders.EMA
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct EmaSensorData
    {
        public EmaRuleEnum Rule;
        public byte EchoCount;
        public EmaEcho* Echos;
    }
}

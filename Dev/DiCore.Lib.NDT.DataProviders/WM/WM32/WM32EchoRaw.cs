using System.Runtime.InteropServices;

namespace DiCore.Lib.NDT.DataProviders.WM.WM32
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct WM32EchoRaw
    {
        public readonly byte TimeIndex;
        public readonly byte AmplitudeIndex;
    }
}
using System.Runtime.InteropServices;

namespace DiCore.Lib.ServerTasks.Idt
{
    [StructLayout(LayoutKind.Explicit, Pack = 2)]
    public struct CoordinateItem
    {
        [FieldOffset(0)] public uint Scan;
        [FieldOffset(4)] public uint Odometer;
        [FieldOffset(8)] public uint Time;
        [FieldOffset(12)] public ushort Angle;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public CoordinateItem(uint scan, uint odometer, uint time, ushort angle)
        {
            Scan = scan;
            Odometer = odometer;
            Time = time;
            Angle = angle;
        }
    }
}

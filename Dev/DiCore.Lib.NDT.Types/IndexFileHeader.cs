using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DiCore.Lib.NDT.Types
{
    [StructLayout(LayoutKind.Explicit, Pack = 2)]
    internal struct IndexFileHeader
    {
        [FieldOffset(0)]
        public ushort FileType;
        [FieldOffset(2)]
        public ushort HeaderSize;
        [FieldOffset(4)]
        public ushort DataCode;
        [FieldOffset(6)]
        public uint MinScanNumber;
        [FieldOffset(10)]
        public uint MaxScanNumber;
        [FieldOffset(14)]
        public ushort Reserved;

        internal bool CheckHeader(ushort fileType, IEnumerable<ushort> dataCode, ushort step)
        {
            foreach (var dc in dataCode)
            {
                if /*((FileType != fileType) || //Проверка типа файла*/
                    (DataCode == dc) /*)||//Проверка кода информационного пространства
                    /*(Reserved != 1)) //Проверка шага сканов*/
                return true;
            }
            return false;
        }
    }
}

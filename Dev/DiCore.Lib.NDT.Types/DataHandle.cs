using System;
using System.Runtime.InteropServices;
using Diascan.Utils.DataBuffers;

namespace DiCore.Lib.NDT.Types
{
    public unsafe class DataHandle<T> : IDisposable where T: unmanaged
    {
        /// <summary>
        /// Указатель на данные
        /// </summary>
        public T* Ptr { get; }
        /// <summary>
        /// Количество строк в матрице данных
        /// </summary>
        public int RowCount { get; }
        /// <summary>
        /// Количество столбцов в матрице данных
        /// </summary>
        public int ColCount { get; }

        private readonly int elementSize;
        private readonly MemoryAllocator memoryAllocator;

        public DataHandle(int rowCount, int colCount)
        {
            RowCount = rowCount;
            ColCount = colCount;
            elementSize = Marshal.SizeOf<T>();

            memoryAllocator = new MemoryAllocator((uint) (Environment.SystemPageSize*1000));

            var totalSize = (uint)(rowCount * colCount * elementSize);

            Ptr = (T*)memoryAllocator.Allocate(totalSize);
        }

        public T* GetDataPointer(int row, int col)
        {
            return Ptr + (row * ColCount + col);
        }

        public TA* Allocate<TA>(uint count) where TA: unmanaged
        {
            return (TA*)memoryAllocator.Allocate((uint) (Marshal.SizeOf<TA>() * count));
        }

        public void Dispose()
        {
            memoryAllocator.Free();
            memoryAllocator.Dispose();
        }
    }
}

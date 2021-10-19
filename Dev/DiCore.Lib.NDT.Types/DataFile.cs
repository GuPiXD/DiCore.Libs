//#define readwrite

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Diascan.Utils.FileMapper;

namespace DiCore.Lib.NDT.Types
{
    /// <summary>
    /// Тип для работы с файлом индексов
    /// </summary>
    internal sealed class DataFile: IDisposable
    {
        #region Свойства и поля

        /// <summary>
        /// Номер файла
        /// </summary>
        public int FileNumber { get; private set; }

        /// <summary>
        /// Маппер файла данных
        /// </summary>
        private FileMapper file;

        /// <summary>
        /// Заголовок
        /// </summary>
        internal DataFileHeader header;  

        private bool disposed;
        private readonly uint dataBufferSize;
        private readonly DiagdataDescription description;
        private readonly string path;
        private int maxScanSize;
        private bool readOnly;

        #endregion

        #region Конструктор

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="description"></param>
        /// <param name="path"></param>
        /// <param name="dataBufferSize"></param>
        /// <param name="maxScanSize"></param>
        public DataFile(DiagdataDescription description, string path, uint dataBufferSize, int maxScanSize, bool readOnly = true)
        {
            this.description = description;
            this.path = path;
            this.dataBufferSize = dataBufferSize;
            this.maxScanSize = maxScanSize;
            this.readOnly = readOnly;
        }

        #endregion   
        
        #region GetScan()

        /// <summary>
        /// Получение скана в файле данных
        /// </summary>
        /// <param name="offset">Номер скана</param>        
        /// <param name="maxScanSize"></param>
        /// <returns>Указатель на скан</returns>
        internal UIntPtr GetScan(uint offset)
        {
            var range = file.BufferRange;
            var begin = range.Begin;
            var end = range.End;
            var delta = (int) (offset - begin);

            if (delta >= 0 && (offset + maxScanSize) < end)
                return file.Buffer + delta;

            file.MapBuffer(offset);

            return file.Buffer; 
            
            //if (file.BufferRange.Contains(new Range<long>(offset, offset + maxScanSize)))
            //    return (IntPtr) ((int) file.Buffer + offset - file.BufferRange.Begin);
            
            //file.MapBuffer(offset);            

            //return file.Buffer;
        }

        #endregion
      
        #region CheckBorder()

        /// <summary>
        /// Проверка границ (соответствие номера скана данному файлу)
        /// </summary>
        /// <param name="scanNumber">Номер скана</param>
        /// <param name="smaller">Необходим файл с меньшим номером</param>
        /// <returns>Номер скана внутри файла индексов</returns>
        private bool CheckBorder(uint scanNumber, out bool smaller)
        {
            smaller = false;

            if (header.MinScanNumber > scanNumber)
            {
                smaller = true;
                return false;
            }
            
            return scanNumber <= header.MaxScanNumber;
        }
 
        /// <summary>
        /// Проверка границ (соответствие номера скана данному файлу)
        /// </summary>
        /// <param name="scanNumber">Номер скана</param>
        /// <returns>Номер скана внутри файла индексов</returns>
        private bool CheckBorder(uint scanNumber)
        {
            if (header.MinScanNumber > scanNumber)
                return false;

            return scanNumber <= header.MaxScanNumber;
        }

        #endregion

        #region Open()

        /// <summary>
        /// Открытие и проверка файла
        /// </summary>
        /// <param name="number">Номер файла</param>
        /// <returns>Результат открытия</returns>
        internal unsafe bool Open(int number)
        {
            if (file != null)
            {
                if (FileNumber == number)
                    return true;
                
                file.Dispose();
                file = null;
            }

            var source = String.Concat(path, String.Format("_{0:d6}", number), description.DataFileExt);

#if readwrite
            file = new FileMapper(source, false, String.Empty, 0, dataBufferSize);
#else
            file = new FileMapper(source, readOnly, String.Empty, 0, dataBufferSize);
#endif
            
            header = *(DataFileHeader*)file.Buffer;
            FileNumber = number;

            return true;
        }

        #endregion

        #region struct DataFileHeader

        [StructLayout(LayoutKind.Explicit)]
        public struct DataFileHeader
        {
            [FieldOffset(0)] public ushort FileType;
            [FieldOffset(2)] public ushort DataCode;
            [FieldOffset(4)] public ushort HeaderSize;
            [FieldOffset(6)] public uint MinScanNumber;
            [FieldOffset(10)] public uint MaxScanNumber;

            internal bool CheckHeader(ushort fileType, IEnumerable<ushort> dataCode)
            {
                foreach (var dc in dataCode)
                {
                    if (DataCode == dc) // Проверка кода информационного пространства
                        return true;
                }
                return false;
            }
        }

        #endregion

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool isDisposing)
        {
            if (disposed) return;
            if (file != null)
                file.Dispose();

            disposed = true;
        }       

        public unsafe ushort GetFileType(uint scanOffset)
        {
            var range = file.BufferRange;
            var begin = range.Begin;
            var end = range.End;
            var delta = scanOffset - begin;

            if ((delta >= 0) && ((scanOffset + 2) <= end))
                return *((ushort*) ((ulong)file.Buffer + (ulong)delta));

            file.MapBuffer(scanOffset);
            return *((ushort*) file.Buffer);
        }
    }

    #region struct DataHeader

    [StructLayout(LayoutKind.Explicit)]
    internal struct DataHeader
    {
        [FieldOffset(0)] public ushort DataType;
        [FieldOffset(2)] public uint Scan;
        [FieldOffset(6)] public ushort DataSize;
    }

    #endregion
}

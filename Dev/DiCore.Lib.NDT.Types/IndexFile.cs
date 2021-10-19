using System;
using System.Runtime.ExceptionServices;
using Diascan.Utils.FileMapper;

namespace DiCore.Lib.NDT.Types
{
    /// <summary>
    /// Тип для работы с файлом индексов
    /// </summary>
    public sealed class IndexFile : IDiagDataReader
    {
        #region Свойства и поля

        private int fileNumber;

        /// <summary>
        /// Номер файла
        /// </summary>
        public int FileNumber => fileNumber;

        public bool FileExist => file != null;

        /// <summary>
        /// Маппер индексного файла
        /// </summary>
        private FileMapper file;

        /// <summary>
        /// Заголовок
        /// </summary>
        private IndexFileHeader header;

        /// <summary>
        /// Указатель на массив индексов
        /// </summary>
        private unsafe uint* offsetVector;        

        /// <summary>
        /// Маска пути к файлам данных
        /// </summary>
        private readonly string path;

        /// <summary>
        /// Соответствующий файл данных
        /// </summary>
        private readonly DataFile dataFile;

        private bool disposed;

        private uint minScanNumber;
        private uint maxScanNumber;

        private readonly DiagdataDescription description;
        private long recordCount;
        private const int indexFileSizeThreshold = 64 * 1024 * 1024;
        private const uint WRONG_SCANOFFSET = 0xFFFFFFFF;

        public uint MinScanNumber => minScanNumber;

        public uint MaxScanNumber => maxScanNumber;

        #endregion

        #region Конструктор

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="description"></param>
        /// <param name="path">Маска пути к данным</param>
        /// <param name="dataBufferSize"></param>
        /// <param name="maxScanSize"></param>
        /// <param name="readOnly"></param>
        public IndexFile(DiagdataDescription description, string path, uint dataBufferSize, int maxScanSize, bool readOnly = true)
        {
            this.description = description;
            this.path = path;

            dataFile = new DataFile(description, path, dataBufferSize, maxScanSize, readOnly);            
            DefaultInit();
        }

        #endregion

        #region GetOffset()
        private unsafe uint GetOffset(int scanNumber)
        {
            var diffScans = scanNumber - minScanNumber; //!!! А учитывать частоту индексации кто будет??? Никто она должна быть равна 1 и точка. (c) Котов А.В.

            if (diffScans >= recordCount)
                return WRONG_SCANOFFSET;

            var offset = offsetVector[diffScans];

            return offset == 0 ? WRONG_SCANOFFSET : offset;
        }

        #endregion

        private unsafe void DefaultInit()
        {
            minScanNumber = uint.MaxValue;
            maxScanNumber = uint.MinValue;
            fileNumber = -1;
            offsetVector = null;
        }

        public bool ContainsScan(int scan)
        {
            return minScanNumber <= scan && scan <= maxScanNumber;
        }

        public UIntPtr GetDataPointer(int scanNumber)
        {
            var scanOffset = GetOffset(scanNumber);

            return scanOffset == WRONG_SCANOFFSET ? UIntPtr.Zero : dataFile.GetScan(scanOffset);
        }

        #region CheckBorder()

        /// <summary>
        /// Проверка границ (соответствие номера скана данному файлу)
        /// </summary>
        /// <param name="scanNumber">Номер скана</param>
        /// <param name="smaller">Необходим файл с меньшим номером</param>
        /// <returns>Номер скана внутри файла индексов</returns>
        private bool CheckBorder(int scanNumber, out bool smaller)
        {      
            smaller = false;
            
            if (fileNumber == -1)
                return false;

            if (minScanNumber > scanNumber)
            {
                smaller = true;
                return false;
            }

            return scanNumber <= maxScanNumber;
        }
 
        /// <summary>
        /// Проверка границ (соответствие номера скана данному файлу)
        /// </summary>
        /// <param name="scanNumber">Номер скана</param>
        /// <returns>Номер скана внутри файла индексов</returns>
        private bool CheckBorder(int scanNumber)
        {
            if (fileNumber == -1)
                return false;

            if (minScanNumber > scanNumber)
                return false;

            return scanNumber <= maxScanNumber;
        }

        #endregion

        #region Open()

        /// <summary>
        /// Открытие и проверка файла
        /// </summary>
        /// <param name="number">Номер файла</param>
        /// <returns>Результат открытия</returns>
        public unsafe bool Open(int number)
        {           
            /* первый раз filе д.б. null */
            if (file != null)
            {
                if (fileNumber == number)
                    return true;

                file.Dispose();
                file = null;
            }

            DefaultInit();            
            var filePath = String.Concat(path, $"_{number:d6}", description.IndexFileExt);

            if (!Diascan.Utils.IO.File.Exists(filePath)) return false;

            var length = Diascan.Utils.IO.File.GetLength(filePath);
            
            file = new FileMapper(filePath, true, String.Empty, 0, length);            
            header = *((IndexFileHeader*)file.Buffer);

            recordCount = (file.FileSize - sizeof (IndexFileHeader))/sizeof (uint);

            minScanNumber = header.MinScanNumber;
            maxScanNumber = header.MaxScanNumber;
            fileNumber = number;
            offsetVector = (uint*)(file.Buffer + header.HeaderSize);

            return dataFile.Open(fileNumber);            
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool isDisposing)
        {
            if (disposed) return;

            file?.Dispose();
            dataFile?.Dispose();

            disposed = true;
        }

        #endregion
        internal ushort GetFileType()
        {
            for (var i = (int) minScanNumber; i < maxScanNumber; i++)
            {
                var scanOffset = GetOffset(i);

                if (scanOffset == WRONG_SCANOFFSET)
                {
                    continue;
                }
                
                return dataFile.GetFileType(scanOffset);
            }

            return 0;
        }

        internal ushort GetDataFileType()
        {
            return dataFile.header.FileType;
        }
    }     
}

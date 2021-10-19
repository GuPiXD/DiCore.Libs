using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using Diascan.Utils.IO;

namespace DiCore.Lib.NDT.Types
{
    public class IndexFileManager: IDisposable
    {
        private const int MAX_OPENED_FILES = 2;
        private readonly ConcurrentBag<IndexFile> indexFilesTh = new ConcurrentBag<IndexFile>();

        /// <summary>
        /// Файл указателей данного типа данных
        /// </summary>
        private PointerFile pointerFile;
        private bool disposed;
        private readonly string path;
        private readonly uint dataBufferSize;
        private readonly ushort fileType;
        private readonly int maxScanSize;
        private readonly DiagdataDescription description;

        public ushort FileType
        {
            get { return fileType; }
        }

        public readonly IndexFile dataReader;
        private int dataReaderCount;
        private bool readOnly;

        public int MaxScan{ get { return pointerFile.MaxScan; }}
        public int MinScan{ get { return pointerFile.MinScan; }}
        public int FirstScan { get; private set; }

        public IndexFileManager(DiagdataDescription description, string path, uint dataBufferSize, int maxScanSize, bool readOnly = true)
        {           
            this.description = description;
            this.path = path;
            this.dataBufferSize = dataBufferSize;
            this.maxScanSize = maxScanSize;
            this.readOnly = readOnly;

            pointerFile = new PointerFile(description.PointerFileExt, this.path);                                    

            var directoryName = Path.GetDirectoryName(this.path);
            if (directoryName == null) throw new ArgumentException("Not found path", path);
            
            var file = Directory.EnumerateFiles(directoryName, $"{Path.GetFileName(this.path)}_??????{description.IndexFileExt}").FirstOrDefault();
            if (String.IsNullOrEmpty(file)) throw new ArgumentException("Not found path", path);

            var fileName = Path.GetFileName(file);
            var minNumber = fileName.Substring(fileName.LastIndexOf("_", StringComparison.Ordinal) + 1, 6);
            int number;
            if (!Int32.TryParse(minNumber, out number)) throw new ArgumentException("Not found path", path);

            FirstScan = pointerFile.GetFirstScan(number);

            dataReader = CreateDataReader();
            indexFilesTh.Add(dataReader);
            dataReaderCount = 1;

            dataReader.Open(number);
            fileType = dataReader.GetFileType();                     
        }

        private IndexFile CreateDataReader()
        {            
            return new IndexFile(description, path, dataBufferSize, maxScanSize, readOnly);
        }

        public ushort GetDataFileType()
        {
            return dataReader.GetDataFileType();
        }

        /// <summary>
        /// Получить объект чтения данных из пула объектов
        /// </summary>
        /// <returns></returns>
        public IDiagDataReader TakeDataReader()
        {
            IndexFile indexFile;

            while (!indexFilesTh.TryTake(out indexFile))
            {
                var locDataReaderCount = dataReaderCount;
                if (locDataReaderCount < MAX_OPENED_FILES)
                {                    
                    if (Interlocked.CompareExchange(ref dataReaderCount, locDataReaderCount + 1, locDataReaderCount) ==
                        locDataReaderCount)
                    {
                            indexFile = CreateDataReader();
                            break;
                    }
                }
                Thread.Sleep(300);
            }

            return indexFile;
        }

        /// <summary>
        /// Вернуть объект чтения данных в пул объектов
        /// </summary>
        /// <param name="dataReader"></param>
        public void RetractDataReader(IDiagDataReader dataReader)
        {
            indexFilesTh.Add((IndexFile)dataReader);
        }
        
        public UIntPtr GetDataPointer(IDiagDataReader dataReader, int scanNumber)
        {
            if (dataReader.ContainsScan(scanNumber)) return dataReader.GetDataPointer(scanNumber);

            var needFile = pointerFile.GetNumberFile(scanNumber);

            if (needFile == -1)
                return UIntPtr.Zero;

            var targetScan = dataReader.Open(needFile) ? dataReader.GetDataPointer(scanNumber) : UIntPtr.Zero;            

            return targetScan;
        }

        public UIntPtr GetDataPointer(int scanNumber)
        {
            return GetDataPointer(dataReader, scanNumber);
        }
        
        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool isDisposing)
        {
            if (disposed) return;

            if (isDisposing)
            {
                pointerFile = null;

                IndexFile indexFile;
                while (indexFilesTh.TryTake(out indexFile))
                {
                    indexFile.Dispose();
                }
            }

            disposed = true;
        }

        public Range<uint>[] GetRanges()
        {
            return pointerFile.GetRanges();
        }

        public int GetFileNumber(int physScan)
        {
            return pointerFile.GetNumberFile(physScan);
        }
    }
}

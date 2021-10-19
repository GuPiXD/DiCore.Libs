using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Diascan.Utils.FileMapper;
using File = Diascan.Utils.IO.File;
using File2 =  System.IO.File;

namespace DiCore.Lib.NDT.Types
{
    /// <summary>
    /// Тип для работы с файлом указателей
    /// </summary>
    public sealed class PointerFile
    {
        #region Свойства и поля

        /// <summary>
        /// Тип файла указателя (одновременно и расширение физического файла)
        /// </summary>
        private readonly string[] type;
        
        /// <summary>
        /// Массив указателей
        /// </summary>
        private PointerItem[] pointerItems;

        /// <summary>
        /// Макс. скан в прогоне для данного типа данных
        /// </summary>
        public int MaxScan { get; private set; }

        /// <summary>
        /// Мин. скан в прогоне для данного типа данных
        /// </summary>
        public int MinScan { get; private set; }

        private int averageInterval;

        /// <summary>
        /// Маска пути к файлам данных
        /// </summary>
        private readonly string diagDataMask;

        private long recordCount;
        private string path;

        public string Path => path;

        public int MaxFileNumber => (int) (recordCount - 1);

        /// <summary>
        /// Размер записи в файле указателей
        /// </summary>
        private const int RecordSize = 8;

        #endregion

        #region Конструктор

        /// <summary>
        /// Конструктор объекта для работы с файлом указателей
        /// </summary>
        /// <param name="type">Расширение (тип) файла</param>
        /// <param name="diagDataMask">Маска пути к файлам данных конкретного типа в виде ("...\Имя_прогона_DATAXX\Имя_прогона")</param>
        public PointerFile(string[] type, string diagDataMask)
        {
            this.type = type;
            this.diagDataMask = diagDataMask;

            BuildPointers();
        }

        #endregion

        #region BuildPointers()

        /// <summary>
        /// Инициализация объекта - построение массива указателей
        /// </summary>
        /// <returns>Результат построения массива указателей</returns>
        private unsafe void BuildPointers()
        {
            foreach (var pointerFileExt in type)
            {
                var searchPath = $"{diagDataMask}{pointerFileExt}";

                if (File.Exists(searchPath))
                {
                    this.path = String.Concat(diagDataMask, pointerFileExt) ;
                    break;
                }
            }

            if (String.IsNullOrEmpty(Path))
                throw new FileNotFoundException(System.IO.Path.Combine(diagDataMask, type.FirstOrDefault() ?? ""));

            using (var pointerMapper = new FileMapper(path, true, String.Empty, 0, File.GetLength(path)))
            {
                if (pointerMapper.FileSize == 0)
                    throw new Exception($"Неверный формат файла {path}");

                if (pointerMapper.FileSize%RecordSize != 0) //проверка на кратность размеру одной записи
                    throw new Exception($"Неверный формат файла {path}");

                if (pointerMapper.BufferRange.End != pointerMapper.FileSize)
                    throw new Exception($"Неверный формат файла {path}");

                recordCount = pointerMapper.FileSize/RecordSize;                                            
                pointerItems = new PointerItem[recordCount];

                var pScanRange = (RawScanRange*) pointerMapper.Buffer;

                for (var i = 0; i < recordCount; i++)
                {
                    var pointer = new PointerItem {File = i, Start = pScanRange->Start, Stop = pScanRange->Stop};
                    pointerItems[i] = pointer;
                    pScanRange++;
                }

                MinScan = (int)pointerItems[0].Start;
                MaxScan = (int)pointerItems[recordCount - 1].Stop;

                averageInterval = (int) ((MaxScan - MinScan)/recordCount);

                pointerMapper.Dispose();
            }
        }

        #endregion

        #region GetNumberFile()

        /// <summary>
        /// Получение номера файла данных/индекса для определенного номера скана
        /// </summary>
        /// <param name="scanNumber">Номер скана</param>
        /// <returns>Номер файла</returns>
        internal int GetNumberFile(int scanNumber)
        {
            var index = InternalBinarySearch(scanNumber);

            return index;
        }

        private int InternalBinarySearch(int scan)
        {
            var length = pointerItems.Length;
            var bottom = 0;
            var top = length - 1;
            var index = Math.Min(scan / averageInterval, top);
            var iterationNumber = 0;
            const int maxIterationCount = 1000;

            while (bottom <= top)
            {
                var target = pointerItems[index];
                int newIndex;
                if (target.Stop - scan < 0)
                {
                    bottom = index;
                    newIndex = bottom + (int)Math.Ceiling((top - bottom) / 2.0);
                }
                else if (target.Start - scan > 0)
                {
                    top = index;
                    newIndex = bottom + (int)Math.Floor((top - bottom) / 2.0);
                }
                else
                
                {
                    return index;
                }                

                if (newIndex == index || newIndex >= length) return -1;

                index = newIndex;
                iterationNumber++;

                if (iterationNumber > maxIterationCount) return -1;
            }

            return -1;
        }       

        #endregion

        public Range<uint>[] GetRanges()
        {
            return pointerItems.Select(item => new Range<uint>(item.Start, item.Stop)).ToArray();
        }

        public int GetFirstScan(int fileNumber)
        {
            if (pointerItems == null || fileNumber < 0 || fileNumber >= pointerItems.Length) return 0;
            return pointerItems[fileNumber].File == fileNumber ? (int)pointerItems[fileNumber].Start : 0;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    struct RawScanRange
    {
        public readonly uint Start;
        public readonly uint Stop;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    struct PointerItem
    {
        public int File;
        public uint Start;
        public uint Stop;
    } 
}

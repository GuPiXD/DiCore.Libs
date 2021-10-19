using System;
using System.Diagnostics;
using Diascan.Utils.FileMapper;
using Diascan.Utils.IO;
using DiCore.Lib.NDT.CoordinateProvider;

namespace DiCore.Lib.CoordinateProvider
{
    class IdtFile: IDisposable
    {
        private FileMapper file;
        private unsafe uint* dataOdometers;
        private unsafe uint* dataTimes;
        private ulong bufferRef;

        private CcdIndexFileHeader header;

        internal bool CheckHash(long hashLow, long hashHigh)
        {
            return (header.HashLow == hashLow) && (header.HashHigh == hashHigh);
        }

        public unsafe bool Open(string path)
        {
            if (!File.Exists(path)) return false;

            try
            {                
                file = new FileMapper(path, true, String.Empty, 0, File.GetLength(path));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());

                return false;
            }

            if (file.BufferRange.End < file.FileSize)
                throw new InvalidOperationException(@"Файл idt отобразился в память не полностью");

            if (file.FileSize < 40)
            {
                file.Dispose();
                return false;
            }

            bufferRef = (ulong)file.Buffer;

            header = *(CcdIndexFileHeader*)bufferRef;

            dataOdometers = (uint*)(bufferRef + (ulong) sizeof(CcdIndexFileHeader));

            dataTimes = (uint*)(bufferRef + (ulong) sizeof(CcdIndexFileHeader) + header.OdometersCount * 4);

            return true;
        }

        public unsafe void Close()
        {
            dataOdometers = null;
            dataTimes = null;
            file?.Dispose();
        }

        public unsafe bool Create(string path, uint odometerRecordCount, uint timeRecordCount, uint firstOdometer, uint firstTime, long hashLow, long hashHigh)
        {
            Close();

            try
            {
                if (Diascan.Utils.IO.File.Exists(path))
                {
                    throw new InvalidOperationException(String.Format(@"Файл {0} уже существует, для создания нового выполните его удаление!", path));
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                return false;
            }

            try
            {
                var fileSize = (uint) (sizeof (CcdIndexFileHeader) + (odometerRecordCount + timeRecordCount)*4);
                file = new FileMapper(path, false, String.Empty, fileSize, fileSize);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }

            if (file.BufferRange.End != file.FileSize)
                throw new InvalidOperationException(@"Файл idt отобразился в память не полностью");

            bufferRef = (ulong) file.Buffer;

            header = new CcdIndexFileHeader(hashLow, hashHigh, firstOdometer, firstTime, odometerRecordCount, timeRecordCount);

            *(CcdIndexFileHeader*)bufferRef = header;

            dataOdometers = (uint*)(bufferRef + (ulong) sizeof(CcdIndexFileHeader));

            dataTimes = (uint*)(bufferRef + (ulong) sizeof(CcdIndexFileHeader) + header.OdometersCount * 4);

            return true;
        }

        public unsafe void WriteOdometer(uint indexFromZero, uint number)
        {
            if (indexFromZero > header.OdometersCount - 1)
                return;

            dataOdometers[indexFromZero] = number;
        }

        public unsafe void WriteTime(uint indexFromZero, uint number)
        {

            if (indexFromZero > header.TimesCount - 1)
                return;

            dataTimes[indexFromZero] = number;
        }

        public unsafe uint NextDifferingRecordNumber(uint index)
        {
            var recordNumber = ReadOdometer(index);

            var offset = (int) (index - header.FirstOdometer);

            if (offset < 0) return dataOdometers[0];

            offset++;

            while (offset < header.OdometersCount)
            {
                if (recordNumber != dataOdometers[offset])
                    return dataOdometers[offset];
                offset++;
            }

            return dataOdometers[header.OdometersCount - 1];
        }

        public unsafe uint ReadOdometer(uint index)
        {
            var offset = (int) (index - header.FirstOdometer);
            if (offset < 0)
                return dataOdometers[0];

            return offset < header.OdometersCount - 1 ? dataOdometers[offset] : dataOdometers[header.OdometersCount - 1];
        }

        public uint ReadTime(uint index, out uint nextIndexedRecordNumber)
        {
            nextIndexedRecordNumber = ReadTime(index + 1);

            return ReadTime(index);
        }

        public unsafe uint ReadTime(uint index)
        {
            var offset = (long)index - header.FirstTime;

            if (offset < 0)
                return dataTimes[0];

            return offset < header.TimesCount - 1 ? dataTimes[offset] : dataTimes[header.TimesCount - 1];
        }

        public void Dispose()
        {
            Close();
        }
    }
}

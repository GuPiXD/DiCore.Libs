using System;
using System.IO;

namespace DiCore.Lib.ServerTasks.Idt
{
    internal class IDTFile: IDisposable
    {
        private FileStream file;
        private bool disposed;

        private IdtFileHeader header;
        private long dataOdometersOffset;
        private long dataTimesOffset;

        internal bool Check(long hashLow, long hashHigh)
        {
            return (header.HashLow == hashLow) && (header.HashHigh == hashHigh);
        }

        public bool Open(string path)
        {
            if (!File.Exists(path))
                return false;
            try
            {
                file = File.Open(path, FileMode.Open, FileAccess.ReadWrite);
            }
            catch
            {
                return false;
            }

            if (ReadHeader())
                unsafe
                {
                    dataOdometersOffset = sizeof (IdtFileHeader);
                    dataTimesOffset = dataOdometersOffset + header.OdometersCount*sizeof(uint);
                    return true;
                }

            return false;
        }

        private unsafe bool ReadHeader()
        {
            if (file == Stream.Null || file.Length < sizeof(IdtFileHeader))
            {
                header = new IdtFileHeader();
                return false;
            }

            file.Seek(0, SeekOrigin.Begin);
            using (var reader = new BinaryReader(file))
            {
                header = new IdtFileHeader(reader.ReadUInt32(), reader.ReadUInt32(), reader.ReadUInt32(),
                    reader.ReadUInt32(), reader.ReadInt64(), reader.ReadInt64());
            }
            return true;
        }

        public unsafe bool Create(string path, uint firstOdometer, uint firstTime, uint odometerRecordCount,
            uint timeRecordCount, long hashLow, long hashHigh)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {
                return false;
            }

            try
            {
                file = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, 4096);
                file.SetLength(sizeof (IdtFileHeader) + (odometerRecordCount + timeRecordCount)*4);
                file.Seek(0, SeekOrigin.Begin);
                var writer = new BinaryWriter(file);

                writer.Write(firstOdometer);
                writer.Write(firstTime);
                writer.Write(odometerRecordCount);
                writer.Write(timeRecordCount);
                writer.Write(hashLow);
                writer.Write(hashHigh);

                writer.Flush();

                header = new IdtFileHeader(firstOdometer, firstTime, odometerRecordCount, timeRecordCount, hashLow,
                    hashHigh);
            }
            catch
            {
                return false;
            }

            dataOdometersOffset = sizeof (IdtFileHeader);
            dataTimesOffset = dataOdometersOffset + header.OdometersCount*sizeof (uint);

            return true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool isDisposing)
        {        
            file?.Close();    
            if (disposed) return;
            disposed = true;
        }

        ~IDTFile()
        {
            Dispose(false);
        }

        public void WriteData(uint[] data, bool isOdometersData)
        {
            if (file == Stream.Null)
                return;

            var writer = new BinaryWriter(file);

            file.Seek(isOdometersData ? dataOdometersOffset : dataTimesOffset, SeekOrigin.Begin);
            foreach (var item in data)
                writer.Write(item);
            writer.Flush();
        }
    }
}

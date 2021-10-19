using System;
using System.Runtime.InteropServices;
using Diascan.Utils.FileMapper;
using DiCore.Lib.NDT.Types;

namespace DiCore.Lib.NDT.DataProviders.NAV.NavSetup
{
    internal class SetupDataFile
    {
        private FileMapper fileMapper;
        private DataFileHeader fileHeader;

        private readonly unsafe int packetHeaderSize = sizeof(NavDataPacketHeader);
        private readonly unsafe int scanDataRawSize = sizeof(NavScanDataRaw);
        
        public int MaxScan => (int) fileHeader.MaxScanNumber;
        public int MinScan => (int) fileHeader.MinScanNumber;
        public ushort NavTypeCode => fileHeader.FileType;

        public SetupDataFile()
        {
        }

        public unsafe bool Open(DataLocation location, DiagdataDescription description)
        {
            var path = location.DataBasePath + description.DataDirSuffix + location.BaseName;
            var fullPath = string.Concat(path, $"_{0:d6}", description.DataFileExt);

            if (!Diascan.Utils.IO.File.Exists(fullPath)) return false;

            fileMapper = new FileMapper(fullPath, true, string.Empty, 0, FileMapperHelper.DefaultBufferSizeInByte);
            fileHeader = *(DataFileHeader*)fileMapper.Buffer;

            return true;
        }

        internal UIntPtr ReadData(int scanStart, int scanCount)
        {
            if (scanStart < MinScan)
                scanStart = MinScan;

            if (scanStart + scanCount > MaxScan)
                scanCount = MaxScan - (scanStart + scanCount);

            if (scanCount < 0) return UIntPtr.Zero;

            var offset = fileHeader.HeaderSize + packetHeaderSize + (scanStart - 1) * (packetHeaderSize + scanDataRawSize);
            var bufferSize = scanCount * (packetHeaderSize + scanDataRawSize);
            fileMapper.MapBuffer(offset, bufferSize);
            
            return fileMapper.Buffer;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct DataFileHeader
        {
            [FieldOffset(0)] public ushort FileType;
            [FieldOffset(2)] public ushort DataCode;
            [FieldOffset(4)] public ushort HeaderSize;
            [FieldOffset(6)] public uint MinScanNumber;
            [FieldOffset(10)] public uint MaxScanNumber;
        }
    }
}

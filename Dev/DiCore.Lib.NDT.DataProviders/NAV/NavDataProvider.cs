using System;
using Diascan.Utils.FileMapper;
using DiCore.Lib.NDT.Types;

namespace DiCore.Lib.NDT.DataProviders.NAV
{
    public class NavDataProvider: IDataProvider, IOffsetAdjustProvider
    {
        private readonly unsafe int packetHeaderSize = sizeof(NavDataPacketHeader);
        //private readonly unsafe int scanDataRawSize = sizeof(NavScanDataRaw);

        private readonly unsafe int maxPacketSize = sizeof(NavDataPacketHeader) + sizeof(NavScanDataRaw);
        private int packetSize;

        private IndexFileManager IndexFile;

        private DataLocation DataLocation { get; set; }
        private DiagdataDescription DataDescription { get; }
        public bool IsOpened { get; private set; }
        public double SectionOffset => NavDeviceParameters.DeltaX / 1000f + (AdjustmentSectionOffsetValue ?? 0);
        public int MaxScan => IndexFile.MaxScan;
        public int MinScan => IndexFile.MinScan;
        public short SensorCount => 1;
        private NavDeviceParameters NavDeviceParameters { get; set; }
        public double? AdjustmentSectionOffsetValue { get; set; }
        public enNavType NavType { get; internal set; }

        public string Name => "Nav Data Provider";
        public string Description => "Implement reading Nav data";
        public string Vendor => "Gnetnev A.";
        
        public NavDataProvider(UIntPtr heap)
        {
            DataDescription = Constants.NVDataDescription;
        }

        public bool Open(DataLocation location)
        {
            IsOpened = false;
            DataLocation = location;
            NavDeviceParameters = (NavDeviceParameters)LoadParameters(DataLocation);

            IndexFile = new IndexFileManager(DataDescription, BuildDataPath(), FileMapperHelper.DefaultBufferSizeInByte, CalculateMaxScanSize());
            CheckPacketSize();
            NavType = NavDataProviderHelper.CheckNavType(IndexFile.GetDataFileType());

            IsOpened = true;

            return IsOpened;
        }

        private unsafe void CheckPacketSize()
        {
            var packetHeader = (NavDataPacketHeader*)IndexFile.GetDataPointer(IndexFile.FirstScan);
            packetSize = packetHeader->Size;
        }

        protected int CalculateMaxScanSize()
        {
            return maxPacketSize;
        }

        protected IDeviceParameters LoadParameters(DataLocation location)
        {
            return NavDeviceParameters.LoadFromOmni(location.FullPath);
        }

        protected string BuildDataPath()
        {
            return string.Concat(DataLocation.InspectionFullPath, @"\", DataLocation.BaseName, DataDescription.DataDirSuffix, DataLocation.BaseName);
        }

        private unsafe void FillDataBufferCol(NavScanData* scanData, NavScanDataRaw* baseData)
        {
            switch (packetSize)
            {
                case 84: // <-
                    NavDataProviderHelper.ConverRawToDataRdc(scanData, baseData, NavDeviceParameters.NavPrecision);
                    return;
                case 96:
                    NavDataProviderHelper.ConvertRawToData(scanData, baseData, NavDeviceParameters.NavPrecision);
                    return;
                case 128:
                    NavDataProviderHelper.ConverRawToDataExt(scanData, baseData, NavDeviceParameters.NavPrecision);
                    return;
            }
        }

        public unsafe DataHandle<NavScanData> GetNavData(int scanStart, int scanCount, int compressStep)
        {
            var result = new DataHandle<NavScanData>(SensorCount, scanCount);

            for (var i = 0; i < scanCount; i++)
            {
                var scanData = result.GetDataPointer(0, i);

                var scanOffset = IndexFile.GetDataPointer(i * compressStep + scanStart);

                if (scanOffset == UIntPtr.Zero) continue;

                //var packetHeader = (NavDataPacketHeader*)scanOffset;
                var baseData = (NavScanDataRaw*)(scanOffset + packetHeaderSize);

                FillDataBufferCol(scanData, baseData);
            }

            return result;
        }

        public unsafe NavScanData GetNavScanData(int scan)
        {
            var scanOffset = IndexFile.GetDataPointer(scan);
            if (scanOffset == UIntPtr.Zero)
                return NavScanData.Empty;
            
            var baseData = (NavScanDataRaw*)(scanOffset + packetHeaderSize);
            var scanData = new NavScanData();

            FillDataBufferCol(&scanData, baseData);

            return scanData;
        }

        private void InnerClose()
        {
            IndexFile?.Dispose();
            IsOpened = false;
        }

        public void Close()
        {
            if (IsOpened)
                InnerClose();
        }

        public void Dispose()
        {
            if (IsOpened)
                InnerClose();
        }
    }
}

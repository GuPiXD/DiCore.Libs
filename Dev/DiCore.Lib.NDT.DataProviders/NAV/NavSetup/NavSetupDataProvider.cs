using DiCore.Lib.NDT.Types;

namespace DiCore.Lib.NDT.DataProviders.NAV.NavSetup
{
    public class NavSetupDataProvider: IDataProvider
    {
        private readonly unsafe int maxPacketSize = sizeof(NavDataPacketHeader) + sizeof(NavScanDataRaw);
        private readonly SetupDataFile setupDataFile;

        public bool IsOpened { get; private set; }
        public double SectionOffset { get; }
        public int MaxScan => setupDataFile.MaxScan;
        public int MinScan => setupDataFile.MinScan;
        public short SensorCount => 1;
        public enNavType NavType { get; internal set; }

        public string Name => "Nav Setup Data Provider";
        public string Description => "Implement reading nav setup phase data";
        public string Vendor => "Gnetnev A.";

        public NavSetupDataProvider()
        {
            setupDataFile = new SetupDataFile();
        }

        public bool Open(DataLocation location)
        {
            IsOpened = setupDataFile.Open(location, Constants.NVSetupDataDescription);
            NavType = NavDataProviderHelper.CheckNavType(setupDataFile.NavTypeCode);
            return IsOpened;
        }

        public unsafe DataHandle<NavSetupScanData> GetNavSetupData(int scanStart, int scanCount)
        {
            var result = new DataHandle<NavSetupScanData>(SensorCount, scanCount);

            var scanDataPtr = result.Ptr;
            var baseDataPtr = setupDataFile.ReadData(scanStart, scanCount);

            for (var i = 0; i < scanCount; i++)
            {
                NavDataProviderHelper.ConverRawToSetupData(scanDataPtr, (NavScanDataRaw*)baseDataPtr);
                
                scanDataPtr++;
                baseDataPtr += maxPacketSize;
            }

            return result;
        }

        private void InnerClose()
        {
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

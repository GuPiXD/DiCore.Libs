using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Diascan.Utils.DataBuffers;
using DiCore.Lib.NDT.Carrier;
using DiCore.Lib.NDT.DataProviders.CDL;
using DiCore.Lib.NDT.DataProviders.CDM;
using DiCore.Lib.NDT.Types;

namespace DiCore.Lib.NDT.DataProviders.CDPA
{
    public class CDpaDataProvider : IDataProvider, IOffsetAdjustProvider
    {
        private UIntPtr Heap { get; }

        private DiagdataDescription DataDescription { get; }

        private readonly Dictionary<CDpaDirection, CDpaDirectionDataProvider> innerProviders = new Dictionary<CDpaDirection, CDpaDirectionDataProvider>();

        private CDpaDeviceParameters CDpaDeviceParameters { get; set; }

        private Carrier.Carrier Carrier { get; set; }

        public Action<int, int, int, float, SensorsByDeltaScanOrdering> CalcAligningSensors;
        private DataLocation location;
        

        public string Name => "CDPA Data Provider";
        public string Description => "Implement reading CDPA(DFR) data";
        public string Vendor => "Sharov V.Y.";
        public bool IsOpened { get; private set; }

        public int MaxScan { get; private set; }
        public int MinScan { get; private set; }
        
        public short SensorCount => Carrier?.SensorCount ?? 0;
        public double? AdjustmentSectionOffsetValue { get; set; }
        public double SectionOffset => CDpaDeviceParameters.DeltaX / 1000f + (AdjustmentSectionOffsetValue ?? 0);


        public CDpaDataProvider(UIntPtr heap)
        {
            DataDescription = Constants.CDPADataDescription;
            Heap = heap;
        }


        protected IDeviceParameters LoadParameters(string fullPath)
        {
            return CDpaDeviceParameters.LoadFromOmni(fullPath);
        }

        protected string BuildDataPath()
        {
            return String.Concat(location.InspectionFullPath, @"\", location.BaseName, DataDescription.DataDirSuffix, location.BaseName);
        }

        private Carrier.Carrier LoadCarrier()
        {
            var carrierLoader = new Loader();
            var carrier = carrierLoader.Load(CDpaDeviceParameters.CarrierId);

            if (carrier == null)
            {
                var localCarrierLoader = new LocalLoader();
                carrier = localCarrierLoader.Load(CDpaDeviceParameters.CarrierId);
            }

            return carrier;
        }

        private float GetEntryAngle(int directionId)
        {
            var anySensor = Carrier.FirstOrDefault(item => MathHelper.TestFloatEquals(item.Angle2, directionId));

            return anySensor?.Angle ?? 0f;
        }

        private void FillEntryAngles()
        {
            for (var i = 0; i < CDpaDeviceParameters.CDpaDirections.Count; i++)
            {
                var direction = CDpaDeviceParameters.CDpaDirections[i];
                direction.EntryAngle = GetEntryAngle(direction.Id);
                CDpaDeviceParameters.CDpaDirections[i] = direction;
            }
        }

        private bool DataNotPresent(int itemId)
        {
            var basePath = String.Concat(location.InspectionFullPath, Path.DirectorySeparatorChar, location.BaseName, DataDescription.DataDirSuffix);

            var currentPath = basePath.Replace("*", itemId.ToString());

            if (!Diascan.Utils.IO.Directory.Exists(currentPath)) return false;

            var pointerFileName = $"{currentPath}{Path.DirectorySeparatorChar}{location.BaseName}.cdp";

            return !Diascan.Utils.IO.File.Exists(pointerFileName);
        }

        private bool CheckRealDataPresent()
        {
            CDpaDeviceParameters.CDpaDirections.RemoveAll(item => DataNotPresent(item.Id));
            return CDpaDeviceParameters.CDpaDirections.Any();
        }

        private CDpaDirectionDataProvider GetDirectionDataProvider(CDpaDirection direction)
        {
            if (innerProviders.ContainsKey(direction))
                return innerProviders[direction];

            var provider = new CDpaDirectionDataProvider(Heap, direction, Carrier);
            provider.Open(location);
            provider.AdjustmentSectionOffsetValue = AdjustmentSectionOffsetValue;
            innerProviders.Add(direction, provider);
            return innerProviders[direction];
        }



        private void OpenAnyDirectionProvider()
        {
            var firstDirection = CDpaDeviceParameters.CDpaDirections.FirstOrDefault();
            var dirProvider = GetDirectionDataProvider(firstDirection);

            MinScan = dirProvider.MinScan;
            MaxScan = dirProvider.MaxScan;
        }

        public bool Open(DataLocation dataLocation)
        {
            IsOpened = false;

            location = dataLocation;

            CDpaDeviceParameters = (CDpaDeviceParameters)LoadParameters(location.FullPath);

            Carrier = LoadCarrier();
            if (Carrier == null)
                return IsOpened;

            FillEntryAngles();

            if (!CheckRealDataPresent())
                return IsOpened;

            OpenAnyDirectionProvider(); // ToDo: fix that shit 

            IsOpened = true;

            return IsOpened;
        }

        public DataHandleCDpa GetDirectionData(CDpaDirection direction, int scanStart, int countScan, int compressStep)
        {
            var directionDataProvider = GetDirectionDataProvider(direction); // ToDo: fix that shit 
            directionDataProvider.CalcAligningSensors = CalcAligningSensors;
            return directionDataProvider.GetData(scanStart, countScan, compressStep);
        }

        public IEnumerable<DataHandleCDpa> GetCDpaData(enDirectionName directionName, int scanStart, int countScan, int compressStep = 1)
        {
            var directions = CDpaDeviceParameters.CDpaDirections.FindAll(item => directionName.HasFlag(item.DirectionName));

            foreach (var direction in directions)
                yield return GetDirectionData(direction, scanStart, countScan, compressStep);
        }

        private void InnerClose()
        {
            foreach (var directionDataProvider in innerProviders.Values)
                directionDataProvider.Dispose();

            innerProviders.Clear();
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

        public List<CDpaDirection> GetDirections()
        {
            return CDpaDeviceParameters?.CDpaDirections;
        }

        public float SensorToAngle(short sensor, double dist, ICoordinateProvider cdp)
        {
            return innerProviders.Values.FirstOrDefault()?.SensorToAngle(sensor, dist, cdp) ?? -1f;
        }

        public short AngleToSensor(float angle, double dist, ICoordinateProvider cdp)
        {
            return innerProviders.Values.FirstOrDefault()?.AngleToSensor(angle, dist, cdp) ?? -1;
        }

        public int SensorIndexToSensor(int sensorIndex, CDpaDirection direction)
        {
            var dirDataProvider = GetDirectionDataProvider(direction); // ToDo: fix that shit 
            return dirDataProvider.SensorIndexByDirectionToSensor((short)sensorIndex);
        }

        public int SensorToSensorIndex(int sensor, CDpaDirection direction)
        {
            var dirDataProvider = GetDirectionDataProvider(direction); // ToDo: fix that shit 
            return dirDataProvider.SensorToSensorIndexByDirection((short)sensor);
        }
    }
}
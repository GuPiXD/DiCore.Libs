using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using DiCore.Lib.NDT.Carrier;
using DiCore.Lib.NDT.Types;

namespace DiCore.Lib.NDT.DataProviders.CDM
{
    public class CdmDataProvider : IDataProvider, IOffsetAdjustProvider
    {
        private UIntPtr Heap { get; }
        private DiagdataDescription DataDescription { get; }
        
        private readonly Dictionary<CdmDirection, DirectionDataProvider> innerProviders = new Dictionary<CdmDirection, DirectionDataProvider>();

        private CdmDeviceParameters CdmDeviceParameters { get; set; }

        private Carrier.Carrier Carrier { get; set; }

        public CdmDataProvider(UIntPtr heap)
        {
            DataDescription = Constants.CD360DataDescription;
            Heap = heap;
        }

        public Action<int, int, int, float, SensorsByDeltaScanOrdering> CalcAligningSensors;
        private DataLocation location;

        public string Name => "Cdm(360, DKP) Data Provider";
        public string Description => "Implement reading Cdm(360, DKP) data";
        public string Vendor => "Vagner I.";
        public bool IsOpened { get; private set; }
        public int MinScan { get; private set; }
        public short SensorCount => Carrier?.SensorCount ?? 0;
        public int MaxScan { get; private set; }

        private IDeviceParameters LoadParameters(string fullPath)
        {
            return CdmDeviceParameters.LoadFromOmni(fullPath);
        }

        private string BuildDataPath()
        {
            return String.Concat(location.InspectionFullPath, @"\", location.BaseName, DataDescription.DataDirSuffix, location.BaseName);
        }


        public bool Open(DataLocation dataLocation)
        {
            IsOpened = false;

            location = dataLocation;

            CdmDeviceParameters = (CdmDeviceParameters)LoadParameters(location.FullPath);

            Carrier = LoadCarrier();

            if (Carrier == null)
                return false;

            FillEntryAngles();

            if (!CheckRealDataPresent())
                return false;

            OpenAnyDirectionProvider();

            IsOpened = true;

            return true;
        }

        private void FillEntryAngles()
        {
            for (var i = 0; i < CdmDeviceParameters.Directions.Count; i++)
            {
                var direction = CdmDeviceParameters.Directions[i];
                direction.EntryAngle = GetEntryAngle(direction.Id);
                CdmDeviceParameters.Directions[i] = direction;
            }
        }

        private void OpenAnyDirectionProvider()
        {
            var direction = CdmDeviceParameters.Directions.FirstOrDefault();

            var provider = GetDirectionDataProvider(direction);

            MinScan = provider.MinScan;
            MaxScan = provider.MaxScan;
        }

        private DirectionDataProvider GetDirectionDataProvider(CdmDirection direction)
        {
            if (innerProviders.ContainsKey(direction))
                return innerProviders[direction];

            var provider = new DirectionDataProvider(Heap, direction);
            provider.Open(location);
            provider.AdjustmentSectionOffsetValue = AdjustmentSectionOffsetValue;

            innerProviders.Add(direction, provider);

            return provider;
        }

        public void Close()
        {
            if (IsOpened)
                InnerClose();
        }

        private Carrier.Carrier LoadCarrier()
        {
            var carrierLoader = new Loader();
            var carrier = carrierLoader.Load(CdmDeviceParameters.CarrierId);

            if (carrier == null)
            {
                var localCarrierLoader = new LocalLoader();
                carrier = localCarrierLoader.Load(CdmDeviceParameters.CarrierId);
            }

            return carrier;
        }

        private bool CheckRealDataPresent()
        {
            CdmDeviceParameters.Directions.RemoveAll(item => DataNotPresent(item.Id));
            return CdmDeviceParameters.Directions.Any();
        }

        private bool DataNotPresent(int itemId)
        {
            var basePath = String.Concat(location.InspectionFullPath, Path.DirectorySeparatorChar,
                location.BaseName, DataDescription.DataDirSuffix);

            var currentPath = basePath.Replace("*", itemId.ToString());

            if (!Diascan.Utils.IO.Directory.Exists(currentPath)) return false;

            var pointerFileName = $"{currentPath}{Path.DirectorySeparatorChar}{location.BaseName}.cdp";

            return !Diascan.Utils.IO.File.Exists(pointerFileName);
        }

        private void InnerClose()
        {
            foreach (var directionDataProvider in innerProviders.Values)
                directionDataProvider.Dispose();

           innerProviders.Clear();
        }

        public IEnumerable<DataHandleCdm> GetCdmData(enDirectionName directionName, int scanStart, int countScan, int compressStep = 1)
        {
            var directions = CdmDeviceParameters.Directions.FindAll(item => directionName.HasFlag(item.DirectionName));

            foreach (var direction in directions)
            {
                yield return GetDirectionData(direction, scanStart, countScan, compressStep);
            }
        }

        public DataHandleCdm GetDirectionData(CdmDirection direction, int scanStart, int countScan, int compressStep)
        {
            var directionDataProvider = GetDirectionDataProvider(direction);
            directionDataProvider.CalcAligningSensors = CalcAligningSensors;
            return directionDataProvider.GetData(scanStart, countScan, compressStep);
        }
        
        public void Dispose()
        {
            if (IsOpened)
                InnerClose();
        }

        public double SectionOffset => CdmDeviceParameters.DeltaX / 1000f + (AdjustmentSectionOffsetValue ?? 0);
  
        public double? AdjustmentSectionOffsetValue { get; set; }

        private float GetEntryAngle(int directionId)
        {
            var anySensor = Carrier.FirstOrDefault(item => MathHelper.TestFloatEquals(item.Angle2, directionId));

            return anySensor?.Angle ?? 0f;
        }

        public List<CdmDirection> GetDirections()
        {
            return CdmDeviceParameters?.Directions;
        }

        public float SensorToAngle(short sensor, double dist, ICoordinateProvider cdp)
        {
            return innerProviders.Values.FirstOrDefault()?.SensorToAngle(sensor, dist, cdp) ?? -1f;
        }

        public short AngleToSensor(float angle, double dist, ICoordinateProvider cdp)
        {
            return innerProviders.Values.FirstOrDefault()?.AngleToSensor(angle, dist, cdp) ?? -1;
        }

        public int SensorIndexToSensor(int sensorIndex, CdmDirection direction)
        {
            var dirDataProvider = GetDirectionDataProvider(direction);
            return dirDataProvider.SensorIndexByDirectionToSensor((short)sensorIndex);
        }

        public int SensorToSensorIndex(int sensor, CdmDirection direction)
        {
            var dirDataProvider = GetDirectionDataProvider(direction);
            return dirDataProvider.SensorToSensorIndexByDirection((short) sensor);
        }
    }
}

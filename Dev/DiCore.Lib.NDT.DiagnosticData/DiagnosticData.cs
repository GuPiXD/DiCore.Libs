using System;
using System.Collections.Generic;
using System.IO;
using Diascan.NDT.Enums;
using Diascan.Utils.DataBuffers;
using DiCore.Lib.NDT.CoordinateProvider;
using DiCore.Lib.NDT.DataProviders;
using DiCore.Lib.NDT.DataProviders.CDL;
using DiCore.Lib.NDT.DataProviders.CDM;
using DiCore.Lib.NDT.DataProviders.CDPA;
using DiCore.Lib.NDT.DataProviders.EMA;
using DiCore.Lib.NDT.DataProviders.MFL;
using DiCore.Lib.NDT.DataProviders.MPM;
using DiCore.Lib.NDT.DataProviders.NAV;
using DiCore.Lib.NDT.DataProviders.NAV.NavSetup;
using DiCore.Lib.NDT.DataProviders.WM;
using DiCore.Lib.NDT.Types;

namespace DiCore.Lib.NDT.DiagnosticData
{
    public partial class DiagnosticData: IDisposable
    {
        private bool isOpen;
        private DataLocation dataLocation;
        private bool coordinateDataAvailable;
        private UIntPtr heap;
        private bool disposed;
        private DataType availableDataTypes;
        private CoordinateDataProviderCrop coordinateDataProvider;

        public bool IsOpen => isOpen;

        public DataType AvailableDataTypes => availableDataTypes;

        private Dictionary<DataType, IDataProvider> DataProviders { get; set; }

        public CoordinateDataProviderCrop CoordinateDataProvider => coordinateDataProvider;

        public bool Open(DataLocation dataLocat)
        {
            if (isOpen)
                Clear();

            dataLocation = dataLocat;
            isOpen = CheckDataLocation(dataLocation);
            if (isOpen)
            {
                isOpen = coordinateDataAvailable = TestDataHelper.TestCoordinateData(dataLocation);
                availableDataTypes = TestDataAvailable(dataLocation);
            }

            if (!isOpen) return false;

            DataProviders = new Dictionary<DataType, IDataProvider>();
            heap = BufferHelper.CreateHeap(0);

            CreateCoordinateDataProviders();
            CreateWmDataProvider();
            
            if (DataProviders.ContainsKey(DataType.Wm))
            {
                var wmDataProvider = DataProviders[DataType.Wm];
                wmDataProvider.Open(dataLocation);
                DataHelper.AdjustmentOffsetProviders(DataProviders.Values);
            }

            return isOpen;
        }

        private void CreateCoordinateDataProviders()
        {
            if (!coordinateDataAvailable) return;

            coordinateDataProvider = new CoordinateDataProviderCrop(heap, CalcParameters.LoadFromOmni(dataLocation.FullPath));
            coordinateDataProvider.Open(dataLocation);
            DataProviders.Add(DataType.None, coordinateDataProvider);
        }
        

        private static bool CheckDataLocation(DataLocation dataLocation)
        {
            var result = !String.IsNullOrEmpty(dataLocation.BaseFile);

            result = result && Path.GetExtension(dataLocation.BaseFile) != "omni";

            result = result && !String.IsNullOrEmpty(dataLocation.FullPath);

            return result;
        }

        private static DataType TestDataAvailable(DataLocation dataLocation)
        {
            if (!CheckDataLocation(dataLocation)) return DataType.None;
            
            return TestDataHelper.TestDataType(dataLocation);
        }

        public static List<DataType> GetAvailableDataTypes(DataLocation dataLocation)
        {
            var result = new List<DataType>();
            
            var dataTypes = TestDataAvailable(dataLocation);

            if ((int)dataTypes == 0) return result;

            foreach (DataType type in Enum.GetValues(typeof(DataType)))
            {
                if (dataTypes.HasFlag(type) && (int)type > 0)
                    result.Add(type);
            }
            return result;
        }

        public void Dispose()
        {
            if (disposed)
                return;

            Clear();

            disposed = true;
        }

        private void Clear()
        {
            DisposeDataProviders();
            BufferHelper.DestroyHeap(heap);
            heap = UIntPtr.Zero;
            dataLocation = null;
        }

        private void DisposeDataProviders()
        {
            foreach (var provider in DataProviders.Values)
                provider.Dispose();

            DataProviders.Clear();
        }

        private bool CreateWmDataProvider()
        {
            if (IsOpen && availableDataTypes.HasFlag(DataType.Wm))
            {
                var provider = new WmDataProvider(heap);
                DataProviders.Add(DataType.Wm, provider);
                return true;
            }
            return false;
        }
        private bool CreateMflDataProvider(DataType dataType)
        {
            if (IsOpen && availableDataTypes.HasFlag(dataType))
            {
                var provider = MflProviderFactory.Create(heap, dataType);
                if (provider != null)
                    DataProviders.Add(dataType, provider);
                return true;
            }
            return false;
        }
        private bool CreateCdlDataProvider()
        {
            if (IsOpen && availableDataTypes.HasFlag(DataType.Cdl))
            {
                var provider = new CdlDataProvider(heap);
                DataProviders.Add(DataType.Cdl, provider);
                return true;
            }
            return false;
        }
        private bool CreateCdmDataProvider()
        {
            if (IsOpen && availableDataTypes.HasFlag(DataType.Cd360))
            {
                var provider = new CdmDataProvider(heap);
                DataProviders.Add(DataType.Cd360, provider);
                return true;
            }
            return false;
        }

        private bool CreateCDpaDataProvider()
        {
            if (IsOpen && availableDataTypes.HasFlag(DataType.CDPA))
            {
                var provider = new CDpaDataProvider(heap);
                DataProviders.Add(DataType.CDPA, provider);
                return true;
            }
            return false;
        }

        private bool CreateMpmDataProvider()
        {
            if (IsOpen && availableDataTypes.HasFlag(DataType.Mpm))
            {
                var provider = new MpmDataProvider(heap);
                DataProviders.Add(DataType.Mpm, provider);
                return true;
            }
            return false;
        }

        private bool CreateNavDataProvider()
        {
            if (IsOpen && availableDataTypes.HasFlag(DataType.Nav))
            {
                var provider = new NavDataProvider(heap);
                DataProviders.Add(DataType.Nav, provider);
                return true;
            }
            return false;
        }

        private bool CreateNavSetupDataProvider()
        {
            if (IsOpen && availableDataTypes.HasFlag(DataType.NavSetup))
            {
                var provider = new NavSetupDataProvider();
                DataProviders.Add(DataType.NavSetup, provider);
                return true;
            }
            return false;
        }

        private bool CreateEmaDataProvider()
        {
            if (IsOpen && availableDataTypes.HasFlag(DataType.Ema))
            {
                var provider = new EmaDataProvider(heap);
                DataProviders.Add(DataType.Ema, provider);
                return true;
            }
            return false;
        }

        private bool CreateDataProvider(DataType dataType)
        {
            switch (dataType)
            {
                case DataType.Mpm:
                    return CreateMpmDataProvider();
                case DataType.Wm:
                    return CreateWmDataProvider();
                case DataType.MflT1:
                case DataType.MflT11:
                case DataType.MflT2:
                case DataType.MflT22:
                case DataType.MflT3:
                case DataType.MflT31:
                case DataType.MflT32:
                case DataType.MflT33:
                case DataType.MflT34:
                case DataType.TfiT4:
                case DataType.TfiT41:
                    return CreateMflDataProvider(dataType);
                case DataType.Cdl:
                    return CreateCdlDataProvider();
                case DataType.Cd360:
                    return CreateCdmDataProvider();
                case DataType.CDPA:
                    return CreateCDpaDataProvider();
                case DataType.Nav:
                    return CreateNavDataProvider();
                case DataType.NavSetup:
                    return CreateNavSetupDataProvider();
                case DataType.Ema:
                    return CreateEmaDataProvider();
                default:
                    throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null);
            }
        }


        public Range<double> GetDistanceRange(DataType dataType)
        {
            if (!IsOpen || !availableDataTypes.HasFlag(dataType))
                return new Range<double>(0, 0);
            
            var dataProvider = GetDataProvider(dataType);

            return new Range<double>(
                coordinateDataProvider.Scan2Dist(dataProvider.MinScan),
                coordinateDataProvider.Scan2Dist(dataProvider.MaxScan));
        }

        public short GetSensorCount(DataType dataType)
        {
            if (!IsOpen || !availableDataTypes.HasFlag(dataType))
            {
                return 0;
            }

            var dataProvider = GetDataProvider(dataType);
            return dataProvider.SensorCount;
        }

        public List<CdmDirection> GetCdmDirections()
        {
            if (!IsOpen || !availableDataTypes.HasFlag(DataType.Cd360))
                return new List<CdmDirection>();

            var cdmDataProvider = (CdmDataProvider)GetDataProvider(DataType.Cd360);
            return cdmDataProvider?.GetDirections();
        }

        public Dictionary<int, List<int>> GetMflSensorsByGroup(DataType dataType)
        {
            if (!IsOpen || !availableDataTypes.HasFlag(dataType))
                return new Dictionary<int, List<int>>();

            var mflDataProvider = (MflDataProvider)GetDataProvider(dataType);
            return mflDataProvider.GetSensorsByGroup();
        }

        public float GetCdlSensorRayDirection(int sensorIndex)
        {
            if (!IsOpen || !availableDataTypes.HasFlag(DataType.Cdl)) return -1f;

            var cdlDataProvider = (CdlDataProvider) GetDataProvider(DataType.Cdl);

            return cdlDataProvider.GetSensorRayDirection(sensorIndex);
        }

        public float CdmSensorToAngle(short sensor, double dist)
        {
            if (!IsOpen || !availableDataTypes.HasFlag(DataType.Cd360) || !coordinateDataAvailable)
                return -1f;

            var cdmDataProvider = (CdmDataProvider) GetDataProvider(DataType.Cd360);
            
            return cdmDataProvider.SensorToAngle(sensor, dist, CoordinateDataProvider);
        }

        public short CdmAngleToSensor(float angle, double dist)
        {
            if (!IsOpen || !availableDataTypes.HasFlag(DataType.Cd360) || !coordinateDataAvailable)
                return -1;

            var cdmDataProvider = (CdmDataProvider)GetDataProvider(DataType.Cd360);

            return cdmDataProvider.AngleToSensor(angle, dist, CoordinateDataProvider);
        }

        public float CdlSensorToAngle(short sensor, double dist)
        {
            if (!IsOpen || !availableDataTypes.HasFlag(DataType.Cdl) || !coordinateDataAvailable)
                return -1f;

            var cdlDataProvider = (CdlDataProvider)GetDataProvider(DataType.Cdl);

            return cdlDataProvider.SensorToAngle(sensor, dist, CoordinateDataProvider);
        }

        public int CdmSensorIndexToSensor(int sensorIndex, CdmDirection direction)
        {
            if (!IsOpen || !availableDataTypes.HasFlag(DataType.Cd360)) return -1;

            var cdmDataProvider = (CdmDataProvider) GetDataProvider(DataType.Cd360);

            return cdmDataProvider.SensorIndexToSensor(sensorIndex, direction);
        }

        public List<CDpaDirection> GetCDpaDirections()
        {
            if (!IsOpen || !availableDataTypes.HasFlag(DataType.CDPA))
                return new List<CDpaDirection>();

            var cdpaDataProvider = (CDpaDataProvider)GetDataProvider(DataType.CDPA);
            return cdpaDataProvider?.GetDirections();
        }

        public float CDpaSensorToAngle(short sensor, double dist)
        {
            if (!IsOpen || !availableDataTypes.HasFlag(DataType.CDPA) || !coordinateDataAvailable)
                return -1f;

            var cdpaDataProvider = (CDpaDataProvider)GetDataProvider(DataType.CDPA);

            return cdpaDataProvider.SensorToAngle(sensor, dist, CoordinateDataProvider);
        }

        public short CDpaAngleToSensor(float angle, double dist)
        {
            if (!IsOpen || !availableDataTypes.HasFlag(DataType.CDPA) || !coordinateDataAvailable)
                return -1;

            var cdpaDataProvider = (CDpaDataProvider)GetDataProvider(DataType.CDPA);

            return cdpaDataProvider.AngleToSensor(angle, dist, CoordinateDataProvider);
        }

        public int CDpaSensorIndexToSensor(int sensorIndex, CDpaDirection direction)
        {
            if (!IsOpen || !availableDataTypes.HasFlag(DataType.CDPA)) return -1;

            var cdpaDataProvider = (CDpaDataProvider)GetDataProvider(DataType.CDPA);

            return cdpaDataProvider.SensorIndexToSensor(sensorIndex, direction);
        }
        public float SensorToAngle(short sensor, double dist, DataType dataType)
        {
            if (!IsOpen || !availableDataTypes.HasFlag(dataType) || !coordinateDataAvailable)
                return -1f;

            var dataProvider = (BaseDataProvider)GetDataProvider(dataType);

            return dataProvider.SensorToAngle(sensor, dist, CoordinateDataProvider);
        }

        public float GetCdlEntryAngle()
        {
            if (!IsOpen || !availableDataTypes.HasFlag(DataType.Cdl)) return -1f;

            var cdlDataProvider = (CdlDataProvider) GetDataProvider(DataType.Cdl);

            return cdlDataProvider.GetEntryAngle();
        }

        public int SensorIndexToSensor(int sensorIndex, DataType dataType)
        {
            if (!IsOpen || !availableDataTypes.HasFlag(dataType)) return -1;

            var dataProvider = (BaseDataProvider) GetDataProvider(dataType);

            return dataProvider.SensorIndex2Sensor((short) sensorIndex);
        }

        public enNavType GetNavType()
        {
            if (!IsOpen || !availableDataTypes.HasFlag(DataType.Nav) || 
                !availableDataTypes.HasFlag(DataType.NavSetup)) return enNavType.None;

            var navType = ((NavDataProvider)GetDataProvider(DataType.Nav))?.NavType ?? 
                          ((NavSetupDataProvider)GetDataProvider(DataType.NavSetup)).NavType;

            return navType;
        }

        public Range<int> GetScanRange(DataType dataType)
        {
            if (!IsOpen || !availableDataTypes.HasFlag(dataType)) return new Range<int>(0, 0);

            var dataProvider = GetDataProvider(dataType);
            return new Range<int>(dataProvider.MinScan, dataProvider.MaxScan);
        }

        /// <summary>
        /// Преобразование дистанции в физически скан с учетом смещения секции (SectionOffset)        
        /// </summary>
        /// <param name="dist">целевая дистанция, м</param>
        /// <returns>физический скан диагностических данных (в пространстве провайдера данных)</returns>
        public int Dist2PhysScan(double dist, DataType dataType)
        {
            if (!IsOpen || !availableDataTypes.HasFlag(dataType) || !coordinateDataAvailable)
                return -1;

            var dataProvider = (BaseDataProvider)GetDataProvider(dataType);

            return CoordinateDataProvider.Dist2Scan(dist + dataProvider.SectionOffset);
        }

        public short AngleToSensor(float angle, double dist, DataType dataType)
        {
            if (!IsOpen || !availableDataTypes.HasFlag(dataType) || !coordinateDataAvailable)
                return -1;

            var dataProvider = (BaseDataProvider)GetDataProvider(dataType);

            return dataProvider.AngleToSensor(angle, dist, CoordinateDataProvider);
        }
    }
}

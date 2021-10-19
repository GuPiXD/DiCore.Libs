using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Xml.Linq;
using Diascan.Utils.DataBuffers;
using Diascan.Utils.FileMapper;
using DiCore.Lib.NDT.Carrier;

namespace DiCore.Lib.NDT.Types
{
    public abstract class BaseDataProvider : IDataProvider, IOffsetAdjustProvider
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string Vendor { get; }

        protected DataLocation Location { get; private set; }
        private string dataPath;

        protected Carrier.Carrier Carrier;

        public Action<int, int, int, float, SensorsByDeltaScanOrdering> CalcAligningSensors;

        protected SensorsByDeltaScanOrdering SensorsByDeltaScanOrdering;

        protected IndexFileManager IndexFile;

        protected float AlignmentFactor { get; set; } = 1f;

        /// <summary>
        /// Список физических номеров сканов в буфере чтения
        /// </summary>
        private int[] readScans = new int[0];

        /// <summary>
        /// Тип файлов
        /// </summary>
        public ushort FileType => IndexFile.FileType;

        /// <summary>
        /// Максимальный скан в данных данного типа
        /// </summary>
        public int MaxScan => IndexFile.MaxScan;

        /// <summary>
        /// Минимальный скан в данных данного типа
        /// </summary>
        public int MinScan => IndexFile.MinScan;

        public virtual double SectionOffset => Parameters.DeltaX / 1000f + (AdjustmentSectionOffsetValue ?? 0);
        /// <summary>
        /// Идентификатор прибора
        /// </summary>
        public virtual int CarrierId => Parameters.CarrierId;

        public double? AdjustmentSectionOffsetValue { get; set; }

        protected IDeviceParameters Parameters;
        protected readonly UIntPtr Heap;
        protected readonly DiagdataDescription DataDescription;
        private readonly ParallelOptions parallelReadOptions;

        private OrderablePartitioner<Tuple<int, int>> readScansRangePartitioner;

        private static uint DataBufferSize => FileMapperHelper.DefaultBufferSizeInByte;

        private const int MaxDegreeOfReadParallelism = 2;

        protected VectorBuffer<SensorAligningItem> SensorAligningItems;

        protected VectorBuffer<short> SensorAligningItemIndexesByIndexOnRing;

        public short SensorCount { get; protected set; }

        private float sensorTDCRingLocation;

        public float SensorTDCRingLocation => IsOpened ? sensorTDCRingLocation : 0;
        protected virtual int SensorTDC => (Parameters.SensorTDC - 1 + Carrier.SensorCount) % Carrier.SensorCount;

        protected BaseDataProvider(UIntPtr heap, DiagdataDescription dataDescription)
        {
            Heap = heap;
            DataDescription = dataDescription;
            parallelReadOptions = new ParallelOptions {MaxDegreeOfParallelism = MaxDegreeOfReadParallelism};
            CalcAligningSensors = InternalCalcAligningSensors;
        }

        private void InternalCalcAligningSensors(int scanStart, int scanCount, int compressStep,
            float addAlignmentFactor, SensorsByDeltaScanOrdering destSensorsByDeltaScanOrdering)
        {
            if (!IsOpened)
                return;

            var averageOdometer = Parameters.OdoFactor * 1000;

            if (MathHelper.TestFloatEquals(addAlignmentFactor, 1, 0.001))
                addAlignmentFactor = 1;

            foreach (var sensor in destSensorsByDeltaScanOrdering.Items)
            {
                var dy = sensor.Dy * addAlignmentFactor;

                sensor.DeltaScan = (short) Math.Round(dy / averageOdometer, MidpointRounding.AwayFromZero);
                sensor.AdditionalAligmentInputInDeltaScan =
                    (short)
                    (Math.Round(dy * (addAlignmentFactor - 1) / averageOdometer,
                        MidpointRounding.AwayFromZero));
            }
        }

        protected virtual unsafe short SensorToSensorIndex(short sensor)
        {
            var sensors = (SensorAligningItem*) SensorAligningItems.Data;
            if (Parameters.IsReverse)
                sensor = (short) ((SensorCount - sensor) % SensorCount);
            return sensors[sensor].IndexOnRing;
        }

        protected virtual unsafe short SensorIndexToSensor(short sensorIndex)
        {
            if (Parameters.IsReverse)
                sensorIndex = (short)((SensorCount - sensorIndex) % SensorCount);
            var sensors = (short*)SensorAligningItemIndexesByIndexOnRing.Data;
            return sensors[sensorIndex];
        }

        public short SensorIndex2Sensor(short sensorIndex)
        {
            return SensorIndexToSensor(sensorIndex);
        }

        public virtual unsafe bool Open(DataLocation location)
        {
            Location = location;
            dataPath = BuildDataPath();

            Parameters = LoadParameters(location);
            
            if( Carrier == null )
                Carrier = LoadCarrier(location.BaseDirectory);

            if (Carrier == null)
                return false;

            SensorsByDeltaScanOrdering = new SensorsByDeltaScanOrdering(Carrier);

            SensorCount = Carrier.SensorCount;
            SensorAligningItems = new VectorBuffer<SensorAligningItem>(Heap, SensorCount, 1);
            SensorAligningItems.InitSensorAligningMap(Carrier, (float)Math.Round(Parameters.PipeDiameterMm * Math.PI, 3, MidpointRounding.AwayFromZero), Parameters.IsReverse);

            SensorAligningItemIndexesByIndexOnRing = new VectorBuffer<short>(Heap, SensorCount, 1);
            SensorAligningItems.FillSensorAligningIndexesByIndexOnRing(SensorAligningItemIndexesByIndexOnRing);

            IndexFile = new IndexFileManager(DataDescription, dataPath, DataBufferSize, CalculateMaxScanSize());

            sensorTDCRingLocation = ((SensorAligningItem*) SensorAligningItems.Data)[SensorTDC].DeltaX;

            IsOpened = true;
            return IsOpened;
        }

        private Carrier.Carrier LoadCarrier(string basePath)
        {
            var carrierLoader = new Loader();
            var carrier = carrierLoader.Load(Parameters.CarrierId);

            if (carrier == null)
            {
                try
                {
                    var localCarrierLoader = new LocalLoader();
                    carrier = localCarrierLoader.Load(Parameters.CarrierId);
                }
                catch (Exception e)
                {
                    // при вызове из веб данной библиотеки нет и прибор данным образом не загружаем
                    if(!e.Message.StartsWith("Could not load file or assembly 'System.Windows.Forms"))
                        throw;
                }
            }

            return carrier ?? (carrier = GetFromXml(Parameters.CarrierId, basePath));
        }

        private Carrier.Carrier GetFromXml(int oldId, string path)
        {
            var xmlFile = $"{path}\\CARRIERS\\{oldId}.xml";
            if (!System.IO.File.Exists(xmlFile)) return null;
            var xml = XElement.Load(xmlFile);
            var carrierXml = xml;
            if (carrierXml.Name != "Carrier") return null;

            var ic = CultureInfo.InvariantCulture;
            var carrier = new Carrier.Carrier(Guid.NewGuid());
            carrier.OldId = Convert.ToInt32(carrierXml.Attribute("ID").Value);
            carrier.Circle = Convert.ToSingle(carrierXml.Attribute("Circle").Value, ic);
            carrier.Description = carrierXml.Attribute("Description").Value;
            carrier.Name = carrierXml.Attribute("Name").Value;
            carrier.PigType = Convert.ToByte(carrierXml.Attribute("PigType").Value);
            carrier.Diametr =
                Convert.ToByte((carrierXml.Attributes().FirstOrDefault(item => item.Name == "Diametr") ??
                                carrierXml.Attribute("Diameter")).Value);
                

            var sensorsXml = carrierXml.Elements("Sensor").ToArray();
            var sensors = new Sensor[sensorsXml.Length];
            var i = 0;

            int primary;
            foreach (var sensor in sensorsXml.Select(element => new Sensor
            {
                Primary = Convert.ToBoolean(Int32.TryParse(element.Attribute("Primary").Value, out primary) ? (primary == 1 ? "true" : "false") : element.Attribute("Primary").Value),
                LogicalNumber = Convert.ToInt16(element.Attribute("LogicalNumber").Value),
                PhysicalNumber = Convert.ToInt16(element.Attribute("PhysicalNumber").Value),
                OpposedLogicalNumber = Convert.ToInt16(element.Attribute("OpposedLogicalNumber").Value),
                GroupNumber = Convert.ToInt16(element.Attribute("GroupNumber").Value),
                SkiNumber = Convert.ToInt16(element.Attribute("SkiNumber").Value),
                Delay = Convert.ToInt32(element.Attribute("Delay").Value),
                Dx = (float)Math.Round(Convert.ToSingle(element.Attribute("Dx").Value, ic), 2, MidpointRounding.AwayFromZero),
                Dy = (float)Math.Round(Convert.ToSingle(element.Attribute("Dy").Value, ic), 2, MidpointRounding.AwayFromZero),
                Angle = (float)Math.Round(Convert.ToSingle(element.Attribute("Angle").Value, ic), 1, MidpointRounding.AwayFromZero),
                Angle2 = (float)Math.Round(Convert.ToSingle(element.Attribute("Angle2").Value, ic), 1, MidpointRounding.AwayFromZero),
                Bodynum = Convert.ToInt32(element.Attribute("Bodynum").Value)
            }))
            {
                sensors[i++] = sensor;
            }

            ReflectionSetField(typeof(Carrier.Carrier), carrier, "m_sensors", sensors);
            return carrier;
        }


        public static void ReflectionSetField(Type entityType, object entity, string fieldName, object value)
        {
            var fi = entityType.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Debug.Assert(fi != null);

            fi.SetValue(entity, value);
        }


        protected abstract int CalculateMaxScanSize();

        protected abstract IDeviceParameters LoadParameters(DataLocation location);

        public void Close()
        {
            if (IsOpened)
                InnerClose();
        }

        protected virtual void InnerClose()
        {
            IndexFile?.Dispose();
            SensorAligningItems?.Dispose();
            SensorAligningItemIndexesByIndexOnRing?.Dispose();
            IsOpened = false;
        }

        public virtual bool IsOpened { get; protected set; }

        protected abstract string BuildDataPath();

        public virtual void Dispose()
        {
            if (IsOpened)
                InnerClose();
        }
        protected void ExecuteBaseReadScans(Action<IDiagDataReader, int, int, object> readAction, object args, bool multiThread)
        {
            if (multiThread)
                Parallel.ForEach(readScansRangePartitioner, parallelReadOptions, () => IndexFile.TakeDataReader(),
                    (range, loopstate, dataReader) =>
                    {
                        for (int i = range.Item1; i < range.Item2; i++)
                            readAction(dataReader, readScans[i], i, args);

                        return dataReader;
                    },
                    dataReader => IndexFile.RetractDataReader(dataReader)
                );
            else
                for (var i = 0; i < readScans.Length; i++)
                    readAction(IndexFile.dataReader, readScans[i], i, args);
        }

        protected void DefineReadScans(int scanStart, int scanCount, int compressStep)
        {
            Array.Resize(ref readScans, scanCount);
            Array.Clear(readScans, 0, scanCount);

            var currentScan = scanStart;

            for (var i = 0; i < scanCount; i++)
            {
                readScans[i] = currentScan;
                currentScan += compressStep;
            }

            readScansRangePartitioner = Partitioner.Create(0, readScans.Length,
                (int) Math.Ceiling(Math.Max((double) readScans.Length / MaxDegreeOfReadParallelism, 1.0)));
        }

        protected void ReCreateIndexFileManager()
        {
            IndexFile?.Dispose();

            IndexFile = new IndexFileManager(DataDescription, dataPath, DataBufferSize, CalculateMaxScanSize());
        }

        public unsafe float SensorToAngle(short sensor, double dist, ICoordinateProvider coordinateProvider)
        {
            var sensorAligningItemsPtr = (SensorAligningItem*)SensorAligningItems.Data;
            var sensorTDCDeltaX = sensorAligningItemsPtr[SensorTDC].DeltaX;
            var sensorBaseAngle = sensorAligningItemsPtr[sensor].DeltaX;
            return coordinateProvider.GetAngle(sensorBaseAngle, sensorTDCDeltaX, dist);
        }

        public unsafe short AngleToSensor(float angle, double dist, ICoordinateProvider coordinateProvider)
        {
            var sensorAligningItemsPtr = (SensorAligningItem*)SensorAligningItems.Data;
            var sensorTDCDeltaX = sensorAligningItemsPtr[SensorTDC].DeltaX;

            var carrierCircle = Parameters.PipeCircle;

            var angleOffset = coordinateProvider.AngleOffset(sensorTDCDeltaX, dist);

            var mmInDegree = carrierCircle / 360f;

            var targetAngleMm = (angle * mmInDegree - angleOffset + carrierCircle) % carrierCircle;
            
            short index = 0;
            var halfCircle = carrierCircle / 2f;
            var delta = Math.Abs(sensorAligningItemsPtr[index].DeltaX - targetAngleMm);
            if (delta > halfCircle)
                delta = carrierCircle - delta;
            for (short i = 1; i < SensorAligningItems.Count; i++)
            {
                var x = Math.Abs(sensorAligningItemsPtr[i].DeltaX - targetAngleMm);
                if (x > halfCircle)
                    x = Math.Abs(carrierCircle - x);
                if (delta < x) continue;
                delta = x;
                index = i;
            }
            return index;
        }
        
    }
}

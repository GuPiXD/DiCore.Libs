using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Diascan.Utils.DataBuffers;
using DiCore.Lib.NDT.Carrier;
using DiCore.Lib.NDT.DataProviders.CDM;
using DiCore.Lib.NDT.Types;


namespace DiCore.Lib.NDT.DataProviders.CDPA
{
    public class CDpaDirectionDataProvider: BaseDataProvider
    {
        public override string Name => $"{direction.DirectionName} CDPA Data Provider";
        public override string Description => "Implement reading CDPA(DFR) data";
        public override string Vendor => "Sharov V.Y.";

        private readonly CDpaDirection direction;
        private CDpaDeviceParameters cdpaParameters;
        private readonly Dictionary<short, short> sensorNumbersThisDirection = new Dictionary<short, short>();
        private readonly unsafe int packetHeaderSize = sizeof(CDpaDataPacketHeader); // <-
        private readonly int sensorDataRawSize = 1;
        private int dataOffsetInPacket;

        /// <summary>
        /// Карта амплитуд
        /// </summary>
        private VectorBuffer<float> amplitudeMap;
        private unsafe float* amplitudeMapPtr;
        /// <summary>
        /// Карта времен для первого эха
        /// </summary>
        private VectorBuffer<float> timeFirstMap;
        private unsafe float* timeFirstMapPtr;
        /// <summary>
        /// Карта времен для следующих эх
        /// </summary>
        private VectorBuffer<float> timeNextMap;
        private unsafe float* timeNextMapPtr;

        private UIntPtr[] sensorInRawScanOffsets;

        internal readonly unsafe int PacketHeaderSize = sizeof(CDpaDataPacketHeader);
        internal readonly unsafe int SensorDataEchosSize = sizeof(CDpaSensorEchosIndex);

        /// <summary>
        /// Размер полей Номер датчика и Количество излучений
        /// </summary>
        internal const byte SensorHeaderSize = 4;
        /// <summary>
        /// Размер полей Идентификатор закона и Количество эхосигналов
        /// </summary>
        internal const byte SensorDataHeaderSize = 4;
        /// <summary>
        /// Размер поля Номер датчика
        /// </summary>
        internal const byte SENSOR_NUMBER_SIZE = 2;

        /// <summary>
        /// Размер в байтах поля "Кол-во пар измерений для датчика"
        /// </summary>
        private const byte SensorDataRawSize = 1;
        public CDpaDirectionDataProvider(UIntPtr heap, CDpaDirection dataDirection, Carrier.Carrier carrier) : base(heap, Constants.CDPADataDescription)
        {
            this.direction = dataDirection;

            var desc = DataDescription.DataDirSuffix;
            desc = desc.Remove(desc.Length - 4);
            desc += $@"{direction.Id:d3}\";

            DataDescription.DataDirSuffix = desc;

            CreateVirtualCarriers(carrier);
        }

        protected override int CalculateMaxScanSize()
        {
            cdpaParameters = (CDpaDeviceParameters)Parameters;
            return PacketHeaderSize + base.Carrier.SensorCount * (SensorHeaderSize + cdpaParameters.CDpaRuleParametersList.Count * (SensorDataHeaderSize + cdpaParameters.MaxCountSignal * SensorDataEchosSize));
        }

        protected override IDeviceParameters LoadParameters(DataLocation location)
        {
            return CDpaDeviceParameters.LoadFromOmni(location.FullPath);
        }

        protected override string BuildDataPath()
        {
            return String.Concat(Location.InspectionFullPath, @"\", Location.BaseName, DataDescription.DataDirSuffix, Location.BaseName);
        }

        private unsafe void FillMap()
        {
            var mapSize = cdpaParameters.RawEchoSize == enRawEchoSize._32bit ? ushort.MaxValue
                                                                             : byte.MaxValue;

            amplitudeMap = new VectorBuffer<float>(Heap, mapSize + 1, 1);
            timeFirstMap = new VectorBuffer<float>(Heap, mapSize + 1, 1);
            timeNextMap = new VectorBuffer<float>(Heap, mapSize + 1, 1);

            amplitudeMapPtr = (float*)amplitudeMap.Data;
            timeFirstMapPtr = (float*)timeFirstMap.Data;
            timeNextMapPtr = (float*)timeNextMap.Data;

            var koeff = cdpaParameters.KAmplitude;
            var timeSleepZoneDiv10 = cdpaParameters.TimeSleepZone / 10.0;
            var timeDiscretFirstDiv10 = cdpaParameters.TimeDiscretFirst / 10.0;
            var timeDiscretNextDiv10 = cdpaParameters.TimeDiscretNext / 10.0;

            Parallel.For(short.MinValue, short.MaxValue + 1, i =>
            {
                amplitudeMapPtr[(ushort)i] = (float)(/*Math.Sign(i) **/  Math.Sqrt(Math.Abs(i)) * koeff);
            });

            Parallel.For(0, ushort.MaxValue + 1, i =>
            {
                timeFirstMapPtr[i] = (float)(timeSleepZoneDiv10 + i * timeDiscretFirstDiv10);
                timeNextMapPtr[i] = (float)(i * timeDiscretNextDiv10);
            });
        }


        private static Sensor[] CopySensors(Sensor[] sensors)
        {
            var outputSensors = new Sensor[sensors.Length];

            for (var i = 0; i < sensors.Length; i++)
            {
                var currentSensor = new Sensor()
                {
                    Angle = sensors[i].Angle,
                    Angle2 = sensors[i].Angle2,
                    Bodynum = sensors[i].Bodynum,
                    Delay = sensors[i].Delay,
                    DirectionCode = sensors[i].DirectionCode,
                    DirectionInitialCode = sensors[i].DirectionInitialCode,
                    Dx = sensors[i].Dx,
                    Dy = sensors[i].Dy,
                    GroupNumber = sensors[i].GroupNumber,
                    CosAngle2 = sensors[i].CosAngle2,
                    SinAngle2 = sensors[i].SinAngle2,
                    SkiNumber = sensors[i].SkiNumber,
                    LogicalNumber = sensors[i].LogicalNumber,
                    OpposedLogicalNumber = sensors[i].OpposedLogicalNumber,
                    PhysicalNumber = sensors[i].PhysicalNumber,
                    Primary = sensors[i].Primary
                };

                outputSensors[i] = currentSensor;
            }

            return outputSensors;
        }

        private new void ReflectionSetField(Type entityType, object entity, string fieldName, object value)
        {
            var fi = entityType.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Debug.Assert(fi != null);

            fi.SetValue(entity, value);
        }

        private void FillCarrier(Carrier.Carrier cdpaCarrier, Carrier.Carrier virtualCarrier, Sensor[] sensors)
        {
            sensors = CopySensors(sensors);
            sensors = sensors.OrderBy(sensor => sensor.LogicalNumber).ToArray();
            var count = sensors.Length;

            if (sensors.Any(sensor => sensor.LogicalNumber > count))
            {
                short index = 1;
                foreach (var sensor in sensors)
                {
                    sensor.PhysicalNumber = sensor.LogicalNumber;
                    sensor.LogicalNumber = index++;
                }
            }
            ReflectionSetField(typeof(Carrier.Carrier), virtualCarrier, "m_sensors", sensors);
            virtualCarrier.Name = cdpaCarrier.Name;
            virtualCarrier.Circle = cdpaCarrier.Circle;
            virtualCarrier.Description = cdpaCarrier.Description;
            virtualCarrier.Diametr = cdpaCarrier.Diametr;
            virtualCarrier.PigType = cdpaCarrier.PigType;
        }

        /// <summary>
        /// Формеруем carrier для каждого направления DirectionDataProvider
        /// </summary>
        /// <param name="carrier"></param>
        private void CreateVirtualCarriers(Carrier.Carrier carrier)
        {
            var dirs = carrier.Select(item => item.Angle2).Distinct().ToArray();
            var virtualCarriers = new Carrier.Carrier[dirs.Length];

            for (var i = 0; i < dirs.Length; i++)
            {
                virtualCarriers[i] = new Carrier.Carrier();
                var dir = dirs[i];

                var sensors = carrier.ToList().FindAll(item => Math.Abs(item.Angle2 - dir) < Single.Epsilon).ToArray();
                FillCarrier(carrier, virtualCarriers[i], sensors);
            }
            base.Carrier = virtualCarriers.FirstOrDefault(item => item[0].Angle2 == direction.Angle && item[0].Angle == direction.EntryAngle);
        }


        public override bool Open(DataLocation location)
        {
            var baseOpened = base.Open(location);

            if (!baseOpened) return false;


            sensorInRawScanOffsets = new UIntPtr[base.Carrier.SensorCount];

            short i = 0;
            foreach (var sensor in base.Carrier)
                sensorNumbersThisDirection.Add((short)(sensor.LogicalNumber - 1), i++);

            SensorCount = (short)sensorNumbersThisDirection.Count;

            dataOffsetInPacket = SensorCount * sensorDataRawSize + packetHeaderSize;

            FillMap();

            return true;
        }

        private unsafe void FillDataBufferItem(CDPASensorData* sensorData, CDpaSensorEchosIndex* echosIndex)
        {
            var echos = sensorData->Echos;
            var time = timeFirstMapPtr[echosIndex->TimeIndex];

            echos->Amplitude = amplitudeMapPtr[echosIndex->AmplitudeIndex];
            echos->Time = time;

            for (var j = 1; j < sensorData->EchoCount; j++)
            {
                echos++;
                echosIndex++;
                time += timeNextMapPtr[echosIndex->TimeIndex];
                echos->Amplitude = amplitudeMapPtr[echosIndex->AmplitudeIndex];
                echos->Time = time;
            }
        }

        private unsafe void CalcSensorOffsets(UIntPtr packetPtr, ushort packetSize)
        {
            Array.Clear(sensorInRawScanOffsets, 0, sensorInRawScanOffsets.Length);

            var readBytes = PacketHeaderSize;

            var allBytes = packetSize * cdpaParameters.PacketKSize;
            var alignedSize = allBytes - cdpaParameters.PacketKSize;

            while (readBytes < alignedSize)
            {
                var sensorDataPtr = (packetPtr + readBytes);

                var sensorDataUsPtr = (ushort*)sensorDataPtr;

                sensorInRawScanOffsets[*sensorDataUsPtr] = sensorDataPtr;

                sensorDataUsPtr++;
                readBytes += 4;

                for (var i = 0; i < *sensorDataUsPtr; i++)
                {
                    var sensorDataHeaderPtr = (CDPASensorData*)(packetPtr + readBytes);

                    readBytes += SensorDataEchosSize * sensorDataHeaderPtr->EchoCount + SensorHeaderSize;
                }
            }
        }

        private unsafe void FillDataBufferCol(IDiagDataReader dataReader, int realScan, int refScan, object args)
        {
            if (realScan < 0) return;

            //Подготовка сервисов и базовые расчеты
            var arg = (DataHandleCDpa)args;

            var result = arg; /// prm.SensorMemoryManager

            //расчет сенсоров
            foreach (var item in SensorsByDeltaScanOrdering.Items)
            {
                var deltaScan = cdpaParameters.ScanFactor ? item.AdditionalAligmentInputInDeltaScan : item.DeltaScan;
                var targetScan = realScan + deltaScan;
                var packetHeaderPtr = IndexFile.GetDataPointer(dataReader, targetScan);
                if (packetHeaderPtr == UIntPtr.Zero) continue;

                var packetHeader = (CDpaDataPacketHeader*)packetHeaderPtr;

                CalcSensorOffsets(packetHeaderPtr, packetHeader->Size);

                foreach (var sensorIndex in item.SensorNumbers)
                {
                    var sensorPtr = sensorInRawScanOffsets[sensorIndex];
                    if (sensorPtr == UIntPtr.Zero) continue;

                    var sensorItem = (CDPASensorItem*)result.GetDataPointer(sensorIndex, refScan);

                    sensorItem->RayCount = *(ushort*)(sensorPtr + 2);

                    sensorItem->Data = result.Allocate<CDPASensorData>(sensorItem->RayCount);

                    var sensorDataPtr = sensorItem->Data;

                    var readBytes = 4;

                    for (var i = 0; i < sensorItem->RayCount; i++)
                    {
                        var rawRayDataPtr = (CDPASensorDataRaw*)(sensorPtr + readBytes);

                        sensorDataPtr->RuleId = rawRayDataPtr->RuleId;
                        sensorDataPtr->EchoCount = rawRayDataPtr->EchoCount;
                        sensorDataPtr->Echos = result.Allocate<CDPAEcho>(sensorDataPtr->EchoCount);

                        readBytes += 4;

                        var echosIndex = (CDpaSensorEchosIndex*)(sensorPtr + readBytes);

                        FillDataBufferItem(sensorDataPtr, echosIndex);

                        readBytes += SensorDataEchosSize * sensorDataPtr->EchoCount;

                        sensorDataPtr++;
                    }
                }
            }
        }

        public DataHandleCDpa GetData(int scanStart, int countScan, int compressStep)
        {
            var result = new DataHandleCDpa(SensorCount, countScan);

            DefineReadScans(scanStart, countScan, compressStep);
            CalcAligningSensors(scanStart, countScan, compressStep, AlignmentFactor, SensorsByDeltaScanOrdering);

            result.DirectionAngleCode = direction.Id;
            result.MaxCountSignal = cdpaParameters.MaxCountSignal;

            var args = result;

            ExecuteBaseReadScans(FillDataBufferCol, args, false);

            return result;
        }

        protected override void InnerClose()
        {
            sensorNumbersThisDirection.Clear();

            amplitudeMap?.Dispose();
            timeFirstMap?.Dispose();
            timeNextMap?.Dispose();

            base.InnerClose();
        }

        protected override short SensorToSensorIndex(short sensor)
        {
            return sensorNumbersThisDirection[sensor];
        }

        protected override short SensorIndexToSensor(short sensorIndex)
        {
            return sensorNumbersThisDirection.FirstOrDefault(item => item.Value == sensorIndex).Key;
        }


        internal short SensorIndexByDirectionToSensor(short sensorIndex)
        {
            return SensorIndexToSensor(sensorIndex);
        }

        internal short SensorToSensorIndexByDirection(short sensor)
        {
            return SensorToSensorIndex(sensor);
        }
    }
}
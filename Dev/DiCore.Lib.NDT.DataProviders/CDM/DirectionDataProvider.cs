using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Diascan.Utils.DataBuffers;
using DiCore.Lib.NDT.DataProviders.CDL;
using DiCore.Lib.NDT.Types;

namespace DiCore.Lib.NDT.DataProviders.CDM
{
    internal class DirectionDataProvider:BaseDataProvider
    {
        private readonly CdmDirection direction;
        private CdmDeviceParameters cdmParameters;
        private readonly Dictionary<short, short> sensorNumbersThisDirection = new Dictionary<short, short>();
        private readonly unsafe int packetHeaderSize = sizeof(DataPacketHeader); // <-
        private readonly int sensorDataRawSize = 1;
        private int dataOffsetInPacket;

        private unsafe float* amplitudeMapPtr;
        private unsafe float* timeFirstMapPtr;
        private unsafe float* timeNextMapPtr;

        /// <summary>
        /// Карта амплитуд
        /// </summary>
        private VectorBuffer<float> amplitudeMap;
        /// <summary>
        /// Карта времен для первого эха
        /// </summary>
        private VectorBuffer<float> timeFirstMap;
        /// <summary>
        /// Карта времен для следующих эх
        /// </summary>
        private VectorBuffer<float> timeNextMap;

        /// <summary>
        /// Размер в байтах поля "Кол-во пар измерений для датчика"
        /// </summary>
        private const byte SensorDataRawSize = 1;

        public DirectionDataProvider(UIntPtr heap, CdmDirection direction) : base(heap, Constants.CD360DataDescription)
        {
            this.direction = direction;

            var desc = DataDescription.DataDirSuffix;
            desc = desc.Remove(desc.Length - 4);
            desc += $@"{direction.Id:d3}\";

            DataDescription.DataDirSuffix = desc;
        }

        public override string Name => $"{direction.DirectionName} Data Provider";
        public override string Description => "Implement reading one direction Cdm(360, DKP) data";
        public override string Vendor => "Vagner I.";


        private byte EchoRawSize()
        {
            return (byte)(cdmParameters.RawEchoSize == enRawEchoSize._16bit ? sizeof(byte) * 2 : sizeof(ushort) * 2);
        }

        protected override int CalculateMaxScanSize()
        {
            cdmParameters = (CdmDeviceParameters)Parameters;

            return DataPacketHeader.RawSize + SensorCount * (SensorDataRawSize + EchoRawSize() * 64);
        }

        protected override IDeviceParameters LoadParameters(DataLocation location)
        {
            return CdmDeviceParameters.LoadFromOmni(location.FullPath);
        }

        protected override string BuildDataPath()
        {
            return String.Concat(Location.InspectionFullPath, @"\", Location.BaseName, DataDescription.DataDirSuffix, Location.BaseName);
        }

        public override bool Open(DataLocation location)
        {
            var baseOpened = base.Open(location);

            if (!baseOpened) return false;

            short i = 0;
            foreach (var sensor in Carrier.OrderBy(item => item.Dx).Where(item => Math.Abs(item.Angle2 - direction.Id) < Single.Epsilon))
                sensorNumbersThisDirection.Add((short)(sensor.LogicalNumber - 1), i++);

            SensorCount = (short) sensorNumbersThisDirection.Count;

            dataOffsetInPacket = SensorCount * sensorDataRawSize + packetHeaderSize;

            FillMap();

            return true;
        }

        private unsafe void FillMap()
        {
            var mapSize = cdmParameters.RawEchoSize == enRawEchoSize._16bit
                ? byte.MaxValue
                : ushort.MaxValue;

            amplitudeMap = new VectorBuffer<float>(Heap, mapSize, 1);
            timeFirstMap = new VectorBuffer<float>(Heap, mapSize, 1);
            timeNextMap = new VectorBuffer<float>(Heap, mapSize, 1);

            amplitudeMapPtr = (float*)amplitudeMap.Data;
            timeFirstMapPtr = (float*)timeFirstMap.Data;
            timeNextMapPtr = (float*)timeNextMap.Data;

            var timeSleepZoneDiv10 = cdmParameters.TimeSleepZone / 10f;
            var timeDiscretFirstDiv10 = cdmParameters.TimeDiscretFirst / 10f;
            var timeDiscretNextDiv10 = cdmParameters.TimeDiscretNext / 10f;

            float koeff = cdmParameters.KAmplitude;

            if (cdmParameters.RawEchoSize == enRawEchoSize._16bit)
            {
                Parallel.For(0, byte.MaxValue + 1, i => { amplitudeMapPtr[(ushort)i] = (float)Math.Sqrt(i) * koeff; }
                );
            }
            else
            {
                Parallel.For(short.MinValue, short.MaxValue,
                    i => { amplitudeMapPtr[(ushort)i] = (float)(Math.Sign(i) * Math.Sqrt(Math.Abs(i)) * koeff); }
                );
            }

            Parallel.For(0, mapSize + 1, i =>
                {
                    timeFirstMapPtr[i] = timeSleepZoneDiv10 + i * timeDiscretFirstDiv10;
                    timeNextMapPtr[i] = i * timeDiscretNextDiv10;
                }
            );
        }

        protected override void InnerClose()
        {
            sensorNumbersThisDirection.Clear();

            amplitudeMap?.Dispose();
            timeFirstMap?.Dispose();
            timeNextMap?.Dispose();

            base.InnerClose();
        }

        public DataHandleCdm GetData(int scanStart, int countScan, int compressStep)
        {
            var result = new DataHandleCdm(SensorCount, countScan);

            DefineReadScans(scanStart, countScan, compressStep);
            CalcAligningSensors(scanStart, countScan, compressStep, AlignmentFactor, SensorsByDeltaScanOrdering);

            result.EntryAngle = Carrier.FirstOrDefault(item => MathHelper.TestFloatEquals(item.Angle2, direction.Id))?.Angle ?? -1f;
            result.DirectionAngle = direction.Angle;
            result.DirectionAngleCode = direction.Id;
            result.MaxCountSignal = cdmParameters.MaxCountSignal;

            var args = result;

            ExecuteBaseReadScans(FillDataBufferCol, args, false);

            return result;
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

        private unsafe void FillDataBufferCol(IDiagDataReader dataReader, int realScan, int refScan, object args)
        {
            if (realScan < 0) return;

            var arg = (DataHandleCdm)args;

            var result = arg;

            foreach (var item in SensorsByDeltaScanOrdering.Items)
            {
                var deltaScan = cdmParameters.ScanFactor ? item.AdditionalAligmentInputInDeltaScan : item.DeltaScan;
                var targetScan = realScan + deltaScan;
                var packetHeaderPtr = IndexFile.GetDataPointer(dataReader, targetScan);

                if (packetHeaderPtr == UIntPtr.Zero) continue;

                var dataDescriptionPrt = (byte*)(packetHeaderPtr + DataPacketHeader.RawSize);
                var baseData = (DataItem*)(packetHeaderPtr + dataOffsetInPacket);

                foreach (var sensorNumber in item.SensorNumbers)
                {
                    if (!sensorNumbersThisDirection.ContainsKey((short) (sensorNumber))) continue;

                    var sensorIndex = SensorToSensorIndex((short) sensorNumber);

                    var sensorData = result.GetDataPointer(sensorIndex, refScan);
                    var echoCount = dataDescriptionPrt[sensorIndex];
                    var targetData = baseData;

                    if (echoCount == 0) continue;
                     sensorData->Count = echoCount;

                    for (var i = 0; i < sensorIndex; i++)
                        targetData += dataDescriptionPrt[i];

                    sensorData->Echos = result.Allocate<CDEcho>(sensorData->Count);

                    FillDataBufferItem(sensorData, targetData);
                }
            }
        }

        private unsafe void FillDataBufferItem(CDSensorDataEx* sensorData, DataItem* dataItem)
        {
            var echos = sensorData->Echos;
            var time = timeFirstMapPtr[dataItem->TimeIndex];

            echos->Amplitude = amplitudeMapPtr[dataItem->AmplitudeIndex];
            echos->Time = time;

            for (var j = 1; j < sensorData->Count; j++)
            {
                echos++;
                dataItem++;
                time += timeNextMapPtr[dataItem->TimeIndex];
                echos->Amplitude = amplitudeMapPtr[dataItem->AmplitudeIndex];
                echos->Time = time;
            }
        }
    }
}

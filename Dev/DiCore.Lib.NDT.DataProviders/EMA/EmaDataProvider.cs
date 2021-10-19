using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Diascan.Utils.DataBuffers;
using DiCore.Lib.NDT.Types;
using File = Diascan.Utils.IO.File;

namespace DiCore.Lib.NDT.DataProviders.EMA
{
    public class EmaDataProvider: BaseDataProvider
    {
        public override string Name => "Ema Data Provider";
        public override string Description => "Implement reading EMA data";
        public override string Vendor => "Gnetnev A.";

        public const int DefaultSizeSensor = 16;
        private const int MaxAmplitude = 2047;


        internal readonly unsafe int PacketHeaderSize = sizeof(EmaDataPacketHeader);
        internal readonly unsafe int SensorDataEchosSize = sizeof(EmaSensorEchosIndex);
        internal readonly int SensorHeaderSize = 4;
        internal readonly int SensorDataHeaderSize = 2;

        private EmaDeviceParameters emaParameters;

        private enEchoType echoType;

        /// <summary>
        /// Карта амплитуд
        /// </summary>
        private VectorBuffer<float> amplitudeMap;
        private unsafe float* amplitudeMapPtr;
        private Dictionary<EmaRuleEnum, VectorBuffer<float>> discreteTimeMap;

        /// <summary>
        /// Карта времен для эха
        /// </summary>
        private VectorBuffer<float> timeMap;
        private unsafe float* timeMapPtr;

        private float[] calibration;
        private float[] fakeCalibration;

        private UIntPtr[] sensorInRawScanOffsets;

        protected override int CalculateMaxScanSize()
        {
            emaParameters = (EmaDeviceParameters)Parameters;

            return PacketHeaderSize + Carrier.SensorCount *
                   (SensorHeaderSize + emaParameters.EmaRules.Count *
                    (SensorDataHeaderSize + emaParameters.MaxCountSignal * SensorDataEchosSize));
        }

        public EmaDataProvider(UIntPtr heap) : base(heap, Constants.EMADataDescription)
        {
        }

        protected override IDeviceParameters LoadParameters(DataLocation location)
        {
            return EmaDeviceParameters.LoadFromOmni(location.FullPath);
        }

        protected override string BuildDataPath()
        {
            return string.Concat(Location.InspectionFullPath, @"\", Location.BaseName, DataDescription.DataDirSuffix, Location.BaseName);
        }

        public override bool Open(DataLocation location)
        {
            if (!base.Open(location)) return false;

            FillMap();
            ReadCalibrations();
            sensorInRawScanOffsets = new UIntPtr[Carrier.SensorCount];

            return true;
        }

        private unsafe void FillMap()
        {
            var count = emaParameters.SizeSensor > DefaultSizeSensor ? ushort.MaxValue : byte.MaxValue;

            amplitudeMap = new VectorBuffer<float>(Heap, count + 1, 1);
            timeMap      = new VectorBuffer<float>(Heap, count + 1, 1);

            discreteTimeMap = new Dictionary<EmaRuleEnum, VectorBuffer<float>>(emaParameters.EmaRules.Count);
            echoType = emaParameters.EchoType;

            foreach (var emaRule in emaParameters.EmaRules)
                discreteTimeMap.Add(emaRule.Id, new VectorBuffer<float>(Heap, byte.MaxValue + 1, 1)); // карта времени для индекса 

            amplitudeMapPtr = (float*)amplitudeMap.Data;
            timeMapPtr = (float*)timeMap.Data;

            if (emaParameters.SizeSensor > DefaultSizeSensor)
                FillMapSizeSensor32();
            else
                FillMapSizeSensor16();

            if (echoType == enEchoType.Short)
                FillDiscreteTimeMap();
        }

        private unsafe void FillMapSizeSensor16()
        {
            var koeff = emaParameters.KAmplitude;
            var timeSleepZone = emaParameters.TimeSleepZone;
            var timeDiscretDiv10 = emaParameters.TimeDiscret / 10f;

            Parallel.For(0, byte.MaxValue + 1, i =>
            {
                amplitudeMapPtr[i] = (float)Math.Sqrt(i) * koeff;
                timeMapPtr[i] = timeSleepZone + i * timeDiscretDiv10;
            });
        }

        private unsafe void FillMapSizeSensor32()
        {
            var timeSleepZone = emaParameters.TimeSleepZone;
            var timeDiscretDiv10 = emaParameters.TimeDiscret / 10f;

            Parallel.For(short.MinValue, short.MaxValue + 1, i =>
            {
                amplitudeMapPtr[(ushort)i] = i;
            });

            Parallel.For(0, ushort.MaxValue + 1, i =>
            {
                timeMapPtr[i] = timeSleepZone + i * timeDiscretDiv10;
            });
        }

        private unsafe void FillDiscreteTimeMap()
        {
            foreach (var emaTimeRule in emaParameters.EmaTimeRules)
            {
                var discreteTimeMapPtr = (float*)discreteTimeMap[emaTimeRule.Id].Data;
                if (discreteTimeMapPtr == null) continue;

                Parallel.For(0, byte.MaxValue + 1, i =>
                {
                    discreteTimeMapPtr[i] = (emaTimeRule.TimeSleepZone + i * emaTimeRule.TimeDiscrete) / 10f; // параметры записаны в формате 0.1мкс
                });
            }
        }

        private void ReadCalibrations()
        {
            var basePath = string.Concat(Location.InspectionFullPath, Path.DirectorySeparatorChar,
                Location.BaseName, DataDescription.DataDirSuffix);

            var path = Path.Combine(basePath, "EmaCalibrations.json");

            fakeCalibration = new float[SensorCount];
            calibration = new float[SensorCount];

            for (var i = 0; i < SensorCount; i++)
                fakeCalibration[i] = 1f;

            if (File.Exists(path))
            {
                var calibrationsSerializer = new DataContractJsonSerializer(typeof(EmaCalibration[]));

                var reader = new StreamReader(path);

                try
                {
                    var emaCalibrations = (EmaCalibration[])calibrationsSerializer.ReadObject(reader.BaseStream);

                    for (var i = 0; i < SensorCount; i++)
                    {
                        var emaCalibration = emaCalibrations.FirstOrDefault(item => item.Sensor == i + 1);
                        calibration[i] = emaCalibration.Value;
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            else
                calibration = fakeCalibration;
        }

        public DataHandleEma GetEmaData(int scanStart, int scanCount, int compressStep = 1, bool calibrationMatrixEnabled = false)
        {
            var result = new DataHandleEma(SensorCount, scanCount) {MaxCountSignal = emaParameters.MaxCountSignal};

            DefineReadScans(scanStart, scanCount, compressStep);

            CalcAligningSensors(scanStart, scanCount, compressStep, AlignmentFactor, SensorsByDeltaScanOrdering);

            var args = new Tuple<DataHandleEma, float[]>(result, calibrationMatrixEnabled ? calibration : fakeCalibration);
            if (echoType == enEchoType.Short)
                ExecuteBaseReadScans(FillDataBufferColShort, args, false);
            else
                ExecuteBaseReadScans(FillDataBufferCol, args, false);

            return result;
        }

        private unsafe void FillDataBufferCol(IDiagDataReader dataReader, int realScan, int refScan, object args)
        {
            if (realScan < 0) return;

            var arg = (Tuple<DataHandleEma, float[]>)args;

            var result = arg.Item1;
            var innerCalibration = arg.Item2;

            foreach (var item in SensorsByDeltaScanOrdering.Items)
            {
                var deltaScan = emaParameters.ScanFactor ? item.AdditionalAligmentInputInDeltaScan : item.DeltaScan;
                var targetScan = realScan + deltaScan;
                var packetHeaderPtr = IndexFile.GetDataPointer(dataReader, targetScan);

                if (packetHeaderPtr == UIntPtr.Zero) continue;

                var packetHeader = (EmaDataPacketHeader*)packetHeaderPtr;
                if (packetHeader->ScanNumber != targetScan)
                    throw new Exception($"Failed scan number. Request scan - {targetScan}; response scan - {packetHeader->ScanNumber}");

                CalcSensorOffsets(packetHeaderPtr, packetHeader->Size);

                foreach (var sensorIndex in item.SensorNumbers)
                {
                    var sensorPtr = sensorInRawScanOffsets[sensorIndex];
                    if (sensorPtr == UIntPtr.Zero) continue;

                    var sensorData = result.GetDataPointer(sensorIndex, refScan);

                    sensorData->RayCount = *(ushort*)(sensorPtr + 2);
                    //sensorData->Data = (EmaSensorData*)sensorDataMemoryManager.AllocateMemory(sensorData->RayCount);
                    sensorData->Data = result.Allocate<EmaSensorData>(sensorData->RayCount);

                    var sensorDataDataPtr = sensorData->Data;

                    var calibrationValue = innerCalibration[sensorIndex];

                    var readBytes = 4;

                    for (var i = 0; i < sensorData->RayCount; i++)
                    {
                        var rawRayDataPtr = (EmaSensorDataRaw*)(sensorPtr + readBytes);

                        sensorDataDataPtr->Rule = ParseRuleId(rawRayDataPtr->RuleId);
                        sensorDataDataPtr->EchoCount = rawRayDataPtr->EchoCount;
                        //sensorDataDataPtr->Echos = (EmaEcho*)memoryManager.AllocateMemory(sensorDataDataPtr->EchoCount);
                        sensorDataDataPtr->Echos = result.Allocate<EmaEcho>(sensorDataDataPtr->EchoCount);

                        readBytes += 2;

                        var echosIndex = (EmaSensorEchosIndex*)(sensorPtr + readBytes);

                        FillDataBufferItem(sensorDataDataPtr, echosIndex, calibrationValue);

                        readBytes += SensorDataEchosSize * sensorDataDataPtr->EchoCount;

                        sensorDataDataPtr++;
                    }
                }
            }
        }

        private unsafe void FillDataBufferColShort(IDiagDataReader dataReader, int realScan, int refScan, object args)
        {
            if (realScan < 0) return;

            var arg = (Tuple<DataHandleEma, float[]>)args;

            var result = arg.Item1;
            var innerCalibration = arg.Item2;

            foreach (var item in SensorsByDeltaScanOrdering.Items)
            {
                var deltaScan = emaParameters.ScanFactor ? item.AdditionalAligmentInputInDeltaScan : item.DeltaScan;
                var targetScan = realScan + deltaScan;
                var packetHeaderPtr = IndexFile.GetDataPointer(dataReader, targetScan);

                if (packetHeaderPtr == UIntPtr.Zero) continue;

                var packetHeader = (EmaDataPacketHeader*)packetHeaderPtr;
                if (packetHeader->ScanNumber != targetScan)
                    throw new Exception($"Failed scan number. Request scan - {targetScan}; response scan - {packetHeader->ScanNumber}");

                CalcSensorOffsets(packetHeaderPtr, packetHeader->Size);

                foreach (var sensorIndex in item.SensorNumbers)
                {
                    var sensorPtr = sensorInRawScanOffsets[sensorIndex];
                    if (sensorPtr == UIntPtr.Zero) continue;

                    var sensorData = result.GetDataPointer(sensorIndex, refScan);

                    sensorData->RayCount = *(ushort*)(sensorPtr + 2);
                    //sensorData->Data = (EmaSensorData*)sensorDataMemoryManager.AllocateMemory(sensorData->RayCount);
                    sensorData->Data = result.Allocate<EmaSensorData>(sensorData->RayCount);

                    var sensorDataDataPtr = sensorData->Data;

                    var calibrationValue = innerCalibration[sensorIndex];

                    var readBytes = 4;

                    for (var i = 0; i < sensorData->RayCount; i++)
                    {
                        var rawRayDataPtr = (EmaSensorDataRaw*)(sensorPtr + readBytes);

                        sensorDataDataPtr->Rule = ParseRuleId(rawRayDataPtr->RuleId);
                        sensorDataDataPtr->EchoCount = rawRayDataPtr->EchoCount;
                        //sensorDataDataPtr->Echos = (EmaEcho*)memoryManager.AllocateMemory(sensorDataDataPtr->EchoCount);
                        sensorDataDataPtr->Echos = result.Allocate<EmaEcho>(sensorDataDataPtr->EchoCount);

                        readBytes += 2;

                        var echosIndex = (ushort*)(sensorPtr + readBytes);

                        FillDataBufferItem(sensorDataDataPtr, echosIndex, calibrationValue);

                        readBytes += 2 * sensorDataDataPtr->EchoCount;

                        sensorDataDataPtr++;
                    }
                }
            }
        }

        private static EmaRuleEnum ParseRuleId(sbyte emaRuleId)
        {
            switch (emaRuleId)
            {
                case 0:
                    return EmaRuleEnum.NR;
                case 1:
                    return EmaRuleEnum.R1;
                case 2:
                    return EmaRuleEnum.R2;
                case 3:
                    return EmaRuleEnum.L1;
                case 4:
                    return EmaRuleEnum.L2;
                case 5:
                    return EmaRuleEnum.N;
                case 6:
                    return EmaRuleEnum.NC;
                default:
                    return EmaRuleEnum.Null;
            }
        }

        private unsafe void CalcSensorOffsets(UIntPtr packetPtr, ushort packetSize)
        {
            Array.Clear(sensorInRawScanOffsets, 0, sensorInRawScanOffsets.Length);

            var echoSize = echoType == enEchoType.Short ? 2 : SensorDataEchosSize;

            var readBytes = PacketHeaderSize;

            var allBytes = packetSize * 2; // размер пакета в словах (2байта)

            while (allBytes > readBytes)
            {
                var sensorDataPtr = (packetPtr + readBytes);

                var sensorDataUsPtr = (ushort*)sensorDataPtr;

                sensorInRawScanOffsets[*sensorDataUsPtr] = sensorDataPtr;

                sensorDataUsPtr++;
                readBytes += SensorHeaderSize;

                for (int i = 0; i < *sensorDataUsPtr; i++)
                {
                    var sensorDataHeaderPtr = (EmaSensorData*)(packetPtr + readBytes);

                    readBytes += echoSize * sensorDataHeaderPtr->EchoCount + SensorDataHeaderSize;
                }
            }
        }

        private unsafe void FillDataBufferItem(EmaSensorData* sensorData, ushort* rawAmplitude, float calibrationValue)
        {
            var echos = sensorData->Echos;
            var discreteTimeMapItemPtr = (float*)discreteTimeMap[sensorData->Rule].Data;

            for (var i = 0; i < sensorData->EchoCount; i++)
            {
                var amplitude = Math.Abs(calibrationValue * amplitudeMapPtr[*rawAmplitude]);

                echos->Amplitude = Math.Min(amplitude, MaxAmplitude); // заполнять карту до максимального значения? 
                echos->Time = discreteTimeMapItemPtr[i];
                echos++;
                rawAmplitude++;
            }
        }

        private unsafe void FillDataBufferItem(EmaSensorData* sensorData, EmaSensorEchosIndex* echosIndex, float calibrationValue)
        {
            var echos = sensorData->Echos;

            for (var i = 0; i < sensorData->EchoCount; i++)
            {
                var amplitude = Math.Abs(calibrationValue * amplitudeMapPtr[echosIndex->AmplitudeIndex]);

                echos->Amplitude = Math.Min(amplitude, MaxAmplitude);
                echos->Time = timeMapPtr[echosIndex->TimeIndex];
                echos++;
                echosIndex++;
            }
        }
    }
}

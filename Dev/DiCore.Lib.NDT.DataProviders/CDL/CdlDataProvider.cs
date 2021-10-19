using System;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Diascan.Utils.DataBuffers;
using DiCore.Lib.NDT.Types;
using File = Diascan.Utils.IO.File;

namespace DiCore.Lib.NDT.DataProviders.CDL
{
    public class CdlDataProvider : BaseDataProvider
    {
        public override string Name => "CDL Data Provider";
        public override string Description => "Implement reading CDL data";
        public override string Vendor => "Gnetnev A.";

        private CdlDeviceParameters cdlParameters;

        private readonly unsafe int packetHeaderSize = sizeof(DataPacketHeader); // <-
        private readonly int sensorDataRawSize = 1;
        private readonly int echoRawSize = 2;
        private readonly int maxEchoCount = 64;
        private int dataOffsetInPacket;

        private float[] calibrationMatrix;
        private float[] fakeCalibrationMatrix;

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

        public override bool Open(DataLocation location)
        {
            if (!base.Open(location)) return false;

            FillMap();
            InitCalibrationMatrix();

            dataOffsetInPacket = SensorCount * sensorDataRawSize + packetHeaderSize;
            return true;
        }

        private unsafe void FillMap()
        {
            amplitudeMap = new VectorBuffer<float>(Heap, 256, 1);
            timeFirstMap = new VectorBuffer<float>(Heap, 256, 1);
            timeNextMap = new VectorBuffer<float>(Heap, 256, 1);

            amplitudeMapPtr = (float*)amplitudeMap.Data;
            timeFirstMapPtr = (float*)timeFirstMap.Data;
            timeNextMapPtr = (float*)timeNextMap.Data;

            var timeSleepZoneDiv10 = cdlParameters.TimeSleepZone / 10f;
            var timeDiscretFirstDiv10 = cdlParameters.TimeDiscretFirst / 10f;
            var timeDiscretNextDiv10 = cdlParameters.TimeDiscretNext / 10f;
            var koeff = cdlParameters.KAmplitude;

            Parallel.For(0, 256, i =>
                {
                    amplitudeMapPtr[i] = (float)Math.Sqrt(i) * koeff;
                    timeFirstMapPtr[i] = timeSleepZoneDiv10 + i * timeDiscretFirstDiv10;
                    timeNextMapPtr[i] = i * timeDiscretNextDiv10;
                }
            );
        }

        protected override int CalculateMaxScanSize()
        {
            cdlParameters = (CdlDeviceParameters)Parameters;

            return packetHeaderSize + SensorCount * (sensorDataRawSize + echoRawSize * maxEchoCount);
        }

        protected override IDeviceParameters LoadParameters(DataLocation location)
        {
            return CdlDeviceParameters.LoadFromOmni(location.FullPath);
        }

        protected override string BuildDataPath()
        {
            return String.Concat(Location.InspectionFullPath, @"\", Location.BaseName, DataDescription.DataDirSuffix, Location.BaseName);
        }

        public DataHandleCdl GetCdlData(int scanStart, int scanCount, int compressStep = 1, bool calibrationMatrixEnabled = false)
        {
            var result = new DataHandleCdl(SensorCount, scanCount);
            result.MaxCountSignal = cdlParameters.MaxCountSignal;

            DefineReadScans(scanStart, scanCount, compressStep);

            CalcAligningSensors(scanStart, scanCount, compressStep, AlignmentFactor, SensorsByDeltaScanOrdering);

            var args = new Tuple<DataHandleCdl, float[]>(result, calibrationMatrixEnabled ? calibrationMatrix : fakeCalibrationMatrix);

            ExecuteBaseReadScans(FillDataBufferCol, args, false);
            
            return result;
        }

        private void InitCalibrationMatrix()
        {
            var calibrationMatrixPath = $"{Location.BaseDirectory}" + @"\Statistics\CDAmplitude_Q.dat";

            var emptyMatrix = new float[SensorCount];
                for (var i = 0; i < SensorCount; i++)
                    emptyMatrix[i] = 1f;

            fakeCalibrationMatrix = emptyMatrix;

            if (File.Exists(calibrationMatrixPath))
            {
                using (var fileStream = new FileStream(calibrationMatrixPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var formatter = new BinaryFormatter();
                    var matrix = (double[]) formatter.Deserialize(fileStream);
                    calibrationMatrix = matrix.Cast<float>().ToArray();
                }
            }
            else
            {
                calibrationMatrix = emptyMatrix;
            }
        }

        private unsafe void FillDataBufferCol(IDiagDataReader dataReader, int realScan, int refScan, object args)
        {
            if (realScan < 0) return;

            var arg = (Tuple<DataHandleCdl, float[]>) args;

            var result = arg.Item1;
            var calibrMatrix = arg.Item2;

            foreach (var item in SensorsByDeltaScanOrdering.Items)
            {
                var deltaScan = cdlParameters.ScanFactor ? item.AdditionalAligmentInputInDeltaScan : item.DeltaScan;
                var targetScan = realScan + deltaScan;
                var packetHeaderPtr = IndexFile.GetDataPointer(dataReader, targetScan);

                if (packetHeaderPtr == UIntPtr.Zero) continue;

                var dataDescriptionPrt = (byte*) (packetHeaderPtr + packetHeaderSize);
                var baseData = (CDDataItem*) (packetHeaderPtr + dataOffsetInPacket);

                foreach (var sensorNumber in item.SensorNumbers)
                {
                    //var dataSensorIndex = SensorToSensorIndex((short)sensorNumber);

                    var sensorData = result.GetDataPointer(sensorNumber, refScan);
                    var echoCount = dataDescriptionPrt[sensorNumber];
                    var targetData = baseData;
                    
                    if (echoCount == 0) continue;
                    sensorData->Count = echoCount;

                    for (var i = 0; i < sensorNumber; i++)
                        targetData += dataDescriptionPrt[i];

                    sensorData->Echos = result.Allocate<CDEcho>(sensorData->Count);

                    FillDataBufferItem(sensorData, targetData, calibrMatrix[sensorNumber]);
                }
            }
        }

        private unsafe void FillDataBufferItem(CDSensorDataEx* sensorData, CDDataItem* dataItem, float calibrationFactor)
        {
            var echos = sensorData->Echos;
            var time = timeFirstMapPtr[dataItem->TimeIndex];

            echos->Amplitude = amplitudeMapPtr[dataItem->AmplitudeIndex] * calibrationFactor;
            echos->Time = time;

            for (var j = 1; j < sensorData->Count; j++)
            {
                echos++;
                dataItem++;
                time += timeNextMapPtr[dataItem->TimeIndex];
                echos->Amplitude = amplitudeMapPtr[dataItem->AmplitudeIndex] * calibrationFactor;
                echos->Time = time;
            }
        }

        public float GetSensorRayDirection(int sensorIndex)
        {
            var sensor = Carrier.FirstOrDefault(item => item.LogicalNumber == (short)sensorIndex + 1);
            if (sensor == null) return -1f;

            return sensor.Angle > 0 ? 90f : 270f;
        }

        public float GetEntryAngle()
        {
            return Carrier.FirstOrDefault()?.Angle ?? -1f;
        }
        
        public CdlDataProvider(UIntPtr heap) : base(heap, Constants.CDLDataDescription)
        {
        }

        protected override void InnerClose()
        {
            amplitudeMap?.Dispose();
            timeFirstMap?.Dispose();
            timeNextMap?.Dispose();
            base.InnerClose();
        }
    }
}

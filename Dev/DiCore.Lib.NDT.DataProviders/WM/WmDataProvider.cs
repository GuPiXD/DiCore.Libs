using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Diascan.Utils.DataBuffers;
using DiCore.Lib.NDT.DataProviders.WM.WM32;
using DiCore.Lib.NDT.Types;
using File = Diascan.Utils.IO.File;

namespace DiCore.Lib.NDT.DataProviders.WM
{
    public partial class WmDataProvider : BaseDataProvider
    {
        private WmDeviceParameters wmParameters;
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
        /// Карта мм для первого эха
        /// </summary>
        private VectorBuffer<float> mmFirstMap;
        /// <summary>
        /// Карта мм для следующих эх
        /// </summary>
        private VectorBuffer<float> mmNextMap;

        private unsafe float* amplitudeMapPtr;
        private unsafe float* timeFirstMapPtr;
        private unsafe float* timeNextMapPtr;
        private unsafe float* mmFirstMapPtr;
        private unsafe float* mmNextMapPtr;

        private readonly VectorBuffer<WMEcho3> sensorEchos3 = new VectorBuffer<WMEcho3>(3);
        private unsafe WMEcho3* sensorEchos3Ptr;

        private Histogram wtHistogram;
        private Histogram soHistogram;

        private const int HistogramCount = 512;
        private const int HistogramShift = 2;

        private Calibrations calibrations;
        private int mapSize;
        private double wtCorrectionByCarrier;
        private VectorBuffer<float> echoInterlaceMap;
        private int[] thresholdsFastArray;
        public bool CalibrationAvailable => calibrations != null;

        private VectorBuffer<int> sensorOffsetInRawScanMap;
        private unsafe int* sensorOffsetInRawScanMapPtr;

        private VectorBuffer<int> sensorOffsetInAllignedRawScanMap;
        private unsafe int* sensorOffsetInAllignedRawScanMapPtr;

        private int packetHeaderSize;

        /// <summary>
        /// Размер в байтах поля "Кол-во пар измерений для датчика"
        /// </summary>
        private const byte Sensor32DataRawSize = 1;

        public UIntPtr HeapPub => Heap;

        public WmDeviceParameters WmParameters => wmParameters;

        public WmDataProvider(UIntPtr heap) : base(heap, Constants.WMDataDescription)
        {
        }

        public override string Name => "WM Data Provider";
        public override string Description => "Implement reading WM data";
        public override string Vendor => "Vagner I.";
        protected override string BuildDataPath()
        {
            return String.Concat(Location.InspectionFullPath, @"\", Location.BaseName, DataDescription.DataDirSuffix, Location.BaseName);
        }

        private int MapResolution()
        {
            return wmParameters.RawEchoSize == enRawEchoSize._16bit ? byte.MaxValue : ushort.MaxValue;
        }

        public override bool Open(DataLocation location)
        {
            var opened = base.Open(location);

            opened = opened && IndexFile != null;

            if (!opened) return false;

            unsafe
            {
                wtCorrectionByCarrier = WtCorrection.GetWtCorrectionByCarrierId(Carrier.OldId);

                sensorEchos3Ptr = (WMEcho3*)sensorEchos3.Data;
            }

            mapSize = MapResolution();

            amplitudeMap = new VectorBuffer<float>(Heap, mapSize, 1);
            timeFirstMap = new VectorBuffer<float>(Heap, mapSize, 1);
            timeNextMap = new VectorBuffer<float>(Heap, mapSize, 1);
            mmFirstMap = new VectorBuffer<float>(Heap, mapSize, 1);
            mmNextMap = new VectorBuffer<float>(Heap, mapSize, 1);

            wtHistogram = new Histogram(HistogramCount, HistogramShift);
            soHistogram = new Histogram(HistogramCount, HistogramShift);



            PrepareOldFormatSupporting();

            FillMaps();
            ReadCalibrations();

            return true;
        }

        protected override unsafe int CalculateMaxScanSize()
        {
            wmParameters = (WmDeviceParameters)Parameters;

            packetHeaderSize = sizeof(WMDataPacketHeader);


            return packetHeaderSize +
                          Carrier.SensorCount *
                          Math.Max(DataRawSize(),
                              Sensor32DataRawSize +
                              EchoRawSize() *
                              wmParameters.MaxCountSignal);
        }

        protected override IDeviceParameters LoadParameters(DataLocation location)
        {
            return WmDeviceParameters.LoadFromOmni(location.FullPath);
        }

        public DataHandleWm GetWmData(int scanStart, int scanCount, float[] currentSoCalibration, int compressStep = 1)
        {
            ResetNominals();

            DefineReadScans(scanStart, scanCount, compressStep);

            CalcAligningSensors(scanStart, scanCount, compressStep, AlignmentFactor, SensorsByDeltaScanOrdering);

            var result = new DataHandleWm(SensorCount, scanCount);

            if (currentSoCalibration == null)
                currentSoCalibration = new float[Carrier.SensorCount];

            var args = new Tuple<DataHandleWm, float[]>(result, currentSoCalibration);

            switch (IndexFile.FileType)
            {
                case 0x4443:
                    ExecuteBaseReadScans(WM32FillDataBuffer, args, false);
                    break;
                case 0x4D57:
                    if (wmParameters.BlockSize == 2)
                    {
                        if (wmParameters.RawEchoSize == enRawEchoSize._16bit)
                            ExecuteBaseReadScans(ReadByteRaws4D57Item2, args, true);
                        else
                            ExecuteBaseReadScans(ReadUShortRaws4D57Item2, args, true);
                    }
                    else
                    {
                        ExecuteBaseReadScans(ReadByteRaws4D57Item3, args, false);
                    }
                    break;
                case 0x4d58:
                    ExecuteBaseReadScans(ReadData4D58, args, false);
                    break;
            }

            return result;
        }
        
        public DataHandle<float> GetWtData(int scanStart, int scanCount, int compressStep = 1)
        {
            DefineReadScans(scanStart, scanCount, compressStep);

            CalcAligningSensors(scanStart, scanCount, compressStep, AlignmentFactor, SensorsByDeltaScanOrdering);

            var result = new DataHandle<float>(SensorCount, scanCount);
            
            switch (IndexFile.FileType)
            {
                case 0x4443:
                    ExecuteBaseReadScans(WM32FillDataBufferWt, result, true);
                    break;
                case 0x4D57:
                    if (wmParameters.BlockSize == 2)
                    {
                        if (wmParameters.RawEchoSize == enRawEchoSize._16bit)
                            ExecuteBaseReadScans(ReadByteRaws4D57Item2Wt, result, true);
                        else
                            ExecuteBaseReadScans(ReadUShortRaws4D57Item2Wt, result, true);
                    }
                    else
                    {
                        ExecuteBaseReadScans(ReadByteRaws4D57Item3Wt, result, true);
                    }
                    break;
                case 0x4d58:
                    ExecuteBaseReadScans(ReadData4D58Wt, result, true);
                    break;
            }

            return result;
        }

        public DataHandle<WM32SensorDataEx> GetWm32Data(int scanStart, int scanCount, int compressStep = 1)
        {
            DefineReadScans(scanStart, scanCount, compressStep);

            CalcAligningSensors(scanStart, scanCount, compressStep, AlignmentFactor, SensorsByDeltaScanOrdering);

            if (IndexFile.FileType != 0x4443) return null;

            var result = new DataHandle<WM32SensorDataEx>(SensorCount, scanCount);

            if (wmParameters.RawEchoSize == enRawEchoSize._16bit)
                ExecuteBaseReadScans(WM32Fill32DataBufferCol, result, true);
            else
                ExecuteBaseReadScans(WM32Fill32DataBufferColUshort, result, true);

            return result;
        }

        #region Standart Read Methods

        private unsafe void ReadByteRaws4D57Item2(IDiagDataReader dataReader, int realScan, int refScan, object args)
        {
            if (realScan < 0) return;

            var scanFactor = wmParameters.ScanFactor;

            var arg = (Tuple<DataHandleWm, float[]>) args;

            var result = arg.Item1;
            var calibrationSo = arg.Item2;

            foreach (var item in SensorsByDeltaScanOrdering.Items)
            {
                var deltaScan = scanFactor ? item.AdditionalAligmentInputInDeltaScan : item.DeltaScan;
                var scanOffset = IndexFile.GetDataPointer(dataReader, realScan + deltaScan);

                if (scanOffset == UIntPtr.Zero)
                    continue;

                foreach (var sensorNumber in item.SensorNumbers)
                {
                    var dataSensorIndex = base.SensorToSensorIndex((short)sensorNumber);

                    var sensorData = result.GetDataPointer(dataSensorIndex, refScan);
                    var baseData = (WMEchoRaw*)(scanOffset + sensorOffsetInRawScanMapPtr[sensorNumber]);

                    FillValueFromItem2(sensorData, baseData, calibrationSo[dataSensorIndex]);

                    UpdateGistoSO(sensorData->SO);
                    UpdateGistoWT(sensorData->WT);
                }
            }

            result.SoNominal = soHistogram.HighUsageValue;
            result.WtNominal = wtHistogram.HighUsageValue;
        }

        private unsafe void ReadUShortRaws4D57Item2(IDiagDataReader dataReader, int realScan, int refScan, object args)
        {
            if (realScan < 0) return;

            var scanFactor = wmParameters.ScanFactor;

            var arg = (Tuple<DataHandleWm, float[]>)args;

            var result = arg.Item1;
            var calibrationSo = arg.Item2;

            foreach (var item in SensorsByDeltaScanOrdering.Items)
            {
                var deltaScan = scanFactor ? item.AdditionalAligmentInputInDeltaScan : item.DeltaScan;
                var scanOffset = IndexFile.GetDataPointer(dataReader, realScan + deltaScan);

                if (scanOffset == UIntPtr.Zero)
                    continue;

                foreach (var sensorNumber in item.SensorNumbers)
                {
                    var dataSensorIndex = base.SensorToSensorIndex((short) sensorNumber);

                    var sensorData = result.GetDataPointer(dataSensorIndex, refScan);
                    var baseData = (WMEcho32Raw*) (scanOffset + sensorOffsetInRawScanMapPtr[sensorNumber]);

                    FillValueFromItem2_32(sensorData, baseData, calibrationSo[dataSensorIndex]);

                    UpdateGistoSO(sensorData->SO);
                    UpdateGistoWT(sensorData->WT);
                }
            }

            result.SoNominal = soHistogram.HighUsageValue;
            result.WtNominal = wtHistogram.HighUsageValue;
        }

        private unsafe void ReadByteRaws4D57Item3(IDiagDataReader dataReader, int realScan, int refScan, object args)
        {
            if (realScan < 0) return;

            var scanFactor = wmParameters.ScanFactor;

            var arg = (Tuple<DataHandleWm, float[]>) args;

            var result = arg.Item1;
            var calibrationSo = arg.Item2;

            foreach (var item in SensorsByDeltaScanOrdering.Items)
            {
                var deltaScan = scanFactor ? item.AdditionalAligmentInputInDeltaScan : item.DeltaScan;
                var scanOffset = IndexFile.GetDataPointer(dataReader, realScan + deltaScan);

                if (scanOffset == UIntPtr.Zero)
                    continue;

                foreach (var sensorNumber in item.SensorNumbers)
                {
                    var dataSensorIndex = base.SensorToSensorIndex((short) sensorNumber);

                    var sensorData = result.GetDataPointer(dataSensorIndex, refScan);
                    var baseData = (WMEchoRaw*) (scanOffset + sensorOffsetInRawScanMapPtr[sensorNumber]);

                    FillValueFromItem3(sensorData, baseData, calibrationSo[dataSensorIndex]);

                    UpdateGistoSO(sensorData->SO);
                    UpdateGistoWT(sensorData->WT);
                }
            }

            result.SoNominal = soHistogram.HighUsageValue;
            result.WtNominal = wtHistogram.HighUsageValue;
        }

        private unsafe void ReadData4D58(IDiagDataReader dataReader, int realScan, int refScan, object args)
        {
            if (realScan < 0) return;

            var arg = (Tuple<DataHandleWm, float[]>) args;

            var result = arg.Item1;
            var calibrationSo = arg.Item2;

            foreach (var item in SensorsByDeltaScanOrdering.Items)
            {
                var scanOffset = IndexFile.GetDataPointer(dataReader, realScan);

                if (scanOffset == UIntPtr.Zero)
                    continue;

                foreach (var sensorNumber in item.SensorNumbers)
                {
                    var dataSensorIndex = base.SensorToSensorIndex((short) sensorNumber);

                    var sensorData = result.GetDataPointer(dataSensorIndex, refScan);
                    var baseData = (WMEchoRawAligned*) (scanOffset + sensorOffsetInAllignedRawScanMapPtr[sensorNumber]);

                    FillValueFromDecimalItem(sensorData, baseData, calibrationSo[dataSensorIndex]);

                    UpdateGistoSO(sensorData->SO);
                    UpdateGistoWT(sensorData->WT);
                }
            }

            result.SoNominal = soHistogram.HighUsageValue;
            result.WtNominal = wtHistogram.HighUsageValue;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void FillValueFromItem2(WMSensorData* sensorData, WMEchoRaw* dataItem, float correctionSo)
        {
            if (IsInvalidCode(dataItem->TimeCode))
            {
                NativeMemoryApi.ZeroMemory((UIntPtr)sensorData, WMSensorData.Size);
            }
            else
            {
                sensorData->SO = GetSO(dataItem->TimeCode) + correctionSo;
                //  sensorData->AW2 = GetAW(dataItem->AmplitudeCode);
                dataItem++;
                sensorData->WT = GetWT(dataItem->TimeCode);
                //   sensorData->AW = GetAW(dataItem->AmplitudeCode);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void FillValueFromItem2_32(WMSensorData* sensorData, WMEcho32Raw* dataItem, float correctionSo)
        {
            if (IsInvalidCode(dataItem->TimeCode))
            {
                NativeMemoryApi.ZeroMemory((UIntPtr)sensorData, WMSensorData.Size);
            }
            else
            {
                sensorData->SO = GetSO(dataItem->TimeCode) + correctionSo;
                //   sensorData->AW2 = GetAW(dataItem->AmplitudeCode);
                dataItem++;
                sensorData->WT = GetWT(dataItem->TimeCode);
                //   sensorData->AW = GetAW(dataItem->AmplitudeCode);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void FillValueFromDecimalItem(WMSensorData* sensorData, WMEchoRawAligned* baseData, double correctionSo)
        {
            var so = (baseData->SO < 0 ? 0 : baseData->SO) / 10f + correctionSo;
            sensorData->SO = (float)(so < 0 ? 0 : so);
            sensorData->WT = baseData->WT / 10f;
            //sensorData->AW = amplitudeMapPtr[baseData->AW];
            //sensorData->AW2 = 0f;
        }

        #endregion

        #region Optimized WT Read Methods
        private unsafe void ReadByteRaws4D57Item2Wt(IDiagDataReader dataReader, int realScan, int refScan, object args)
        {
            if (realScan < 0) return;

            var scanFactor = wmParameters.ScanFactor;

            var result = (DataHandle<float>)args;
            
            foreach (var item in SensorsByDeltaScanOrdering.Items)
            {
                var deltaScan = scanFactor ? item.AdditionalAligmentInputInDeltaScan : item.DeltaScan;
                var scanOffset = IndexFile.GetDataPointer(dataReader, realScan + deltaScan);

                if (scanOffset == UIntPtr.Zero)
                    continue;

                foreach (var sensorNumber in item.SensorNumbers)
                {
                    var dataSensorIndex = SensorToSensorIndex((short)sensorNumber);

                    var sensorData = result.GetDataPointer(dataSensorIndex, refScan);
                    var baseData = (WMEchoRaw*)(scanOffset + sensorOffsetInRawScanMapPtr[sensorNumber]);

                    FillValueFromItem2Wt(sensorData, baseData);
                }
            }
        }

        private unsafe void ReadUShortRaws4D57Item2Wt(IDiagDataReader dataReader, int realScan, int refScan, object args)
        {
            if (realScan < 0) return;

            var scanFactor = wmParameters.ScanFactor;

            var result = (DataHandle<float>)args;

            foreach (var item in SensorsByDeltaScanOrdering.Items)
            {
                var deltaScan = scanFactor ? item.AdditionalAligmentInputInDeltaScan : item.DeltaScan;
                var scanOffset = IndexFile.GetDataPointer(dataReader, realScan + deltaScan);

                if (scanOffset == UIntPtr.Zero)
                    continue;

                foreach (var sensorNumber in item.SensorNumbers)
                {
                    var dataSensorIndex = SensorToSensorIndex((short)sensorNumber);

                    var sensorData = result.GetDataPointer(dataSensorIndex, refScan);
                    var baseData = (WMEcho32Raw*)(scanOffset + sensorOffsetInRawScanMapPtr[sensorNumber]);

                    FillValueFromItem2_32Wt(sensorData, baseData);
                }
            }
        }

        private unsafe void ReadByteRaws4D57Item3Wt(IDiagDataReader dataReader, int realScan, int refScan, object args)
        {
            if (realScan < 0) return;

            var scanFactor = wmParameters.ScanFactor;

            var result = (DataHandle<float>)args;

            foreach (var item in SensorsByDeltaScanOrdering.Items)
            {
                var deltaScan = scanFactor ? item.AdditionalAligmentInputInDeltaScan : item.DeltaScan;
                var scanOffset = IndexFile.GetDataPointer(dataReader, realScan + deltaScan);

                if (scanOffset == UIntPtr.Zero)
                    continue;

                foreach (var sensorNumber in item.SensorNumbers)
                {
                    var dataSensorIndex = SensorToSensorIndex((short)sensorNumber);

                    var sensorData = result.GetDataPointer(dataSensorIndex, refScan);
                    var baseData = (WMEchoRaw*)(scanOffset + sensorOffsetInRawScanMapPtr[sensorNumber]);

                    FillValueFromItem3Wt(sensorData, baseData);
                }
            }
        }

        private unsafe void ReadData4D58Wt(IDiagDataReader dataReader, int realScan, int refScan, object args)
        {
            if (realScan < 0) return;

            var result = (DataHandle<float>)args;

            foreach (var item in SensorsByDeltaScanOrdering.Items)
            {
                var scanOffset = IndexFile.GetDataPointer(dataReader, realScan);

                if (scanOffset == UIntPtr.Zero)
                    continue;

                foreach (var sensorNumber in item.SensorNumbers)
                {
                    var dataSensorIndex = SensorToSensorIndex((short)sensorNumber);

                    var sensorData = result.GetDataPointer(dataSensorIndex, refScan);
                    var baseData = (WMEchoRawAligned*)(scanOffset + sensorOffsetInAllignedRawScanMapPtr[sensorNumber]);

                    FillValueFromDecimalItemWt(sensorData, baseData);
                }
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void FillValueFromItem2Wt(float* sensorData, WMEchoRaw* dataItem)
        {
            if (IsInvalidCode(dataItem->TimeCode))
            {
                *sensorData = 0;
            }
            else
            {
                dataItem++;
                *sensorData = GetWT(dataItem->TimeCode);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void FillValueFromItem2_32Wt(float* sensorData, WMEcho32Raw* dataItem)
        {
            if (IsInvalidCode(dataItem->TimeCode))
            {
                *sensorData = 0;
            }
            else
            {
                dataItem++;
                *sensorData = GetWT(dataItem->TimeCode);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void FillValueFromDecimalItemWt(float* sensorData, WMEchoRawAligned* baseData)
        {
            *sensorData = baseData->WT / 10f;
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsInvalidCode(byte code)
        {
            return (code == 0x00 || code == 0xff);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsInvalidCode(ushort code)
        {
            return (code == 0x00 || code == 0xffff);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe float GetWT(byte wt)
        {
            return mmNextMapPtr[wt];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe float GetSO(byte so)
        {
            return mmFirstMapPtr[so];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe float GetWT(ushort wt)
        {
            return mmNextMapPtr[wt];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe float GetSO(ushort so)
        {
            return mmFirstMapPtr[so];
        }
        private unsafe void FillMaps()
        {
            amplitudeMapPtr = (float*)amplitudeMap.Data;
            timeFirstMapPtr = (float*)timeFirstMap.Data;
            timeNextMapPtr = (float*)timeNextMap.Data;
            mmFirstMapPtr = (float*)mmFirstMap.Data;
            mmNextMapPtr = (float*)mmNextMap.Data;

            var timeSleepZone = wmParameters.TimeSleepZone;
            var timeSleepZoneNext = wmParameters.TimeSleepZoneNext;
            var timeDiscretFirst = wmParameters.TimeDiscretFirst;
            var timeDiscretNext = wmParameters.TimeDiscretNext;
            var koeff = 4;

            speedMetalFactor = wmParameters.UltrasonicSpeedMetal / 2000f;
            speedOilFactor = wmParameters.UltrasonicSpeedOil / 2000f;

            if (CheckWM32())
            {
                Parallel.For(0, mapSize + 1, i =>
                {
                    amplitudeMapPtr[i] = koeff * (float)Math.Sqrt((i & 0x7F) * 2) * ((i & 0x80) > 0 ? -1 : 1) * Math.Sign(wmParameters.AmpMultiplicator);

                    timeFirstMapPtr[i] = (timeSleepZone + i * timeDiscretFirst) / 10;
                    timeNextMapPtr[i] = (timeSleepZoneNext + i * timeDiscretNext) / 10;
                    mmFirstMapPtr[i] = (float)Math.Round(timeFirstMapPtr[i] * speedOilFactor, 1, MidpointRounding.AwayFromZero);
                    mmNextMapPtr[i] = (float)Math.Round(timeNextMapPtr[i] * speedMetalFactor + wtCorrectionByCarrier, 1, MidpointRounding.AwayFromZero);
                });
            }
            else
            {
                Parallel.For(0, mapSize + 1, i =>
                {
                    amplitudeMapPtr[i] = koeff * (float)Math.Sqrt(i);

                    timeFirstMapPtr[i] = (timeSleepZone + i * timeDiscretFirst) / 10;
                    timeNextMapPtr[i] = (timeSleepZoneNext + i * timeDiscretNext) / 10;
                    mmFirstMapPtr[i] = (float)Math.Round(timeFirstMapPtr[i] * speedOilFactor, 1, MidpointRounding.AwayFromZero);
                    mmNextMapPtr[i] = (float)Math.Round(timeNextMapPtr[i] * speedMetalFactor + wtCorrectionByCarrier, 1, MidpointRounding.AwayFromZero);
                });
            }

            //Значения для кода 0x00 и 0xff
            mmFirstMapPtr[0] = 0;
            mmFirstMapPtr[mapSize] = 0;
            mmNextMapPtr[0] = 0;
            mmNextMapPtr[mapSize] = 0;

            sensorOffsetInRawScanMap = new VectorBuffer<int>(Heap, SensorCount, 1);
            sensorOffsetInRawScanMapPtr = (int*)sensorOffsetInRawScanMap.Data;

            var dataRawSize = DataRawSize();
            Parallel.For(0, SensorCount, k =>
            {
                sensorOffsetInRawScanMapPtr[k] =
                    packetHeaderSize +
                    k * dataRawSize;
            });

            sensorOffsetInAllignedRawScanMap = new VectorBuffer<int>(Heap, SensorCount, 1);
            sensorOffsetInAllignedRawScanMapPtr = (int*)sensorOffsetInAllignedRawScanMap.Data;


            var wmEchoRawAlignedSize = sizeof(WMEchoRawAligned);

            Parallel.For(0, SensorCount, k =>
            {
                sensorOffsetInAllignedRawScanMapPtr[k] =
                    packetHeaderSize +
                    k * wmEchoRawAlignedSize;
            });

            TriggerSO = mmFirstMapPtr[1];
            TriggerWT = mmNextMapPtr[1];
            TriggerWT = TriggerWT < 0.1f ? 0.1f : TriggerWT;
            MaxSO = mmFirstMapPtr[mapSize - 1];
            MaxWT = mmNextMapPtr[mapSize - 1];

      
            var maxdT = (int)(Math.Round(MaxWT * 100, MidpointRounding.AwayFromZero));
            echoInterlaceMap = new VectorBuffer<float>(Heap, maxdT, 1);
            var echoInterlaceMapPtr = (float*)echoInterlaceMap.Data;

            Parallel.For(0, maxdT, j =>
            {
                var dT = j / 100f;
                echoInterlaceMapPtr[j] = valueK * (1 - dT / valueT);
            });

            var thresholds = wmParameters.ThresholdsParameters.Thresholds;

            var existed = ThresholdsParameters.CheckThresholds(thresholds);

            if (existed)
            {
                var maxTime = (int)(thresholds[thresholds.Count - 1].Time * 100) + 1;
                var minTime = (int)(thresholds[0].Time * 100);

                thresholdsFastArray = new int[Math.Max(maxTime, minTime)];

                for (var i = 0; i < minTime; i++)
                    thresholdsFastArray[i] = thresholds[0].Ampl;

                for (var i = 1; i < thresholds.Count; i++)
                    for (var j = (int)(thresholds[i - 1].Time * 100) + 1; j <= (int)(thresholds[i].Time * 100); j++)
                        thresholdsFastArray[j] = thresholds[i].Ampl;
            }
            else
                thresholdsFastArray = null;
        }

        public float MaxWT { get; private set; }
        public float MaxSO { get; private set; }
        public float TriggerWT { get; private set; }
        public float TriggerSO { get; private set; }

        private void ReadCalibrations()
        {
            var basePath = String.Concat(Location.InspectionFullPath, Path.DirectorySeparatorChar,
                Location.BaseName, DataDescription.DataDirSuffix, Location.BaseName);

            var path = String.Concat(basePath, "SO.clb");
            if (!File.Exists(path))
                return;

            var calibrationsSerializer = new XmlSerializer(typeof(Calibrations));

            Calibrations calibration;

            var reader = new StreamReader(path);

            try
            {
                calibration = (Calibrations)calibrationsSerializer.Deserialize(reader);
            }
            catch (Exception)
            {
                return;
            }
            finally
            {
                reader.Close();
            }
            calibrations = calibration;
        }
        public float[] GetCorrection(double dist)
        {
            var leftItem = calibrations.Values.LastOrDefault(item => item.Distance < dist);
            var rightItem = calibrations.Values.FirstOrDefault(item => item.Distance > dist);

            return MergeCalibrations(leftItem, rightItem, dist);
        }
        private float[] MergeCalibrations(Calibration leftItem, Calibration rightItem, double dist)
        {
            var curCalibrations = new float[Carrier.SensorCount];

            if (leftItem == null)
            {
                if (rightItem == null || rightItem.SensorValueOffset.Length != curCalibrations.Length)
                {
                    return curCalibrations;
                }

                Array.Copy(rightItem.SensorValueOffset, curCalibrations, curCalibrations.Length);
            }
            else
            {
                if (leftItem.SensorValueOffset.Length != curCalibrations.Length)
                {
                    return curCalibrations;
                }

                if (rightItem == null)
                {
                    Array.Copy(leftItem.SensorValueOffset, curCalibrations, curCalibrations.Length);
                }
                else
                {
                    var coef = (float)((rightItem.Distance - dist) / (rightItem.Distance - leftItem.Distance));

                    for (var i = 0; i < curCalibrations.Length; i++)
                    {
                        curCalibrations[i] = (float)Math.Round(
                            coef * leftItem.SensorValueOffset[i] + (1 - coef) * rightItem.SensorValueOffset[i],
                            1, MidpointRounding.AwayFromZero);
                    }
                }
            }

            return curCalibrations;
        }

       public unsafe new short SensorToSensorIndex(short sensor)
        {
            return base.SensorToSensorIndex(sensor);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateGistoSO(float value)
        {
            soHistogram.Update(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateGistoWT(float value)
        {
            wtHistogram.Update(value);
        }

        private void ResetNominals()
        {
            ResetWTNominal();
            ResetSONominal();
        }

        private void ResetSONominal()
        {
            soHistogram.Reset();
        }

        private void ResetWTNominal()
        {
            wtHistogram.Reset();
        }

        private byte EchoRawSize()
        {
            return (byte)(wmParameters.RawEchoSize == enRawEchoSize._16bit ? sizeof(byte) * 2 : sizeof(ushort) * 2);
        }

        private byte DataRawSize()
        {
            return (byte)(3 * EchoRawSize());
        }

        protected override void InnerClose()
        {
            amplitudeMap?.Dispose();
            timeFirstMap?.Dispose();
            timeNextMap?.Dispose();
            mmFirstMap?.Dispose();
            mmNextMap?.Dispose();
            sensorEchos3?.Dispose();
            wtHistogram?.Dispose();
            soHistogram?.Dispose();
            echoInterlaceMap?.Dispose();
            sensorOffsetInAllignedRawScanMap?.Dispose();
           
            
            base.InnerClose();

        }
    }
}

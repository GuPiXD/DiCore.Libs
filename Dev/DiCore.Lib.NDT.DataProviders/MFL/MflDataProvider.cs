using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Diascan.NDT.Enums;
using DiCore.Lib.NDT.Types;

namespace DiCore.Lib.NDT.DataProviders.MFL
{
    public abstract class MflDataProvider:BaseDataProvider
    {
        private readonly DataType dataType;
        private int sensorRawSize = 2;
        private MflDeviceParameters mflParameters;
        private const int DimensionCode = 0x0fff;
        protected bool InvertAmpl = false;

        private readonly unsafe int packetHeaderSize = sizeof(MFLDataPacketHeader);

        /// <summary>
        /// Размерность кодов амплитуды
        /// </summary>
        public int AmplitudeResolution { get; private set; }

        public int MflId => mflParameters.MflId;

        private float[,] amplitudeMap;

        /// <summary>
        /// Получить/загрузить карту преобразования код амплитуды -> значение амплитуды. Размерность: количество датчиков (SensorCount) на разрешение по амплитуде (AmplitudeResolution)
        /// </summary>
        public float[,] AmplitudeMap
        {
            get => amplitudeMap;
            set
            {
                if (amplitudeMap == null || value == null) return;

                if (value.GetLength(0) != amplitudeMap.GetLength(0) || value.GetLength(1) != amplitudeMap.GetLength(1))
                    return;

                amplitudeMap = value;

                CalibrationMode = CalibrationMode.Custom;
            }
        }

        /// <summary>
        /// Текущий режим калибровки датчиков
        /// </summary>
        public CalibrationMode CalibrationMode { get; private set; }

        protected MflDataProvider(UIntPtr heap, DataType mflDataType) : base(heap, GetDescription(mflDataType))
        {
            dataType = mflDataType;
        }

        protected override int CalculateMaxScanSize()
        {
            mflParameters = (MflDeviceParameters)Parameters;

            return packetHeaderSize + Carrier.SensorCount * sensorRawSize;
        }

        protected override IDeviceParameters LoadParameters(DataLocation location)
        {
            return MflDeviceParameters.LoadFromOmni(location.FullPath, dataType);
        }

        private static DiagdataDescription GetDescription(DataType mflDataType)
        {
            switch (mflDataType)
            {
                case DataType.MflT1:
                    return Constants.MFLT1DataDescription;
                case DataType.MflT11:
                    return Constants.MFLT11DataDescription;
                case DataType.MflT2:
                    return Constants.MFLT2DataDescription;
                case DataType.MflT22:
                    return Constants.MFLT22DataDescription;
                case DataType.MflT3:
                    return Constants.MFLT3DataDescription;
                case DataType.MflT31:
                    return Constants.MFLT31DataDescription;
                case DataType.MflT32:
                    return Constants.MFLT32DataDescription;
                case DataType.MflT33:
                    return Constants.MFLT33DataDescription;
                case DataType.MflT34:
                    return Constants.MFLT34DataDescription;
                case DataType.TfiT4:
                    return Constants.TFIDataDescription;
                case DataType.TfiT41:
                    return Constants.MFLT41DataDescription;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mflDataType), mflDataType, null);
            }
        }

        protected override string BuildDataPath()
        {
            return String.Concat(Location.InspectionFullPath, @"\", Location.BaseName, DataDescription.DataDirSuffix, Location.BaseName);
        }

        public override string Name => $"MFL Data Provider {dataType}";
        public override string Description => $"Implement reading {dataType} data";
        public override string Vendor => "Vagner I.";

        public override bool Open(DataLocation location)
        {
            sensorRawSize = 2;

            if (!base.Open(location)) return false;

            CheckSensorSize();

            if (sensorRawSize != 2)
                ReCreateIndexFileManager();

            CreateAmplitudeMap();

            return true;
        }

        public DataHandle<float> GetData(int scanStart, int scanCount, int compressStep = 1)
        {
            DefineReadScans(scanStart, scanCount, compressStep);

            CalcAligningSensors(scanStart, scanCount, compressStep, AlignmentFactor, SensorsByDeltaScanOrdering);

            var result = new DataHandle<float>(SensorCount, scanCount);

            var args = result;

            if (IndexFile.FileType == DataDescription.TypeCode)
            {
                if (sensorRawSize == 1)
                {
                    ExecuteBaseReadScans(FillDataBufferCol1Bytes, args, false);
                }
                else
                {
                    ExecuteBaseReadScans(FillDataBufferCol2Bytes, args, false);
                }
            }
            else if (IndexFile.FileType == DataDescription.ReverseTypeCode)
            {
                ExecuteBaseReadScans(FillReversDataBufferCol, args, false);
            }

            return result;
        }


        private unsafe void FillDataBufferCol1Bytes(IDiagDataReader dataReader, int realScan, int refScan, object args)
        {
            if (realScan < 0) return;

            var scanFactor = mflParameters.ScanFactor;

            var result = (DataHandle<float>) args;

            foreach (var item in SensorsByDeltaScanOrdering.Items)
            {
                var deltaScan = scanFactor ? item.AdditionalAligmentInputInDeltaScan : item.DeltaScan;
                var scanOffset = IndexFile.GetDataPointer(dataReader, realScan + deltaScan);

                if (scanOffset == UIntPtr.Zero)
                    continue;

                var basePtr = (byte*)(scanOffset + packetHeaderSize);

                foreach (var sensorNumber in item.SensorNumbers)
                {
                    var dataSensorIndex = SensorToSensorIndex((short)sensorNumber);

                    var sensorData = result.GetDataPointer(dataSensorIndex, refScan);
                    var amplitudeCode = NormalizeAmplCode(basePtr[sensorNumber]);

                    *sensorData = AmplitudeMap[sensorNumber, amplitudeCode];
                }
            }
        }

        private unsafe void FillDataBufferCol2Bytes(IDiagDataReader dataReader, int realScan, int refScan, object args)
        {
            if(realScan < 0) return;

            var scanFactor = mflParameters.ScanFactor;

            var result = (DataHandle<float>)args;

            foreach (var item in SensorsByDeltaScanOrdering.Items)
            {
                var deltaScan = scanFactor ? item.AdditionalAligmentInputInDeltaScan : item.DeltaScan;
                var scanOffset = IndexFile.GetDataPointer(dataReader, realScan + deltaScan);

                if (scanOffset == UIntPtr.Zero)
                    continue;

                var basePtr = (ushort*)(scanOffset + packetHeaderSize);

                foreach (var sensorNumber in item.SensorNumbers)
                {
                    var dataSensorIndex = SensorToSensorIndex((short)sensorNumber);

                    var sensorData = result.GetDataPointer(dataSensorIndex, refScan);
                    var amplitudeCode = NormalizeAmplCode(basePtr[sensorNumber]);

                    *sensorData = AmplitudeMap[sensorNumber, amplitudeCode];
                }
            }
        }

        private unsafe void FillReversDataBufferCol(IDiagDataReader dataReader, int realScan, int refScan, object args)
        {
            if (realScan < 0) return;

            var scanOffset = IndexFile.GetDataPointer(dataReader, realScan);

            if (scanOffset == UIntPtr.Zero)
                return;

            var result = (DataHandle<float>) args;
            var basePtr = (ushort*) (scanOffset + packetHeaderSize);

            for (var sensorNumber = 0; sensorNumber < SensorCount; sensorNumber++)
            {
                var dataSensorIndex = SensorToSensorIndex((short) sensorNumber);

                var sensorData = result.GetDataPointer(dataSensorIndex, refScan);
                var amplitudeCode = NormalizeAmplCode(basePtr[sensorNumber]);

                *sensorData = AmplitudeMap[sensorNumber, amplitudeCode];
            }
        }

        private void CreateAmplitudeMap()
        {
            switch (sensorRawSize)
            {
                case 1:
                    AmplitudeResolution = byte.MaxValue + 1;
                    break;

                case 2:
                    AmplitudeResolution = 0xFFF + 2; //12 битное АЦП //ushort.MaxValue + 1;
                    break;
            }

            amplitudeMap = new float[Carrier.SensorCount, AmplitudeResolution];

            ApplyOmniCalibrationMode();
            CorrectAmplitudeSign();
        }

        /// <summary>
        /// Инвертирование амплитуды сигналов, для датчиков MflT31,MflT32,MflT33,MflT34
        /// </summary>
        protected void CorrectAmplitudeSign()
        {
            if (InvertAmpl)
                for (var sensorIndex = 0; sensorIndex < SensorCount; sensorIndex++)
                    for (var i = 0; i < AmplitudeResolution; i++)
                        amplitudeMap[sensorIndex, i] = -amplitudeMap[sensorIndex, i];
        }

        /// <summary>
        /// Применить калибровку из omni файла (по умолчанию)
        /// </summary>
        public void ApplyOmniCalibrationMode()
        {
            // заполняем карту амплитуд
            Parallel.For(0, Carrier.SensorCount, FillAmplitudeMapForSensor);

            CalibrationMode = CalibrationMode.Omni;
        }

        private void FillAmplitudeMapForSensor(int sensorIndex)
        {
            if (sensorIndex >= Carrier.SensorCount) return;

            var fsDiv1000 = mflParameters.FactorSensitivity / 1000f;
            var nominal = mflParameters.NominalOffset;

            // заполняем карту амплитуд
            var sensorCalibration = mflParameters.SensorsCalibrations.FirstOrDefault(item => (item.Number - 1) == sensorIndex);
            var sensNominal = sensorCalibration?.NominalOffset ?? nominal;
            var sensFactorSensitivity = sensorCalibration?.FactorSensitivity / 1000f ?? fsDiv1000;

            for (var i = 0; i < AmplitudeResolution; i++)
                amplitudeMap[sensorIndex, i] = (i - sensNominal) * sensFactorSensitivity;
        }

        /// <summary>
        /// Проверка размера данных датчика
        /// </summary>
        /// <returns>Результат проверки</returns>
        private unsafe bool CheckSensorSize()
        {
            var scanOffset = UIntPtr.Zero;
            for (var i = 0; i < 10; i++)
            {
                scanOffset = IndexFile.GetDataPointer(IndexFile.FirstScan + i);
                if (scanOffset != UIntPtr.Zero)
                    break;
            }

            if (scanOffset == UIntPtr.Zero)
                return false;

            // получаем заголовок
            var packetHeader = (MFLDataPacketHeader*)scanOffset;

            var needSize = Carrier.SensorCount * mflParameters.SizeSensor + packetHeaderSize;

            var result = needSize == packetHeader->Size;

            if (!result)
                sensorRawSize = (byte)((packetHeader->Size - packetHeaderSize) / Carrier.SensorCount);
            else
                sensorRawSize = (byte)(mflParameters.SizeSensor);

            return result;
        }

        private ushort NormalizeAmplCode(ushort code)
        {
            return (ushort)(code & DimensionCode);
        }

        public Dictionary<int, List<int>> GetSensorsByGroup()
        {
            var result = new Dictionary<int, List<int>>();

            foreach (var sensors in Carrier)
            {
                var parity = sensors.SkiNumber % 2;

                if (!result.ContainsKey(parity))
                    result[parity] = new List<int>();

                result[parity].Add(sensors.LogicalNumber - 1);
            }
            return result;
        }
    }

    #region Secret part

    public static class MflProviderFactory
    {
        public static MflDataProvider Create(UIntPtr heap, DataType dataType)
        {
            switch (dataType)
            {
                case DataType.MflT1:
                    return new MflT1DataProvider(heap);
                case DataType.MflT11:
                    return new MflT11DataProvider(heap);
                case DataType.MflT2:
                    return new MflT2DataProvider(heap);
                case DataType.MflT22:
                    return new MflT22DataProvider(heap);
                case DataType.MflT3:
                    return new MflT3DataProvider(heap);
                case DataType.MflT31:
                    return new MflT31DataProvider(heap);
                case DataType.MflT32:
                    return new MflT32DataProvider(heap);
                case DataType.MflT33:
                    return new MflT33DataProvider(heap);
                case DataType.MflT34:
                    return new MflT34DataProvider(heap);
                case DataType.TfiT4:
                    return new TfiT4DataProvider(heap);
                case DataType.TfiT41:
                    return new TfiT41DataProvider(heap);
                default:
                    return null;
            }
        }
    }

    public class MflT1DataProvider : MflDataProvider
    {
        public MflT1DataProvider(UIntPtr heap) : base(heap, DataType.MflT1)
        {
        }
    }
    public class MflT11DataProvider : MflDataProvider
    {
        public MflT11DataProvider(UIntPtr heap) : base(heap, DataType.MflT11)
        {
        }
    }
    public class MflT2DataProvider : MflDataProvider
    {
        public MflT2DataProvider(UIntPtr heap) : base(heap, DataType.MflT2)
        {
        }
    }
    public class MflT22DataProvider : MflDataProvider
    {
        public MflT22DataProvider(UIntPtr heap) : base(heap, DataType.MflT22)
        {
        }
    }
    public class MflT3DataProvider : MflDataProvider
    {
        public MflT3DataProvider(UIntPtr heap) : base(heap, DataType.MflT3)
        {
        }
    }
    public class MflT31DataProvider : MflDataProvider
    {
        public MflT31DataProvider(UIntPtr heap) : base(heap, DataType.MflT31)
        {
            InvertAmpl = true;
        }
    }
    public class MflT32DataProvider : MflDataProvider
    {
        public MflT32DataProvider(UIntPtr heap) : base(heap, DataType.MflT32)
        {
            InvertAmpl = true;
        }
    }
    public class MflT33DataProvider : MflDataProvider
    {
        public MflT33DataProvider(UIntPtr heap) : base(heap, DataType.MflT33)
        {
            InvertAmpl = true;
        }
    }
    public class MflT34DataProvider : MflDataProvider
    {
        public MflT34DataProvider(UIntPtr heap) : base(heap, DataType.MflT34)
        {
            InvertAmpl = true;
        }
    }
    public class TfiT4DataProvider : MflDataProvider
    {
        public TfiT4DataProvider(UIntPtr heap) : base(heap, DataType.TfiT4)
        {
        }
    }
    public class TfiT41DataProvider : MflDataProvider
    {
        public TfiT41DataProvider(UIntPtr heap) : base(heap, DataType.TfiT41)
        {
        }
    }

    #endregion
}

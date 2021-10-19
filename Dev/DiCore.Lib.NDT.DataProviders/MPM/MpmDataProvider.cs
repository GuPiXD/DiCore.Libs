using System;
using System.Runtime.ExceptionServices;
using DiCore.Lib.NDT.Types;

namespace DiCore.Lib.NDT.DataProviders.MPM
{
    public class MpmDataProvider : BaseDataProvider
    {
        private MpmDeviceParameters mpmParameters;

        private readonly unsafe int packetHeaderSize = sizeof(MPMDataPacketHeader);
        private readonly int sensorDataRawSize = sizeof(ushort);

        /// <summary>
        /// Карта значений
        /// </summary>
        private float[,] valueMap;

        /// <summary>
        /// Двумерный массив значений АЦП рычагов для рассчёта калибровки
        /// </summary>
        private uint[,] leversValues;

        public override string Name => "MPM Data Provider";
        public override string Description => "Implement reading MPM data";
        public override string Vendor => "Gnetnev A.";

        public MpmDataProvider(UIntPtr heap) : base(heap, Constants.PFDataDescription)
        {
        }

        public override bool Open(DataLocation location)
        {
            if (!base.Open(location)) return false;

            FillMap();

            return true;
        }

        private void FillMap()
        {
            var valueResoution = ushort.MaxValue;
            valueMap = new float[Carrier.SensorCount, valueResoution];

            PrepareLeversInfo();

            for (var i = 0; i < Carrier.SensorCount; i++)
            {
                for (var j = 0; j < valueResoution; j++)
                    valueMap[i, j] = Calibrate(i, j);
            }
        }

        private void PrepareLeversInfo()
        {
            var separators = new[] {' '};
            var rows = mpmParameters.Calibration.Plates.Count;
            var cols = Carrier.SensorCount;

            leversValues = new uint[rows, cols];

            for (var i = 0; i < rows; i++)
            {
                var sBuffer = mpmParameters.Calibration.Levers[i].Signal;
                var subStrings = sBuffer.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                if (subStrings.Length < cols)
                {
                    IsOpened = false;
                    throw new ArgumentOutOfRangeException();
                }

                for (var j = 0; j < cols; j++)
                {
                    if (ushort.TryParse(subStrings[j], out var val))
                        leversValues[i, j] = val;
                }
            }
        }

        private float Calibrate(int leverNumber, int sourceValue)
        {
            float valueMM = 0;
            uint denominator;
            bool isAscending;
            // На входе значение АЦП
            // на выходе расчёт калибровки в мм

            // 22.05.2013
            // Для 14" ОПТ изменился наклон калибровочной характеристики. Поэтому, 2 варианта калибровки:
            var plateList = mpmParameters.Calibration.Plates;

            // Определить наклон кривой
            isAscending = leversValues[0, 0] < leversValues[plateList.Count - 1, 0];

            if (!isAscending)
            {
                //Старый тип калибровки
                // Определить, в каком диапазоне значение АЦП...
                // 1. Если значение АЦП больше значения последней точки калибровочной характеристики...
                // Из пропорции: x1 - xi / x1 - x0 = (fxi- fx1) / (fx0 - fx1)

                if (sourceValue > leversValues[0, leverNumber])
                {
                    // Вычислить знаменатель...
                    try
                    {
                        denominator = leversValues[0, leverNumber] - leversValues[1, leverNumber];
                    }
                    catch
                    {
                        denominator = 0;
                    }

                    // Знаменатель не вычисляется = калибровка в точке
                    if (denominator != 0)
                    {
                        valueMM = plateList[1].Deviation - (plateList[1].Deviation - plateList[0].Deviation)
                                  * (sourceValue - leversValues[1, leverNumber]) / denominator;
                    }
                    else
                    {
                        valueMM = plateList[0].Deviation;
                    }
                }

                // 2. Если значение АЦП меньше значения последней точки калибровочной характеристики...

                else if (sourceValue < leversValues[plateList.Count - 2, leverNumber]) // -1
                {
                    // Вычислить знаменатель
                    try
                    {
                        denominator = leversValues[plateList.Count - 2, leverNumber] -
                                      leversValues[plateList.Count - 1, leverNumber];
                    }
                    catch
                    {
                        denominator = 0;
                    }

                    // Знаменатель не вычисляется - калибровка в точке
                    if (denominator != 0)
                    {
                        valueMM = plateList[plateList.Count - 2].Deviation + (plateList[plateList.Count - 1].Deviation -
                                                                              plateList[plateList.Count - 2]
                                                                                  .Deviation) *
                                  (leversValues[plateList.Count - 2, leverNumber] - sourceValue) / denominator;
                    }
                    else
                    {
                        valueMM = plateList[plateList.Count - 1].Deviation;
                    }
                }

                // 3. Если значение АЦП внутри калибровочной характеристики...
                // Для остальных значений...

                else
                {
                    for (var i = 1; i < plateList.Count; i++)
                    {
                        if (sourceValue >= leversValues[i, leverNumber])
                        {
                            // Знаменатель...
                            try
                            {
                                denominator = leversValues[i - 1, leverNumber] - leversValues[i, leverNumber];
                            }
                            catch
                            {
                                denominator = 0;
                            }

                            if (denominator != 0)
                            {
                                valueMM = (plateList[i].Deviation - plateList[i - 1].Deviation)
                                          * (leversValues[i - 1, leverNumber] - sourceValue) / denominator +
                                          plateList[i - 1].Deviation;
                            }
                            else
                            {
                                // Калибровка в точке
                                valueMM = plateList[i - 1].Deviation +
                                          ((plateList[i].Deviation - plateList[i - 1].Deviation) / 2);
                            }

                            break;
                        }
                    }
                }
            }
            else
            {
                // Новый алгоритм
                // Определить диапазон для расчёта (с конца)
                if (sourceValue >= leversValues[plateList.Count - 1, leverNumber])
                {
                    // Больше последнего значения - аппроксимация по последнему отрезку
                    // Вычисление знаменателя (АЦП конца участка - АЦП начала участка)
                    denominator = leversValues[plateList.Count - 1, leverNumber] -
                                  leversValues[plateList.Count - 2, leverNumber];
                    if (denominator != 0)
                    {
                        valueMM = plateList[plateList.Count - 2].Deviation +
                                  (plateList[plateList.Count - 1].Deviation -
                                   plateList[plateList.Count - 2].Deviation) *
                                  (sourceValue - leversValues[plateList.Count - 2, leverNumber]) / denominator;

                    }
                    else
                    {
                        // знаменатель 0 - калибровка в точке
                        valueMM = plateList[plateList.Count - 1].Deviation;
                    }
                }
                else
                {
                    if (sourceValue <= leversValues[0, leverNumber])
                    {
                        // Меньше первого значения - аппроксимация по первому отрезку
                        // Вычисление знаменателя (АЦП конца участка - АЦП начала участка)
                        denominator = leversValues[1, leverNumber] - leversValues[0, leverNumber];

                        if (denominator != 0)
                        {
                            valueMM = plateList[0].Deviation + (plateList[1].Deviation - plateList[0].Deviation) *
                                      (sourceValue - leversValues[0, leverNumber]) / denominator;
                        }
                        else
                        {
                            // знаменатель 0 - калибровка в точке
                            valueMM = plateList[0].Deviation;
                        }
                    }
                    else
                    {
                        // Поиск в диапазонах
                        for (var i = 1; i < plateList.Count; i++)
                        {
                            if ((sourceValue < leversValues[i, leverNumber]) &&
                                (sourceValue >= leversValues[i - 1, leverNumber]))
                            {
                                // Диапазон найден
                                denominator = leversValues[i, leverNumber] - leversValues[i - 1, leverNumber];

                                if (denominator != 0)
                                {
                                    valueMM = plateList[i - 1].Deviation +
                                              (plateList[i].Deviation - plateList[i - 1].Deviation) *
                                              (sourceValue - leversValues[i - 1, leverNumber]) / denominator;
                                }
                                else
                                {
                                    // знаменатель 0 - калибровка в точке
                                    valueMM = plateList[i - 1].Deviation +
                                              ((plateList[i].Deviation - plateList[i - 1].Deviation) / 2);
                                }
                            }
                        }
                    }
                }
            }

            // Окончательная обработка: приведение к внешнему диаметру трубы
            // 12.05.2014 По запросу Масленникова М.В. получается двойное приведение к наружнему диаметру при расчете сужения проходного сечения и овальности
            // Закомментировать
            // 15.05.2014 По запросу Масленникова М.В. (НО ООИ) всё назад
            valueMM = valueMM - (plateList[0].RealTh - mpmParameters.PipeDiameterMm) / 2f;

            return valueMM;
        }

        protected override string BuildDataPath()
        {
            return String.Concat(Location.InspectionFullPath, @"\", Location.BaseName, DataDescription.DataDirSuffix,
                Location.BaseName);
        }

        protected override int CalculateMaxScanSize()
        {
            mpmParameters = (MpmDeviceParameters) Parameters;

            return packetHeaderSize + Carrier.SensorCount * sensorDataRawSize;
        }

        protected override IDeviceParameters LoadParameters(DataLocation location)
        {
            return MpmDeviceParameters.LoadFromOmni(location.FullPath);
        }

        public DataHandle<float> GetData(int scanStart, int scanCount, int compressStep = 1)
        {
            var result = new DataHandle<float>(SensorCount, scanCount);

            DefineReadScans(scanStart, scanCount, compressStep);

            CalcAligningSensors(scanStart, scanCount, compressStep, AlignmentFactor, SensorsByDeltaScanOrdering);

            ExecuteBaseReadScans(ReadScan, result, false);

            return result;
        }

        private unsafe void ReadScan(IDiagDataReader dataReader, int realScan, int refScan, object args)
        {
            if (realScan < 0) return;

            var result = (DataHandle<float>) args;

            foreach (var item in SensorsByDeltaScanOrdering.Items)
            {
                var deltaScan = mpmParameters.ScanFactor ? item.AdditionalAligmentInputInDeltaScan : item.DeltaScan;

                var scanOffset = IndexFile.GetDataPointer(realScan + deltaScan);

                if (scanOffset == UIntPtr.Zero)
                    continue;

                foreach (var sensorNumber in item.SensorNumbers)
                {
                    var dataSensorIndex = SensorToSensorIndex((short) sensorNumber);

                    var sensorData = result.GetDataPointer(dataSensorIndex, refScan);
                    var baseData = (ushort*) (scanOffset + packetHeaderSize + sensorNumber * sensorDataRawSize);

                    *sensorData = valueMap[sensorNumber, *baseData];
                }
            }
        }
    }
}

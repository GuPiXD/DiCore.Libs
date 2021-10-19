using System;
using Diascan.Utils.FileMapper;
using DiCore.Lib.CoordinateProvider;
using DiCore.Lib.NDT.CoordinateProvider.Properties;
using DiCore.Lib.NDT.Types;

namespace DiCore.Lib.NDT.CoordinateProvider
{
    public class CoordinateDataProviderCrop : IDataProvider,ICoordinateProvider
    {
        #region Vars

        private FileMapper fileMapper;
        private IdtFile fileIDT;

        private const int iodRate = 7;
        private const int iotRate = 9;

        private const int maxCountStep = 60;
        private const int maxCountStepDiv2 = 30;

        internal CoordinateItem firstRecordInFile;
        private CoordinateItem lastRecordInFile;

        internal int odometerPeriod;
        private int scanPeriod;
        private int timePeriod;

        private int recordSize;
        private int recordCount;

        private int firstRecordNumberInBuffer;
        private Range<int> recordsInBufferRange;
        private unsafe CoordinateItem* coordinateItems;

        private CoordinateData uninterruptedCoordinateData;
        private CoordinateData virtualCoordinateData;
        private readonly UIntPtr heap;
        private double minDistance;
        private double maxDistance;
        private uint averageTimePerOdometer;

        public int RecordCount => recordCount;

        private CorruptedDataInfo corruptedDataInfo;

        /// <summary>
        /// Информация о восстановлении поврежденных данных
        /// </summary>
        private CcdScanDeltaManager ccdScanDeltaManager;

        #endregion

        #region Overrides of BaseService

        public string Name => "Сервер данных для работы с координатной информацией";

        public string Description => "Сервер данных для работы с координатной информацией";

        public string Vendor => "Вагнер И.А.";

        #endregion        

        #region Property Interface

        /// <summary>
        /// Дата провайдер открыт
        /// </summary>
        public bool IsOpened => fileMapper != null;

        /// <summary>
        /// Смещение по дистанции (используется для расчета количество сканов в кадре)
        /// </summary>
        public double SectionOffset { get; set; }       

        public double MinDistance => minDistance + CalcParameters.DistanceOffset;

        public double MaxDistance => maxDistance + CalcParameters.DistanceOffset;

        public int MaxScan { get; private set; }
        public int MinScan { get; private set; }
        public short SensorCount => 0;

        /// <summary>
        /// Параметры, используемые для вычисления 
        /// </summary>
        public CalcParameters CalcParameters { get; }

        public CoordinateItem FirstRecord => firstRecordInFile;

        public CoordinateItem LastRecord => lastRecordInFile;

        private string idtPath;
        private string dataPath;
        private float pipeCircleModulFactor;
        private float mmInDegree;

        public DataLocation Location { get; set; }

        #endregion

        #region Функции по работе с IndexFile

        /// <summary>
        /// Провайдер использует индексный файл IDT
        /// </summary>
        public bool UseIDT { get; private set; }

        private void CheckIDT()
        {
            var hashLow = Diascan.Utils.IO.File.GetCreationTime(dataPath).ToBinary();
            var hashHigh = Diascan.Utils.IO.File.GetLastWriteTime(dataPath).ToBinary();

            fileIDT = new IdtFile();

            UseIDT = fileIDT.CheckIndexFile(idtPath, hashLow, hashHigh);
        }        

        private string BuildIDTPath()
        {
            return String.Concat(Location.InspectionFullPath, @"\", Location.BaseName, ".idt");
        }

        #endregion

        #region ICoordinateDataProvider

        public void GetCustomDistRuler(DistRuler result, int scanStart, int count, int step)
        {
            result.ReAllocate(count);

            if (!virtualCoordinateData.CheckCoordinateDataByInterval(scanStart, count))
            {
                BuildVirtualCoordinateData(scanStart);
            }

            unsafe
            {
                for (var i = 0; i < count; i ++)
                {
                    var scan = scanStart + i*step;
                    result.DataPointer[i].Scan = scan;
                    double dist;
                    if (!virtualCoordinateData.GetDist(scan, out dist))
                    {
                        BuildVirtualCoordinateData(scan);
                        virtualCoordinateData.GetDist(scan, out dist);
                    }
                    result.DataPointer[i].Dist = dist - CalcParameters.DistanceOffset;
                    virtualCoordinateData.GetTime(scan, out result.DataPointer[i].Time);
                }
            }

            result.BuildDistIndex();
        }
        
        public int Dist2Scan(double dist)
        {
            var needVirtualOdometer = Dist2Odometer(dist + CalcParameters.DistanceOffset);

            TryPrepareCoordinateData(needVirtualOdometer);

            bool? isSmaller;
            return virtualCoordinateData.GetScan(needVirtualOdometer, out isSmaller) ?? VirtualOdometer2VirtualScanSlow(needVirtualOdometer);
        }

        public int Dist2DataTypeScan(double distance, double distanceOffset)
        {
            return Dist2Scan(distance + distanceOffset);
        }

        public double Scan2DataTypeDist(int scan, double distanceOffset)
        {
            return Scan2Dist(scan) - distanceOffset;
        }

        private void TryPrepareCoordinateData(double needVirtualOdometer)
        {
            var odometer10 = (int)Math.Floor(needVirtualOdometer * 10);
            bool isSmallerThenRange;
            var coordinateDataReady = virtualCoordinateData.CheckCoordinateDataByOdometer(odometer10,
                out isSmallerThenRange);

            var probableScan = coordinateDataReady
                ? 0
                : VirtualOdometer2VirtualScanSlow(needVirtualOdometer);

            var border = false;

            while (!coordinateDataReady && !border)
            {
                var buildVirtualScanStart = Math.Max((int) MinScan, probableScan - virtualCoordinateData.Count / 2);
                border = buildVirtualScanStart == MinScan || probableScan == MaxScan;
                BuildVirtualCoordinateData(buildVirtualScanStart);

                coordinateDataReady = virtualCoordinateData.CheckCoordinateDataByOdometer(odometer10,
                    out isSmallerThenRange);

                if (isSmallerThenRange)
                    probableScan = probableScan -  virtualCoordinateData.Count / 2;
                else
                    probableScan = Math.Min(probableScan + virtualCoordinateData.Count / 2, (int)MaxScan);
            }
        }

        private double Dist2Odometer(double dist)
        {
            return dist <= 0 ? 0 : dist / CalcParameters.OdoFactor;
        }

        /// <summary>
        /// Быстрый и точный способ получения дистанции по скану, но только в пределах рабочего пространства т.е. +/- сотни метров, иначе будет обычный
        /// </summary>
        /// <param name="scan">Скан</param>
        /// <returns>Дистанция в метрах</returns>
        public double Scan2Dist(int scan)
        {
            double dist;
            return (virtualCoordinateData.GetDist(scan, out dist) ? dist : VirtualScan2Dist(scan)) - CalcParameters.DistanceOffset;
        }

        public double DataTypeScan2Dist(int scan, double distanceOffset)
        {
            return Scan2Dist(scan) - distanceOffset;
        }

        public double Time2Dist(uint ccdTime)
        {
            var ccdScan = CcdTime2CcdScanSlow((int) ccdTime);
            var virtualScan = CcdScan2VirtualScan(ccdScan);

            return VirtualScan2Dist(virtualScan);
        }

        #endregion

        #region BaseDataProvider

        protected string BuildDataPath()
        {
            return String.Concat(Location.InspectionFullPath, @"\", Location.BaseName, ".ccd");
        }

        protected void InnerClose()
        {
            fileMapper?.Dispose();
            fileIDT?.Dispose();
            uninterruptedCoordinateData?.Dispose();
            virtualCoordinateData?.Dispose();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Открытие диагностической сессии
        /// </summary>        
        /// <returns></returns>
        public bool Open(DataLocation location)
        {
            Location = location;
            dataPath = BuildDataPath();

            idtPath = BuildIDTPath();

            CheckIDT();

            fileMapper = new FileMapper(dataPath);

            recordSize = CoordinateDataProviderHelper.COORDINATE_ITEM_SIZE;
            if ((fileMapper.FileSize % recordSize) != 0)
                return false;
            recordCount = Convert.ToInt32(fileMapper.FileSize / recordSize);
            if (recordCount <= 1)
                return false;

            if (!SetPointers(0))
                return false;

            lastRecordInFile = ReadRecord(recordCount - 1);
            firstRecordInFile = ReadRecord(0);

            FillPeriods();

            var defBufferSize = 1048576 / CoordinateDataProviderHelper.COORDINATE_ITEM_FLOAT_SIZE;

            var rawCCDBufferSize = defBufferSize > recordCount * scanPeriod ? recordCount * scanPeriod : defBufferSize;

            //     bufferSize = (bufferSize/scanPeriod)*scanPeriod;

            uninterruptedCoordinateData = new CoordinateData(heap, rawCCDBufferSize)
            {
                StepUpBuffer = rawCCDBufferSize
            };

            virtualCoordinateData = new CoordinateData(heap, defBufferSize)
            {
                StepUpBuffer = defBufferSize,
                OdoFactor = CalcParameters.OdoFactor
            };

            corruptedDataInfo = CorruptedDataInfo.LoadFromXml(dataPath, CcdScan2CcdOdometer);
            ccdScanDeltaManager = new CcdScanDeltaManager(corruptedDataInfo);

            FillMinMaxParameters();

            return true;
        }

        public void Close()
        {
            if (IsOpened)
                InnerClose();
        }

        /// <summary>
        /// Создание провайдера по работе с координатной информацией
        /// </summary>
        /// <param name="calcParameters"></param>
        /// <param name="heap"> </param>
        public CoordinateDataProviderCrop(UIntPtr heap, CalcParameters calcParameters)
        {
            this.heap = heap;
            CalcParameters = calcParameters;

            pipeCircleModulFactor = CalcParameters.PipeCircle * 2f;
            mmInDegree = CalcParameters.PipeCircle / 360;
        }

        private unsafe bool SetPointers(int needRecordNumber)
        {
            var result = false;

            if (recordCount - 1 < needRecordNumber)
            {
                needRecordNumber = recordCount - 1;
            }
            else
            {
                result = true;
            }

            fileMapper.MapBuffer(needRecordNumber*recordSize);
            coordinateItems = (CoordinateItem*) fileMapper.Buffer;
            firstRecordNumberInBuffer = needRecordNumber;
            var lastRecordNumberInBuffer = (int) Math.Floor((double) (fileMapper.BufferRange.End - recordSize)/recordSize);
            recordsInBufferRange = new Range<int>(firstRecordNumberInBuffer, lastRecordNumberInBuffer);

            return result;
        }

        private void FillPeriods()
        {
            //scanPeriod = (int) ((lastRecordInFile.Scan - firstRecordInFile.Scan)/recordCount);
            var secondRecord = ReadRecord(1);

            scanPeriod = (int) (secondRecord.Scan - firstRecordInFile.Scan);


            scanPeriod = scanPeriod <= 0 ? 1 : scanPeriod;
            odometerPeriod = (int) ((lastRecordInFile.Odometer - firstRecordInFile.Odometer)/recordCount);
            odometerPeriod = odometerPeriod <= 0 ? 1 : odometerPeriod;   
            timePeriod = (int) ((lastRecordInFile.Time - firstRecordInFile.Time)/recordCount);
            timePeriod = timePeriod <= 0 ? 1 : timePeriod;

            averageTimePerOdometer = (uint) ((lastRecordInFile.Time - firstRecordInFile.Time)/
                                         (float) (lastRecordInFile.Odometer - firstRecordInFile.Odometer));
        }

        private void FillMinMaxParameters()
        {
            var deltaInfo = corruptedDataInfo.GetCcdScanDeltas((int) firstRecordInFile.Scan, 1);
            minDistance = Odometer2Dist(firstRecordInFile.Odometer + deltaInfo[0].SumOdometerDelta);
            MinScan = (int) (firstRecordInFile.Scan + deltaInfo[0].SumScanDelta);
            deltaInfo = corruptedDataInfo.GetCcdScanDeltas((int) lastRecordInFile.Scan, 1);
            maxDistance = Odometer2Dist(lastRecordInFile.Odometer+ deltaInfo[0].SumOdometerDelta);
            MaxScan = (int) (lastRecordInFile.Scan+deltaInfo[0].SumScanDelta);
        }

        private static double OdometerInScan(CoordinateItem item1, CoordinateItem item2)
        {
            var diffScan = (int)(item2.Scan - item1.Scan);

            if (diffScan == 0)
                return 0;

            return ((double)((int)item2.Odometer - (int)item1.Odometer)) / diffScan;  
        }

        private static double TimeInScan(CoordinateItem item1, CoordinateItem item2)
        {
            var diffScan = (int)(item2.Scan - item1.Scan);

            if (diffScan == 0)
                return 0;

            return Convert.ToDouble((item2.Time - item1.Time) / (double)diffScan);  
        }

        private bool InRange(Range<int> range, int offset)
        {
            return !(offset < range.Begin || offset > range.End);
        }

        internal unsafe CoordinateItem ReadRecord(int numberRecord)
        {
            if (numberRecord > recordCount - 1)
                numberRecord = recordCount - 1;

            if (InRange(recordsInBufferRange, numberRecord))
                return coordinateItems[numberRecord - firstRecordNumberInBuffer];
            
            if (SetPointers(numberRecord))
                return coordinateItems[numberRecord - firstRecordNumberInBuffer];

            return new CoordinateItem();
        }

        private unsafe void AddingLoop(CoordinateItemFloat* ccdItem, CoordinateItemFloat* virtualItem, int insertPosition, int maxInsertItemCount, double summaryDeltaOdometer, int curCcdScan)
        {
            for (var position = insertPosition; position < maxInsertItemCount; position++)
            {
                virtualItem->Odometer = ccdItem->Odometer + summaryDeltaOdometer + position;
                virtualItem->Time = (uint) (ccdItem->Time + (summaryDeltaOdometer + position)*averageTimePerOdometer);
                virtualItem->Angle = ccdItem->Angle;
                virtualItem->RefCCDScan = position == insertPosition ? curCcdScan : -1;
                virtualItem++;
            }
        }

        private unsafe void SimpleCopy(CoordinateItemFloat* ccdItem, CoordinateItemFloat* virtualItem, double summaryDeltaOdometer, int curCcdScan)
        {
            virtualItem->Odometer = ccdItem->Odometer + summaryDeltaOdometer;
            virtualItem->Time = (uint)(ccdItem->Time + summaryDeltaOdometer * averageTimePerOdometer);
            virtualItem->Angle = ccdItem->Angle;
            virtualItem->RefCCDScan = curCcdScan;
        }

        private int CcdScan2VirtualScan(int ccdScan)
        {
            var scanDeltas = corruptedDataInfo.GetCcdScanDeltas(ccdScan, 1);
            return ccdScan + scanDeltas[0].SumScanDelta;
        }

        private unsafe void BuildCoordinateData(CoordinateData coordinateData, int startCcdScan)
        {
            var numberRecord = CcdScan2NumberRecord(startCcdScan);
            coordinateData.Reset();
            var needRecordCount = (coordinateData.Count / scanPeriod);

            coordinateData.ReAllocate(needRecordCount * scanPeriod);

            if ((numberRecord + needRecordCount) >= recordCount)
                numberRecord = recordCount - needRecordCount - 1;
            if (numberRecord < 0) numberRecord = 0;

            var record = ReadRecord(numberRecord);

            var firstScan = (int) record.Scan;

            uint step = 0;
            var lastSuccessScan = firstScan;
            var lastSuccessOdometer = record.Odometer;
            var lastSuccessTime = record.Time;
            var lastSuccessAngle = record.Angle;

// ReSharper disable TooWideLocalVariableScope
            int curScan;
// ReSharper restore TooWideLocalVariableScope

            coordinateData.DataPointer[0].Odometer = lastSuccessOdometer;
            coordinateData.DataPointer[0].Angle = record.Angle;
            coordinateData.DataPointer[0].Time = record.Time;
            coordinateData.DataPointer[0].RefCCDScan = lastSuccessScan;

// ReSharper disable TooWideLocalVariableScope
            int countWritingScans;
            double countOdometersInInterval;
            double countTimeInInterval;
            double stepOdometer;
            double stepTime;
// ReSharper restore TooWideLocalVariableScope
            
            do
            {
                step ++;
                if (step == needRecordCount)
                {
                    // Анализ последнего массива
                    #region Анализ последнего массива

                    record = ReadRecord((int) (numberRecord + step));
                    if (lastSuccessScan == record.Scan)
                        break;

                    countWritingScans = (int)record.Scan - lastSuccessScan;
                    if (record.Odometer < lastSuccessOdometer)
                    {
                        stepOdometer = Convert.ToDouble(1 / ((int)record.Scan - lastSuccessScan));

                        var curRecordNumber = numberRecord + step;
                        for (var i = 0; i < 30000; i++)
                        {
                            var tmpRecord = ReadRecord((int) (curRecordNumber + i));
                            if (tmpRecord.Odometer <= lastSuccessOdometer) continue;

                            countOdometersInInterval = tmpRecord.Odometer - lastSuccessOdometer;
                            stepOdometer = Convert.ToDouble(countOdometersInInterval/((int)tmpRecord.Scan - lastSuccessScan));
                            break;
                        }
                    }
                    else
                    {
                        countOdometersInInterval = record.Odometer - lastSuccessOdometer;
                        stepOdometer = Convert.ToDouble(countOdometersInInterval/countWritingScans);
                    }

                    countTimeInInterval = record.Time - lastSuccessTime;
                    stepTime = Convert.ToDouble(countTimeInInterval / countWritingScans);

                    if ((lastSuccessScan - firstScan + countWritingScans) >= coordinateData.Count)
                        countWritingScans = coordinateData.Count - lastSuccessScan + firstScan - 1;

                    for (var i = 1; i <= countWritingScans; i++)
                    {
                        curScan = lastSuccessScan - firstScan + i;

                        if (curScan < 0 || curScan >= coordinateData.Count)
                            throw new ArgumentOutOfRangeException(curScan + Resources.CDP_ERROR_NOT_IN_ARRAY + firstScan);

                        coordinateData.DataPointer[curScan].Odometer
                            = lastSuccessOdometer + (i*stepOdometer);

                        coordinateData.DataPointer[curScan].Time =
                            lastSuccessTime + (uint) (Math.Round(i*stepTime,MidpointRounding.AwayFromZero));

                        coordinateData.DataPointer[curScan].Angle =
                            lastSuccessAngle;

                        coordinateData.DataPointer[curScan].RefCCDScan = lastSuccessScan + i;
                    }

                    lastSuccessScan += countWritingScans;
                    break;

                    #endregion
                }

                record = ReadRecord((int) (numberRecord + step));

                //if (record.Scan != step + 1)
                //    break;


                if (lastSuccessScan >= record.Scan)
                    break;

                if (lastSuccessOdometer >= record.Odometer)
                    continue;

                countWritingScans = (int)record.Scan - lastSuccessScan;
                countOdometersInInterval = record.Odometer - lastSuccessOdometer;
                countTimeInInterval = record.Time - lastSuccessTime;

                stepOdometer = Convert.ToDouble(countOdometersInInterval/countWritingScans);
                stepTime = Convert.ToDouble(countTimeInInterval/countWritingScans);

                for (var i = 1; i <= countWritingScans; i++)
                {
                    curScan = lastSuccessScan - firstScan + i;

                    if (curScan < 0 || curScan > coordinateData.Count)
                        throw new ArgumentOutOfRangeException(curScan + Resources.CDP_ERROR_NOT_IN_ARRAY + firstScan);

                    coordinateData.DataPointer[curScan].Odometer =
                        lastSuccessOdometer + (i*stepOdometer);

                    coordinateData.DataPointer[curScan].Time =
                        lastSuccessTime + (uint) (i*stepTime);

                    coordinateData.DataPointer[curScan].Angle =
                        lastSuccessAngle;

                    coordinateData.DataPointer[curScan].RefCCDScan = lastSuccessScan + i;
                }

                lastSuccessScan = (int) record.Scan;
                lastSuccessOdometer = record.Odometer;
                lastSuccessTime = record.Time;
                lastSuccessAngle = record.Angle;

            } while (true);

            coordinateData.ScanRange = new Range<int>(firstScan, lastSuccessScan);

            coordinateData.BuildDistIndex();
        }

        private unsafe void BuildVirtualCoordinateData(int virtualScan)
        {
            if (!corruptedDataInfo.Available)
            {
                BuildCoordinateData(virtualCoordinateData, virtualScan);
                return;
            }
            virtualCoordinateData.Reset();

            var referenceCcdScan = corruptedDataInfo.GetCcdScan(virtualScan);
            var curCcdScan = referenceCcdScan;
            var referenceVirtualScan = corruptedDataInfo.GetVirtualScan(referenceCcdScan);
            var curVirtualScanIndex = 0;
            
            var deltaInfo = ccdScanDeltaManager.GetDelta(curCcdScan);

            if (deltaInfo.CorruptedInfo > 0)
                //Есть вставка доп.сканов
            {
                var referenceDelta = virtualScan - referenceVirtualScan;
                if (referenceDelta > 0)
                    //Целевой виртуальный скан находится в середине диапазона вставки
                {
                    if (!uninterruptedCoordinateData.ScanRange.IsIncludeWithLeftOnly(curCcdScan))
                        BuildCoordinateData(uninterruptedCoordinateData, curCcdScan);
                    var insertItemCount = deltaInfo.CorruptedInfo + 1;

                    AddingLoop(
                        (CoordinateItemFloat*)
                            uninterruptedCoordinateData.GetDataPointer(curCcdScan -
                                                                       uninterruptedCoordinateData.ScanRange.Begin),
                        (CoordinateItemFloat*) virtualCoordinateData.GetDataPointer(curVirtualScanIndex), referenceDelta,
                        insertItemCount, deltaInfo.SumOdometerDelta, curCcdScan);

                    curVirtualScanIndex += insertItemCount - referenceDelta;
                    curCcdScan++;
                }
            }

            do
            {
                if (curCcdScan > lastRecordInFile.Scan)
                    break;

                if (!uninterruptedCoordinateData.ScanRange.IsInclude(true, curCcdScan))
                    BuildCoordinateData(uninterruptedCoordinateData, curCcdScan);

                deltaInfo = ccdScanDeltaManager.GetDelta(curCcdScan);

                if (deltaInfo.CorruptedInfo == 0)
                {
                    SimpleCopy(
                        (CoordinateItemFloat*)
                            uninterruptedCoordinateData.GetDataPointer(curCcdScan -
                                                                       uninterruptedCoordinateData.ScanRange.Begin),
                        (CoordinateItemFloat*) virtualCoordinateData.GetDataPointer(curVirtualScanIndex),
                        deltaInfo.SumOdometerDelta, curCcdScan);

                    curVirtualScanIndex++;
                    curCcdScan++;
                }
                else
                {
                    if (deltaInfo.CorruptedInfo > 0)
                    {
                        var insertItemCount = deltaInfo.CorruptedInfo + 1;

                        if (curVirtualScanIndex + insertItemCount > virtualCoordinateData.Count - 1)
                            insertItemCount = virtualCoordinateData.Count - 1 - curVirtualScanIndex;

                        AddingLoop(
                            (CoordinateItemFloat*)
                                uninterruptedCoordinateData.GetDataPointer(curCcdScan -
                                                                           uninterruptedCoordinateData.ScanRange.Begin),
                            (CoordinateItemFloat*) virtualCoordinateData.GetDataPointer(curVirtualScanIndex), 0,
                            insertItemCount, deltaInfo.SumOdometerDelta, curCcdScan);

                        curVirtualScanIndex += insertItemCount;
                        curCcdScan++;
                    }
                    else
                        //deltaInfo.CorruptedInfo < 0
                    {
                        SimpleCopy(
                            (CoordinateItemFloat*)
                                uninterruptedCoordinateData.GetDataPointer(curCcdScan -
                                                                           uninterruptedCoordinateData.ScanRange.Begin),
                            (CoordinateItemFloat*) virtualCoordinateData.GetDataPointer(curVirtualScanIndex),
                            deltaInfo.SumOdometerDelta, curCcdScan);

                        curVirtualScanIndex++;
                        curCcdScan += -deltaInfo.CorruptedInfo + 1;
                    }
                }

            } while (curVirtualScanIndex < virtualCoordinateData.Count);

            virtualCoordinateData.ScanRange = new Range<int>(virtualScan, virtualScan + curVirtualScanIndex - 1);
            virtualCoordinateData.ReAllocate(virtualCoordinateData.ScanRange.Length() + 1);

            virtualCoordinateData.BuildDistIndex();

            //CCDLog();
        }

        #region Odometer2Scan

        private int VirtualOdometer2VirtualScanSlow(double needVirtualOdometerF)
        {
            var needCcdOdometerF = corruptedDataInfo.GetCcdOdometer(needVirtualOdometerF);
            return CcdScan2VirtualScan(CcdOdometer2CcdScanSlow(needCcdOdometerF));
        }

        private int CcdOdometer2CcdScanSlow(double needCcdOdometerF)
        {
            var needCcdOdometerI = Convert.ToInt32(needCcdOdometerF);

            var numberRecord = UseIDT ? CcdOdometer2NumberRecordIdt(needCcdOdometerI) : CcdOdometer2NumberRecord(needCcdOdometerI);

            if (numberRecord <= 0)
                return (int) firstRecordInFile.Scan;

            if (numberRecord >= recordCount)
                return (int) lastRecordInFile.Scan;

            // Расчитываем разницу между запрошенным импульсом одометра и импульсом одометра по расчитанному номеру записи
            var diffOdometer = needCcdOdometerF - ReadRecord(numberRecord).Odometer;

            double odometerInScan;

            if (diffOdometer < 0)
            {
                // Если оказались справа от запрошенного импульса
                // то необходимо расчитать сканирования от предыдущей записи
                // и при пересчете номера скана отнимать от расчитанного
                if (numberRecord <= 0)
                {
                    // Если мы на первой записи
                    // то расчитаем интервал от 1
                    odometerInScan = OdometerInScan(ReadRecord(1), ReadRecord(0));
                }
                else
                {
                    // Если не на первой то расчитываем интервал от предыдущей
                    odometerInScan = OdometerInScan(ReadRecord(numberRecord - 1), ReadRecord(numberRecord));
                }
            }
            else
            {
                // Если оказались слева от запрошенного импульса
                // то необходимо расчитать интервал сканирования от предыдущей записи
                // и при пересчете номера скана прибавлять к расчитанному
                if (numberRecord >= (recordCount - 1))
                {
                    // Если мы на последней записи
                    // то расчитаем интервал от предыдущей
                    odometerInScan = OdometerInScan(ReadRecord(recordCount - 1), ReadRecord(recordCount - 2));
                }
                else
                {
                    // Если не на последней то расчитываем интервал от следующей
                    odometerInScan = OdometerInScan(ReadRecord(numberRecord), ReadRecord(numberRecord + 1));
                }
            }


            int diffScan;
            if (odometerInScan > 2 || odometerInScan < 0.001)
            {
                diffScan = odometerInScan > 2*odometerPeriod ? 0 : Convert.ToInt32(diffOdometer);
            }
            else
            {
                diffScan = Convert.ToInt32(diffOdometer/odometerInScan);
            }

            // По пропорции расчитаем разницу в сканах
            //int diffScan = (int) Math.Floor((cfr_RecalcDistanceToMetr(diffImpulses)/rate)+0.5);

            var numberScan = (int) (ReadRecord(numberRecord).Scan + diffScan);

            if (numberScan > lastRecordInFile.Scan)
                numberScan = (int) lastRecordInFile.Scan;

            //if(((nlrb + 1) < (long)c_CountRecords) && (numberScan > (long)(l_record1.c_numberScan)))
            //    numberScan = (int)l_record1.c_numberScan;

            return numberScan < 0 ? CcdScan2VirtualScan(0) : CcdScan2VirtualScan(numberScan);
        } 
        
        private double Time2DistSlow(int needTime)
        {
            var numberRecord = UseIDT ? Time2NumberRecordIDT(needTime) : Time2NumberRecord(needTime);

            if (numberRecord <= 0)
                return Odometer2Dist((int) firstRecordInFile.Odometer);

            if (numberRecord >= recordCount)
                return Odometer2Dist((int) lastRecordInFile.Odometer);

            // Расчитываем разницу между запрошенным импульсом одометра и импульсом одометра по расчитанному номеру записи
            var diffTime = needTime - ReadRecord(numberRecord).Time;

            double timeInScan;

            if (diffTime < 0)
            {
                // Если оказались справа от запрошенного импульса
                // то необходимо расчитать сканирования от предыдущей записи
                // и при пересчете номера скана отнимать от расчитанного
                if (numberRecord <= 0)
                {
                    // Если мы на первой записи
                    // то расчитаем интервал от 1
                    timeInScan = TimeInScan(ReadRecord(1), ReadRecord(0));
                }
                else
                {
                    // Если не на первой то расчитываем интервал от предыдущей
                    timeInScan = TimeInScan(ReadRecord(numberRecord - 1), ReadRecord(numberRecord));
                }
            }
            else
            {
                // Если оказались слева от запрошенного импульса
                // то необходимо расчитать интервал сканирования от предыдущей записи
                // и при пересчете номера скана прибавлять к расчитанному
                if (numberRecord >= (recordCount - 1))
                {
                    // Если мы на последней записи
                    // то расчитаем интервал от предыдущей
                    timeInScan = TimeInScan(ReadRecord(recordCount - 1), ReadRecord(recordCount - 2));
                }
                else
                {
                    // Если не на последней то расчитываем интервал от следующей
                    timeInScan = TimeInScan(ReadRecord(numberRecord), ReadRecord(numberRecord + 1));
                }
            }

            if (MathHelper.TestFloatEquals(timeInScan, 0d))
                timeInScan = timePeriod;

            // По пропорции расчитаем разницу в сканах
            //int diffScan = (int) Math.Floor((cfr_RecalcDistanceToMetr(diffImpulses)/rate)+0.5);
            var diffScan = Convert.ToInt32(diffTime / timeInScan);

            var numberScan = (int)(ReadRecord(numberRecord).Scan + diffScan);

            if (numberScan > lastRecordInFile.Scan)
                numberScan = (int)lastRecordInFile.Scan;

            //if(((nlrb + 1) < (long)c_CountRecords) && (numberScan > (long)(l_record1.c_numberScan)))
            //    numberScan = (int)l_record1.c_numberScan;

            if (numberScan < 0) numberScan = 0;

            return Scan2Dist(numberScan);
        }

        private int CcdOdometer2NumberRecordIdt(int odometer)
        {
            uint step = 0;

            if (odometer < 0)
                return 0;

            var odometerUint = (uint)odometer;
            var index = odometerUint >> iodRate;

            var recordNumber = (int)fileIDT.ReadOdometer(index);
            var nextRecordNumber = (int)fileIDT.NextDifferingRecordNumber(index);

            if (recordNumber == nextRecordNumber)
                return recordNumber;

            var record = ReadRecord(recordNumber);

            var dif = odometer - (int)record.Odometer;

            if (dif < 0)
            {
                if (index < 1)
                    index = 1;
                recordNumber = (int)fileIDT.ReadOdometer(index - 1);
                nextRecordNumber = (int)fileIDT.NextDifferingRecordNumber(index - 1);
            }

            if (recordNumber == nextRecordNumber)
                return recordNumber;

            record = ReadRecord(recordNumber);
            var nextRecord = ReadRecord(nextRecordNumber);

            dif = odometer - (int)record.Odometer;

            if (dif < 0)
                dif = 0;

            var delta = (nextRecord.Odometer - record.Odometer) /
                        (double)(nextRecord.Scan - record.Scan);

            do
            {
                if (step < maxCountStepDiv2)
                {
                    step++;

                    if (dif == 0)
                        return recordNumber;

                    int diffRecord;
                    if (dif < 0)
                    {
                        diffRecord = (int)(dif / delta) / scanPeriod;

                        if (diffRecord >= -1)
                        {
                            recordNumber--;

                            if (recordNumber < 0)
                                recordNumber = 0;

                            return recordNumber;
                        }

                        recordNumber += diffRecord;
                        if (recordNumber < 0)
                            recordNumber = 0;
                    }
                    else
                    {
                        diffRecord = (int)(dif / delta) / scanPeriod;

                        if (diffRecord <= 1)
                        {
                            recordNumber++;
                            if (recordNumber >= recordCount)
                                recordNumber = recordCount - 1;
                            return recordNumber;
                        }

                        recordNumber += diffRecord;

                        if (recordNumber >= recordCount)
                            recordNumber = recordCount - 1;
                    }

                    record = ReadRecord(recordNumber);

                    dif = odometer - (int)record.Odometer;
                }
                else
                {
                    recordNumber = (int)fileIDT.ReadOdometer(index);

                    var factor = Int32.MaxValue;

                    for (var i = recordNumber; i <= nextRecordNumber; i++)
                    {
                        record = ReadRecord(i);

                        var curDifference = Math.Abs((int) record.Odometer - odometer);

                        if (curDifference > factor)
                            return i - 1;

                        factor = curDifference;
                    }

                    return nextRecordNumber;
                }
            } while (true);
        }
        
        private int CcdOdometer2NumberRecord(int needOdometer)
                        {
            var startNumberRecord = Convert.ToInt32((needOdometer - firstRecordInFile.Odometer) / odometerPeriod);

            var numberRecord = startNumberRecord;
            var lastNumberRecord = -1;
            var lastOdometer = 0;
            var lessValue = false;

            if (numberRecord >= recordCount)
                numberRecord = recordCount - 1;

            if (numberRecord < 0)
                numberRecord = 0;

            uint step = 0;

            // ReSharper disable TooWideLocalVariableScope
            int diffOdometer; 
            int diffRecord; 

            double realPeriodCoordinateInfo;
            // ReSharper restore TooWideLocalVariableScope

            do
            {
                #region if (step < maxCountStepDiv2)

                if (step < maxCountStepDiv2)
                {
                    step++;
                    var odometer = (int)ReadRecord(numberRecord).Odometer;

                    if (odometer == needOdometer) // Прекрасно (c) Котов А.В.
                        break;

                    if (odometer > needOdometer) //Перешли за необходимую дистанцию
                    {
                        diffOdometer = odometer - needOdometer;
                        diffRecord = diffOdometer / odometerPeriod;

                        if (diffRecord <= 1)
                        {
                            if (odometer > needOdometer)
                                numberRecord--;

                            if (numberRecord <= 0)
                                numberRecord = 0;
                            break;
                        }

                        numberRecord -= diffRecord;

                        if (numberRecord < 0)
                            numberRecord = 0;

                        if (numberRecord == startNumberRecord)
                            break;
                    }
                    else //Не дошли до необходимой дистанции
                    {
                        diffOdometer = needOdometer - odometer;
                        diffRecord = diffOdometer / odometerPeriod;

                        if (diffRecord <= 1)
                        {
                            if (odometer < needOdometer)
                                numberRecord++;

                            if (numberRecord >= recordCount)
                                numberRecord = recordCount - 1;
                            break;
                        }

                        numberRecord += diffRecord;

                        if (numberRecord >= recordCount )
                        {
                            numberRecord = recordCount - 1;
                        }

                        if (numberRecord == startNumberRecord)
                            break;
                    }
                } 
                    #endregion
                
                    #region else
                else
                {
                    step++;
                   
                    if (step > maxCountStep)
                    {
                        //Последовательный перебор
                        var record = ReadRecord(numberRecord);
                        
                        if (record.Odometer > needOdometer)
                        {
                            for (var rec = numberRecord - 1; rec >= 0; rec--)
                            {
                                record = ReadRecord(rec);
                                if (record.Odometer <= needOdometer)
                                    return rec;
                            }
                            return 0;
                        }
                        
                        for (var rec = numberRecord + 1; rec < recordCount; rec++)
                        {
                            record = ReadRecord(rec);
                            if (record.Odometer >= needOdometer)
                                return rec;
                        }
                        return recordCount - 1;
                    }

                    var odometer = (int)ReadRecord(numberRecord).Odometer;

                    if (odometer == needOdometer) // Прекрасно (c) Котов А.В.
                        break;

                    #region if (odometer > needOdometer)

                    if (odometer > needOdometer)
                    {
                        diffOdometer = odometer - needOdometer;

                        if (lessValue)
                        {
                            if (lastOdometer < odometer)
                                 return startNumberRecord;

                            realPeriodCoordinateInfo = lastNumberRecord == -1
                                                           ? odometerPeriod
                                                           : (double)(lastOdometer - odometer) /
                                                             (lastNumberRecord - numberRecord);
                        }
                        else
                        {
                            if (lastOdometer > odometer)
                                return startNumberRecord;

                            realPeriodCoordinateInfo = lastNumberRecord == -1
                                                           ? odometerPeriod
                                                           : (double) (odometer - lastOdometer)/
                                                             (numberRecord - lastNumberRecord);
                        }

                        if (MathHelper.TestFloatEquals(realPeriodCoordinateInfo, 0))
                            break;

                        diffRecord = Convert.ToInt32(diffOdometer / realPeriodCoordinateInfo);

                        if (diffRecord <= 1)
                        {
                            numberRecord--;

                            if (numberRecord <= 0)
                                numberRecord = 0;
                            break;
                        }

                        lastNumberRecord = numberRecord;
                        lastOdometer = odometer;
                        numberRecord -= diffRecord;
                        lessValue = true;

                        if (numberRecord < 0)
                        {
                            numberRecord = 0;
                            if (lastNumberRecord == 0)
                                break;
                        }

                        if (numberRecord == startNumberRecord)
                            break;

                        if (lastNumberRecord == numberRecord)
                            break;
                    }
                    #endregion
                    
                    #region else
                    {
                        diffOdometer = needOdometer - odometer;

                        if (lessValue)
                        {
                            if (lastOdometer < odometer)
                                return startNumberRecord;
                            realPeriodCoordinateInfo = lastNumberRecord == -1
                                                           ? odometerPeriod
                                                           : (double)(lastOdometer - odometer) /
                                                             (lastNumberRecord - numberRecord);
                        }
                        else
                        {
                            if (lastOdometer > odometer)
                                return startNumberRecord;

                            realPeriodCoordinateInfo = lastNumberRecord == -1
                                                           ? odometerPeriod
                                                           : (double) (odometer - lastOdometer)/
                                                             (numberRecord - lastNumberRecord);
                        }

                        if (MathHelper.TestFloatEquals(realPeriodCoordinateInfo, 0))
                            break;

                        diffRecord = Convert.ToInt32(diffOdometer / realPeriodCoordinateInfo);

                        if (diffRecord <= 1)
                        {
                            numberRecord++;

                            if (numberRecord <= 0)
                                numberRecord = 0;
                            break;
                        }

                        lastNumberRecord = numberRecord;
                        lastOdometer = odometer;
                        numberRecord += diffRecord;
                        lessValue = false;

                        if (numberRecord >= recordCount)
                        {
                            numberRecord = recordCount - 1;
                        }

                        if (numberRecord == startNumberRecord)
                            break;

                        if (lastNumberRecord == numberRecord)
                            break;
                    }

                    #endregion

                }

                #endregion

            } while (true);

            return numberRecord;
        }
        
        #endregion

        #region Time2Dist

        private int CcdTime2CcdScanSlow(int ccdTime)
        {
            var numberRecord = UseIDT ? Time2NumberRecordIDT(ccdTime) : Time2NumberRecord(ccdTime);

            if (numberRecord <= 0)
                return (int) firstRecordInFile.Scan;

            if (numberRecord >= recordCount)
                return (int) lastRecordInFile.Scan;

            // Расчитываем разницу между запрошенным импульсом одометра и импульсом одометра по расчитанному номеру записи
            var diffTime = ccdTime - ReadRecord(numberRecord).Time;

            double timeInScan;

            if (diffTime < 0)
            {
                // Если оказались справа от запрошенного импульса
                // то необходимо расчитать сканирования от предыдущей записи
                // и при пересчете номера скана отнимать от расчитанного
                if (numberRecord <= 0)
                {
                    // Если мы на первой записи
                    // то расчитаем интервал от 1
                    timeInScan = TimeInScan(ReadRecord(1), ReadRecord(0));
                }
                else
                {
                    // Если не на первой то расчитываем интервал от предыдущей
                    timeInScan = TimeInScan(ReadRecord(numberRecord - 1), ReadRecord(numberRecord));
                }
            }
            else
            {
                // Если оказались слева от запрошенного импульса
                // то необходимо расчитать интервал сканирования от предыдущей записи
                // и при пересчете номера скана прибавлять к расчитанному
                if (numberRecord >= (recordCount - 1))
                {
                    // Если мы на последней записи
                    // то расчитаем интервал от предыдущей
                    timeInScan = TimeInScan(ReadRecord(recordCount - 1), ReadRecord(recordCount - 2));
                }
                else
                {
                    // Если не на последней то расчитываем интервал от следующей
                    timeInScan = TimeInScan(ReadRecord(numberRecord), ReadRecord(numberRecord + 1));
                }
            }

            timeInScan = timeInScan > 0 ? timeInScan : 1;

            // По пропорции расчитаем разницу в сканах
            //int diffScan = (int) Math.Floor((cfr_RecalcDistanceToMetr(diffImpulses)/rate)+0.5);
            var diffScan = Convert.ToInt32(diffTime / timeInScan);

            var numberScan = (int)(ReadRecord(numberRecord).Scan + diffScan);

            if (numberScan > lastRecordInFile.Scan)
                numberScan = (int)lastRecordInFile.Scan;

            //if(((nlrb + 1) < (long)c_CountRecords) && (numberScan > (long)(l_record1.c_numberScan)))
            //    numberScan = (int)l_record1.c_numberScan;

            if (numberScan < 0) numberScan = 0;

            return numberScan;
        }

        private int Time2NumberRecordIDT(int time)
        {
            uint step = 0;

            var index = ((uint)time) >> iotRate;
            uint nextRecordNumber;
            var recordNumber = (int)fileIDT.ReadTime(index, out nextRecordNumber);

            if (recordNumber == nextRecordNumber)
                return recordNumber;

            var record = ReadRecord(recordNumber);

            var dif = time - (int)record.Time;

            if (dif < 0)
                recordNumber = (int)fileIDT.ReadTime(index - 1, out nextRecordNumber);

            if (recordNumber == nextRecordNumber)
                return recordNumber;

            //   record = ReadRecord(recordNumber);
            var nextRecord = ReadRecord((int)(nextRecordNumber));

            dif = time - (int)record.Time;

            if (dif < 0)
                dif = 0;

            var delta = (nextRecord.Time - record.Time) /
                        (double)(nextRecord.Scan - record.Scan);

            do
            {
                if (step < maxCountStepDiv2)
                {
                    step++;

                    if (dif == 0)
                        return recordNumber;

                    int diffRecord;
                    if (dif < 0)
                    {
                        diffRecord = (int)(dif / delta) / scanPeriod;

                        if (diffRecord <= 1)
                        {
                            recordNumber--;

                            if (recordNumber < 0)
                                recordNumber = 0;

                            return recordNumber;
                        }

                        recordNumber -= diffRecord;
                        if (recordNumber < 0)
                            recordNumber = 0;
                    }
                    else
                    {
                        diffRecord = (int)(dif / delta)/scanPeriod;

                        if (diffRecord <= 1)
                        {
                            recordNumber++;
                            if (recordNumber >= recordCount)
                                recordNumber = recordCount - 1;
                            return recordNumber;
                        }
        
                        recordNumber += diffRecord;

                        if (recordNumber >= recordCount)
                            recordNumber = recordCount - 1;
                    }

                    record = ReadRecord(recordNumber);

                    dif = time - (int)record.Time;
                }
                else
                {
                    recordNumber = (int)fileIDT.ReadTime(index);

                    for (var i = recordNumber; i < nextRecordNumber; i++)
                    {
                        record = ReadRecord(recordNumber);

                        if (record.Time >= time)
                            return i;
                    }
                }
            } while (true);
        }

        private int Time2NumberRecord(int needTime)
        {
            var startNumberRecord = Convert.ToInt32((needTime - firstRecordInFile.Time) / timePeriod);

            var numberRecord = startNumberRecord;
            var lastNumberRecord = -1;
            var lastTime = 0;
            var lessValue = false;

            if (numberRecord >= recordCount)
                numberRecord = recordCount - 1;

            if (numberRecord < 0)
                numberRecord = 0;

            uint step = 0;

            // ReSharper disable TooWideLocalVariableScope
            int diffTime; 
            int diffRecord; 

            double realPeriodCoordinateInfo;
            // ReSharper restore TooWideLocalVariableScope

            do
            {
                #region if (step < maxCountStepDiv2)

                if (step < maxCountStepDiv2)
                {
                    step++;
                    var time = (int)ReadRecord(numberRecord).Time;

                    if (time == needTime) // Прекрасно (c) Котов А.В.
                        break;

                    if (time > needTime) //Перешли за необходимую дистанцию
                    {
                        diffTime = time - needTime;
                        diffRecord = diffTime / timePeriod;

                        if (diffRecord <= 1)
                        {
                            if (time > needTime)
                                numberRecord--;

                            if (numberRecord <= 0)
                                numberRecord = 0;
                            break;
                        }

                        numberRecord -= diffRecord;

                        if (numberRecord < 0)
                            numberRecord = 0;

                        if (numberRecord == startNumberRecord)
                            break;
                    }
                    else //Не дошли до необходимой дистанции
                    {
                        diffTime = needTime - time;
                        diffRecord = diffTime / timePeriod;

                        if (diffRecord <= 1)
                        {
                            if (time < needTime)
                                numberRecord++;

                            if (numberRecord >= recordCount)
                                numberRecord = recordCount - 1;
                            break;
                        }

                        numberRecord += diffRecord;

                        if (numberRecord >= recordCount )
                        {
                            numberRecord = recordCount - 1;
                        }

                        if (numberRecord == startNumberRecord)
                            break;
                    }
                } 
                    #endregion
                
                    #region else
                else
                {
                    step++;
                   
                    if (step > maxCountStep)
                    {
                        //Последовательный перебор
                        var record = ReadRecord(numberRecord);

                        if (record.Time > needTime)
                        {
                            for (var rec = numberRecord - 1; rec >= 0; rec--)
                            {
                                record = ReadRecord(rec);
                                if (record.Time <= needTime)
                                    return rec;
                            }
                            return 0;
                        }
                        
                        for (var rec = numberRecord + 1; rec < recordCount; rec++)
                        {
                            record = ReadRecord(rec);
                            if (record.Time >= needTime)
                                return rec;
                        }
                        return recordCount - 1;
                    }

                    var time = (int)ReadRecord(numberRecord).Time;

                    if (time == needTime) // Прекрасно (c) Котов А.В.
                        break;

                    #region if (time > needTime)

                    if (time > needTime)
                    {
                        diffTime = time - needTime;

                        if (lessValue)
                        {
                            if (lastTime < time)
                                return startNumberRecord;

                            realPeriodCoordinateInfo = lastNumberRecord == -1
                                                           ? timePeriod
                                                           : (double)(lastTime - time) /
                                                             (lastNumberRecord - numberRecord);
                        }
                        else
                        {
                            if (lastTime > time)
                                return startNumberRecord;

                            realPeriodCoordinateInfo = lastNumberRecord == -1
                                                           ? timePeriod
                                                           : (double) (time - lastTime)/
                                                             (numberRecord - lastNumberRecord);
                        }

                        if (MathHelper.TestFloatEquals(realPeriodCoordinateInfo, 0))
                            break;

                        diffRecord = Convert.ToInt32(diffTime / realPeriodCoordinateInfo);

                        if (diffRecord <= 1)
                        {
                            numberRecord--;

                            if (numberRecord <= 0)
                                numberRecord = 0;
                            break;
                        }

                        lastNumberRecord = numberRecord;
                        lastTime = time;
                        numberRecord -= diffRecord;
                        lessValue = true;

                        if (numberRecord < 0)
                        {
                            numberRecord = 0;
                            if (lastNumberRecord == 0)
                                break;
                        }

                        if (numberRecord == startNumberRecord)
                            break;

                        if (lastNumberRecord == numberRecord)
                            break;
                    }
                    #endregion
                    
                    #region else
                    {
                        diffTime = needTime - time;

                        if (lessValue)
                        {
                            if (lastTime < time)
                                return startNumberRecord;

                            realPeriodCoordinateInfo = lastNumberRecord == -1
                                                           ? timePeriod
                                                           : (double)(lastTime - time) /
                                                             (lastNumberRecord - numberRecord);
                        }
                        else
                        {
                            if (lastTime > time)
                                return startNumberRecord;

                            realPeriodCoordinateInfo = lastNumberRecord == -1
                                                           ? timePeriod
                                                           : (double) (time - lastTime)/
                                                             (numberRecord - lastNumberRecord);
                        }

                        if (MathHelper.TestFloatEquals(realPeriodCoordinateInfo, 0))
                            break;

                        diffRecord = Convert.ToInt32(diffTime / realPeriodCoordinateInfo);

                        if (diffRecord <= 1)
                        {
                            numberRecord++;

                            if (numberRecord <= 0)
                                numberRecord = 0;
                            break;
                        }

                        lastNumberRecord = numberRecord;
                        lastTime = time;
                        numberRecord += diffRecord;
                        lessValue = false;

                        if (numberRecord >= recordCount)
                        {
                            numberRecord = recordCount - 1;
                        }

                        if (numberRecord == startNumberRecord)
                            break;

                        if (lastNumberRecord == numberRecord)
                            break;
                    }

                    #endregion

                }

                #endregion

            } while (true);

            return numberRecord;
        }

        #endregion

        internal double Odometer2Dist(double odometer)
        {
            return odometer * CalcParameters.OdoFactor - CalcParameters.DistanceOffset;
        }

        private double VirtualScan2Dist(int virtualScan)
        {
            return Odometer2Dist(VirtualScan2VirtualOdometer(virtualScan));
        }

        internal int CcdScan2NumberRecord(int needScan)
        {
            var startNumberRecord = Convert.ToInt32((needScan - firstRecordInFile.Scan)/scanPeriod);

            if (startNumberRecord < 0)
                startNumberRecord = 0;

            if (startNumberRecord >= recordCount)
                startNumberRecord = recordCount - 1;

            if ((needScan >= ReadRecord(startNumberRecord).Scan) && (needScan < ReadRecord(startNumberRecord + 1).Scan))
            {
                return startNumberRecord;
            }
            
            return CcdScan2NumberRecordLongTime(needScan, startNumberRecord);
        }

        private int CcdScan2NumberRecordLongTime(int needScan, int startNumberRecord)
        {
            uint step = 0;
            // ReSharper disable TooWideLocalVariableScope
            int diffScan; 
            int diffRecord;
            // ReSharper restore TooWideLocalVariableScope

            // Если реальная частота записи одометрической информации будет отличаться от той, 
            // что прописана в файле omni, то могут возникнуть проблемы

            var numberRecord = startNumberRecord;

            do
            {
                step++;

                if (step > maxCountStepDiv2)
                {
                    //Последовательный перебор
                    var record = ReadRecord(numberRecord);

                    if (record.Scan > needScan)
                    {
                        for (var rec = numberRecord - 1; rec >= 0; rec--)
                        {
                            record = ReadRecord(rec);
                            if (record.Scan <= needScan)
                                return rec;
                        }

                        return 0;
                    }
                    
                    for (var rec = numberRecord + 1; rec < recordCount; rec++)
                    {
                        record = ReadRecord(rec);
                        if (record.Scan >= needScan)
                            return rec;
                    }
                    return recordCount - 1;
                }

                var scan = (int) ReadRecord(numberRecord).Scan;

                if (scan == needScan)
                    return numberRecord;

                #region if (scan > needScan)

                if (scan > needScan)
                {
                    diffScan = scan - needScan;
                    diffRecord = diffScan / scanPeriod;

                    if (diffRecord <= 1)
                    {
                        numberRecord--;

                        if (numberRecord <= 0)
                            numberRecord = 0;

                        break;
                    }

                    numberRecord -= diffRecord;

                    if (numberRecord < 0)
                    {
                        numberRecord = 0;
                        break;
                    }

                    if (numberRecord == startNumberRecord)
                        break;
                    
                }
                    #endregion

                    #region else

                else
                {
                    diffScan = needScan - scan;
                    diffRecord = Convert.ToInt32(diffScan / scanPeriod);

                    if (diffRecord <= 1)
                    {
                        numberRecord++;

                        if (numberRecord >= recordCount)
                            numberRecord = recordCount - 1;

                        break;
                    }

                    numberRecord += diffRecord;

                    if (numberRecord >= recordCount)
                    {
                        numberRecord = recordCount - 1;
                        break;
                    }

                    if (numberRecord == startNumberRecord)
                        break;
                }

                #endregion

            }
            while (true);

            return numberRecord;
        }

        private double CcdScan2CcdOdometer(int ccdScan)
        {
            double odometer;
            if (!uninterruptedCoordinateData.ScanRange.IsIncludeWithLeftOnly(ccdScan))
                BuildCoordinateData(uninterruptedCoordinateData, ccdScan);

            return uninterruptedCoordinateData.GetOdometer(ccdScan, out odometer) ? odometer : CcdScan2CcdOdometerSlow(ccdScan);
        }

        private double CcdScan2CcdOdometerSlow(int ccdScan)
        {
            var numberRecord = CcdScan2NumberRecord(ccdScan);

            double odometerInScan;

            var diffScans = (int)(ccdScan - ReadRecord(numberRecord).Scan);

            if (diffScans < 0)
            {
                // Если оказались справа от запрошенного скана
                // то необходимо расчитать интервал сканирования от предыдущей записи
                // и при пересчете импульса отнимать от расчитанного
                if (numberRecord == 0)
                {
                    // Если мы на первой записи
                    // то расчитаем интервал от следующей
                    odometerInScan = OdometerInScan(ReadRecord(1), ReadRecord(0));
                }
                else
                {
                    // Если не на первой то расчитываем интервал от предыдущей
                    odometerInScan = OdometerInScan(ReadRecord(numberRecord - 1), ReadRecord(numberRecord));
                }
            }
            else
            {
                // Если оказались слева от запрошенного скана
                // то необходимо расчитать интервал сканирования от предыдущей записи
                // и при пересчете импульса прибавлять к расчитанному
                if (numberRecord == (recordCount - 1))
                {
                    // Если мы на последней записи
                    // то расчитаем интервал от предыдущей
                    odometerInScan = OdometerInScan(ReadRecord(recordCount - 2), ReadRecord(recordCount - 1));
                }
                else
                {
                    // Если не на последней то расчитываем интервал от следующей
                    odometerInScan = OdometerInScan(ReadRecord(numberRecord), ReadRecord(numberRecord + 1));
                }
            }

            if (odometerInScan > 2 || odometerInScan < 0.1)
            {
                odometerInScan = 1;
            }

            // По пропорции расчитаем разницу в имульсах
            var diffOdometer = Math.Round(diffScans * odometerInScan, 3, MidpointRounding.AwayFromZero);
            //int diffScan = cfr_RecalcDistanceToMetr(diffImpulses) / rate));

            var odometer = ReadRecord(numberRecord).Odometer + diffOdometer;// * sign);

            //if(((nlrb + 1) < (long)c_CountRecords) && (numberScan > (long)(l_record1.c_numberScan)))
            //    numberScan = (int)l_record1.c_numberScan;

            return (odometer < 0) ? 0 : odometer;
        }

        private uint CcdScan2CcdTimeSlow(int scan)
        {
            var numberRecord = CcdScan2NumberRecord(scan);

            double timeInScan;

            int diffScans;
            var record = ReadRecord(numberRecord);
            diffScans = Convert.ToInt32(scan - record.Scan);

            if (diffScans < 0)
            {
                // Если оказались справа от запрошенного скана
                // то необходимо расчитать интервал сканирования от предыдущей записи
                // и при пересчете импульса отнимать от расчитанного
                if (numberRecord == 0)
                {
                    // Если мы на первой записи
                    // то расчитаем интервал от следующей
                    timeInScan = TimeInScan(ReadRecord(1), ReadRecord(0));
                }
                else
                {
                    // Если не на первой то расчитываем интервал от предыдущей
                    timeInScan = TimeInScan(ReadRecord(numberRecord - 1), record);
                }
            }
            else
            {
                // Если оказались слева от запрошенного скана
                // то необходимо расчитать интервал сканирования от предыдущей записи
                // и при пересчете импульса прибавлять к расчитанному
                if (numberRecord == (recordCount - 1))
                {
                    // Если мы на последней записи
                    // то расчитаем интервал от предыдущей
                    timeInScan = TimeInScan(ReadRecord(recordCount - 2), ReadRecord(recordCount - 1));
                }
                else
                {
                    // Если не на последней то расчитываем интервал от следующей
                    timeInScan = TimeInScan(record, ReadRecord(numberRecord + 1));
                }
            }

            long diffTime;
            // По пропорции расчитаем разницу в имульсах
            diffTime = Convert.ToInt32(Math.Round(diffScans * timeInScan, MidpointRounding.AwayFromZero));
            //int diffScan = cfr_RecalcDistanceToMetr(diffImpulses) / rate));


            return (uint) (record.Time + diffTime);
        }

        /// <summary>
        /// Быстрый и точный способ получения времени по скану, но только в пределах рабочего пространства т.е. +/- сотни метров, иначе будет обычный
        /// </summary>
        /// <param name="virtualScan">Скан</param>
        /// <returns>Время в мс</returns>
        public uint Scan2Time(int virtualScan)
        {
            uint time;
            return virtualCoordinateData.GetTime(virtualScan, out time) ? time : VirtualScan2VirtualTimeSlow(virtualScan);
        }

        public int Scan2RefCcdScan(int scan)
        {
            if (!virtualCoordinateData.ScanRange.IsIncludeWithLeftOnly(scan))
                BuildVirtualCoordinateData(scan);

            return virtualCoordinateData.GetRefCcdScan(scan);
        }

        /// <summary>
        /// Быстрый и точный способ получения угла по скану, но только в пределах рабочего пространства т.е. +/- сотни метров, иначе будет обычный
        /// </summary>
        /// <param name="virtualScan"></param>
        /// <returns>Угол в градусах</returns>
        public float Scan2Angle(int virtualScan)
        {
            ushort angle;
            return virtualCoordinateData.GetAngle(virtualScan, out angle) ? AngleCode2Angle(CalcParameters, angle) : AngleCode2Angle(CalcParameters, VirtualScan2Angle(virtualScan));
        }

        public static float AngleCode2Angle(CalcParameters calcParameters, ushort code)
        {
            if (calcParameters.TypeAngleSensor == enAngleType.AngleCode)
            {
                return code * 360.0f / 65536.0f;
            }
            return code;
        }

        private double VirtualScan2VirtualOdometer(int virtualScan)
        {
            var ccdScan = corruptedDataInfo.GetCcdScan(virtualScan);
            var ccdOdometer = CcdScan2CcdOdometer(ccdScan);

            var deltaInfo = ccdScanDeltaManager.GetDelta(ccdScan);

            var firstVirtualScan = corruptedDataInfo.GetVirtualScan(ccdScan);
            var deltaVirtualScan = virtualScan - firstVirtualScan;

            return ccdOdometer + deltaInfo.SumOdometerDelta + deltaVirtualScan;
        }

        internal uint VirtualScan2VirtualTimeSlow(int virtualScan)
        {
            var ccdScan = corruptedDataInfo.GetCcdScan(virtualScan);
            var ccdTime = CcdScan2CcdTimeSlow(ccdScan);

            var deltaInfo = ccdScanDeltaManager.GetDelta(ccdScan);

            var firstVirtualScan = corruptedDataInfo.GetVirtualScan(ccdScan);
            var deltaVirtualScan = virtualScan - firstVirtualScan;

            return (uint) (ccdTime + (deltaInfo.SumOdometerDelta + deltaVirtualScan)* averageTimePerOdometer) ;
        }

        private ushort VirtualScan2Angle(int virtualScan)
        {
            var ccdScan = corruptedDataInfo.GetCcdScan(virtualScan);
            return ReadRecord(CcdScan2NumberRecord(ccdScan)).Angle;
        }

        public float AngleOffset(float comparerAngleMm, double dist)
        {
            var scan = Dist2Scan(dist);
            return (Scan2Angle(scan) * mmInDegree - comparerAngleMm + pipeCircleModulFactor) % CalcParameters.PipeCircle;
        }

        public float GetAngle(float targetSensorAngleMm, float sensorTdcAngleMm, double dist)
        {
            var sensorTDCDeltaX = sensorTdcAngleMm + CalcParameters.AngleOffset;
            var angleOffset = AngleOffset(sensorTDCDeltaX, dist + SectionOffset);
            var targetAngle = ((float)Math.Round(((targetSensorAngleMm + angleOffset) / mmInDegree), 2) + 360) % 360;
            return targetAngle;
        }

        #endregion

        public void Dispose()
        {
            fileMapper?.Dispose();
            fileIDT?.Dispose();
            uninterruptedCoordinateData?.Dispose();
            virtualCoordinateData?.Dispose();
        }
    }
}

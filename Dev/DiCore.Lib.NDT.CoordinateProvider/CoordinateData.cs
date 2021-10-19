using System;
using System.Runtime.CompilerServices;
using Diascan.Utils.DataBuffers;
using DiCore.Lib.NDT.Types;

namespace DiCore.Lib.NDT.CoordinateProvider
{
    internal unsafe class CoordinateData : VectorBuffer<CoordinateItemFloat>
    {
        /// <summary>
        /// Шаг увеличения внутреннего буфера
        /// </summary>
        public int StepUpBuffer { get; set; }
        /// <summary>
        /// Шаг одометра, м
        /// </summary>
        public double OdoFactor { get; set; }
        /// <summary>
        /// Указатель на данные буфера
        /// </summary>
        public CoordinateItemFloat* DataPointer { get; private set; }
        /// <summary>
        /// Диапазон сканов в буфере
        /// </summary>
        public Range<int> ScanRange { get; set; }
        /// <summary>
        /// Валидность буфера
        /// </summary>
        private bool IsValid { get; set; }
        /// <summary>
        /// Вектор для быстрого поиска по дистанции
        /// </summary>
        private readonly VectorBuffer<int> distIndexBuffer;
        /// <summary>
        /// Указатель на вектор быстрого поиска по дистанции
        /// </summary>
        int* distIndex;
        /// <summary>
        /// Диапазон показаний одометра в буфере
        /// </summary>
        public Range<int> OdometerRange { get; private set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="count"></param>
        /// <param name="heap"> </param>
        public CoordinateData(UIntPtr heap, int count) : base(heap, count, 1f)
        {
            DataPointer = (CoordinateItemFloat*)Data;
            IsValid = false;
            distIndexBuffer = new VectorBuffer<int>(heap, count * 10, 1f);
        }

        protected override void AllocateNew(int size)
        {
            base.AllocateNew(size);
            DataPointer = (CoordinateItemFloat*)Data;
        }

        private void SetBufferSize(int needCount)
        {
            var deficit = needCount - (capacity / ElementSize);

            if (deficit <= 0) return;

            IsValid = false;
            var neededStepUp = deficit / StepUpBuffer + 1;
            ReAllocate(Count + StepUpBuffer * neededStepUp);
        }

        public bool GetDist(int virtualScan, out double dist)
        {
            if (CheckCoordinateDataByScan(virtualScan))
            {
                dist = DataPointer[virtualScan - ScanRange.Begin].Odometer * OdoFactor;
                return true;
            }
            dist = 0;
            return false;
        }

        public bool GetOdometer(int scan, out double odometer)
        {
            if (CheckCoordinateDataByScan(scan))
            {
                odometer = DataPointer[scan - ScanRange.Begin].Odometer;
                return true;
            }
            odometer = 0;
            return false;
        }

        public bool GetTime(int scan, out uint time)
        {
            if (CheckCoordinateDataByScan(scan))
            {
                time = DataPointer[scan - ScanRange.Begin].Time;
                return true;
            }
            time = 0;
            return false;
        }

        public bool GetAngle(int scan, out ushort angle)
        {
            if (CheckCoordinateDataByScan(scan))
            {
                angle = DataPointer[scan - ScanRange.Begin].Angle;
                return true;
            }
            angle = 0;
            return false;
        }

        internal int GetRefCcdScan(int scan)
        {
            if (CheckCoordinateDataByScan(scan))
                return DataPointer[scan - ScanRange.Begin].RefCCDScan;
            return -1;
        }

        public void BuildDistIndex()
        {
            var realCount = ScanRange.End - ScanRange.Begin;

            OdometerRange =
                new Range<int>(Convert.ToInt32(Math.Round(DataPointer[0].Odometer * 10, MidpointRounding.AwayFromZero)),
                    Convert.ToInt32(Math.Round(DataPointer[realCount].Odometer * 10,
                        MidpointRounding.AwayFromZero)));

            var count = OdometerRange.End - OdometerRange.Begin + 1;

            if (count < 2)
            {
                distIndexBuffer.ReAllocate(0);
                return;
            }

            distIndexBuffer.ReAllocate(count);

            distIndex = (int*) distIndexBuffer.Data;

            // ReSharper disable TooWideLocalVariableScope
            int scan;
            int scanOdometer;
            int nextScanOdometer;
            int lastScanOdometer;
            // ReSharper restore TooWideLocalVariableScope

            var lastNextScanOdometer = 0;

            distIndex[0] = ScanRange.Begin;

            for (var i = 0; i < realCount; i++)
            {
                scan = ScanRange.Begin + i;

                scanOdometer = lastNextScanOdometer;
                nextScanOdometer =
                    Convert.ToInt32(Math.Round(DataPointer[i + 1].Odometer * 10, MidpointRounding.AwayFromZero)) -
                    OdometerRange.Begin;

                lastScanOdometer = (scanOdometer + nextScanOdometer) / 2;

                for (var j = scanOdometer + 1; j <= lastScanOdometer; j++)
                    distIndex[j] = scan;

                for (var j = lastScanOdometer + 1; j <= nextScanOdometer; j++)
                    distIndex[j] = scan + 1;

                lastNextScanOdometer = nextScanOdometer;
            }

            IsValid = true;
        }

        public int? GetScan(double odometer, out bool? isSmaller)
        {
            isSmaller = null;
            //var odometer10 = Convert.ToInt32(Math.Round(odometer*10, MidpointRounding.AwayFromZero));
            var odometer10 = (int)Math.Floor(odometer * 10);

            bool isSmallerThenRange;
            if (CheckCoordinateDataByOdometer(odometer10, out isSmallerThenRange))
                return distIndex[odometer10 - OdometerRange.Begin];

            isSmaller = isSmallerThenRange;
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckCoordinateDataByInterval(int scan, int needCount)
        {
            SetBufferSize(needCount);

            if (IsValid)
            //Размер вектора был и так подходящий, проверим на диапазон сканов
            {
                if (!(ScanRange.IsInclude(true, scan)) || !(ScanRange.IsInclude(true, scan + needCount)))
                    IsValid = false;
            }
            return IsValid;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CheckCoordinateDataByScan(int scan)
        {
            return IsValid && ScanRange.IsInclude(true, scan);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckCoordinateDataByOdometer(int odometer10, out bool isSmaller)
        {
            isSmaller = false;
            return IsValid && OdometerRange.IsInclude(true, odometer10, out isSmaller);
        }

        protected override void Disposing()
        {
            base.Disposing();

            distIndexBuffer?.Dispose();
        }

        public void Reset()
        {
            ReAllocate(capacity / ElementSize);
        }
    }
}

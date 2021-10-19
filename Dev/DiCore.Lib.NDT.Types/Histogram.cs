using System;
using Diascan.Utils.DataBuffers;

namespace DiCore.Lib.NDT.Types
{
    public unsafe class Histogram: IDisposable
    {
        private readonly VectorBuffer<int> valuesFrequency;
        private readonly int* valuesFrequencyPtr;
        private readonly int shift;

        /// <summary>
        /// Количество значений наиболее часто встречаемой величины
        /// </summary>
        public int HighUsageFrequency { get; private set; }
        /// <summary>
        /// Наиболее часто встречаемая величина
        /// </summary>
        public float HighUsageValue { get; private set; }
        /// <summary>
        /// Общее количество значений в гистограмме
        /// </summary>
        public int Count { get; private set; }
        /// <summary>
        /// Емкость гистограммы
        /// </summary>
        public int Capacity { get; private set; }

        public Histogram(int capacity, int shift)
        {
            Capacity = capacity;
            this.shift = shift*10;

            valuesFrequency = new VectorBuffer<int>(capacity);
            valuesFrequencyPtr = (int*) valuesFrequency.Data;
        }

        public void Update(float value)
        {
            var index = CalcIndex(value);

            if (!(index > shift && index < Capacity)) return;

            var pCurValue = valuesFrequencyPtr + index;
            var curValue = *pCurValue;

            curValue++;
            *pCurValue = curValue;
            Count++;

            if (curValue < HighUsageFrequency) return;

            HighUsageFrequency = curValue;
            HighUsageValue = value;            
        }
        public void UpdateByIndexedValue(int value)
        {
            var index = (int)(value + 0.5) + shift;

            if (!(index > shift && index < Capacity)) return;
            
            var pCurValue = valuesFrequencyPtr + index;
            var curValue = *pCurValue;

            curValue++;
            *pCurValue = curValue;
            Count++;

            if (curValue < HighUsageFrequency) return;

            HighUsageFrequency = curValue;
            HighUsageValue = value/10f;            
        }

        public int GetFrequency(int index)
        {
            return *(valuesFrequencyPtr + index);
        }

        public int CalcIndex(float value)
        {
            return (int)(value * 10 + 0.5) // (0.5) Компенсация для правильного округления 
                + shift;
        }

        public void Reset()
        {
            Count = 0;
            HighUsageFrequency = 0;
            HighUsageValue = 0; //float.MinValue;
            NativeMemoryApi.ZeroMemory(valuesFrequency.Data, (uint)valuesFrequency.TotalSize);
        }

        public void Dispose()
        {
            valuesFrequency.Dispose();
        }
    }
}

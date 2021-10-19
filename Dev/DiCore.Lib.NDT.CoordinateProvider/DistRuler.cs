using System;
using Diascan.Utils.DataBuffers;
using DiCore.Lib.NDT.Types;

namespace DiCore.Lib.NDT.CoordinateProvider
{
    public unsafe class DistRuler: VectorBuffer<DistRuleItem>
    {
        private Range<int> distRange;
        private readonly VectorBuffer<int> numberIndexBuffer;
        private int* numberIndex;
        public DistRuleItem* DataPointer { get; private set; }

        public DistRuler(UIntPtr heap, int count) : base(heap, count, 1f)
        {
            DataPointer = (DistRuleItem*)Data;
            numberIndexBuffer = new VectorBuffer<int>(count * 4);
        }

        protected override void AllocateNew(int size)
        {
            base.AllocateNew(size);
            DataPointer = (DistRuleItem*)Data;
        }

        /// <summary>
        /// Возвращает элемент одометрической линейки по индексу (если индекс за пределами линейки - элемент пустой)
        /// </summary>
        /// <param name="itemNumber">Индекс в линейке</param>
        /// <returns>Элемент одометрической линейки</returns>
        public DistRuleItem GetItem(int itemNumber)
        {
            if (itemNumber >= Count || itemNumber < 0)
                return DistRuleItem.Empty;

            return DataPointer[itemNumber];
        }

        /// <summary>
        /// Построить индекс дистанций
        /// </summary>
        public void BuildDistIndex()
        {
            if (Count == 0)
            {distRange = new Range<int>(0, 0);
            return;}

                distRange =
                    new Range<int>(Convert.ToInt32(Math.Round(DataPointer[0].Dist * 1000, MidpointRounding.AwayFromZero)),
                                   Convert.ToInt32(Math.Round(DataPointer[Count - 1].Dist * 1000,
                                                              MidpointRounding.AwayFromZero)));

            var count = distRange.End - distRange.Begin + 1;

            if (count < 2)
            {
                numberIndexBuffer.ReAllocate(0);
                return;
            }

            numberIndexBuffer.ReAllocate(count);

            numberIndex = (int*)numberIndexBuffer.Data;

            // ReSharper disable TooWideLocalVariableScope
            int numberDist;
            int nextNumberDist;
            int lastNumberDist;
            // ReSharper restore TooWideLocalVariableScope

            var lastNextNumberDist = 0;

            numberIndex[0] = 0;

            for (var i = 0; i < Count - 1; i++)
            {
                numberDist = lastNextNumberDist;
                nextNumberDist = Convert.ToInt32(Math.Round(DataPointer[i + 1].Dist * 1000, MidpointRounding.AwayFromZero)) - distRange.Begin;

                lastNumberDist = (numberDist + nextNumberDist) / 2;

                for (var j = numberDist + 1; j <= lastNumberDist; j++)
                    numberIndex[j] = i;

                for (var j = lastNumberDist + 1; j <= nextNumberDist; j++)
                    numberIndex[j] = i + 1;

                lastNextNumberDist = nextNumberDist;
            }
        }

        /// <summary>
        /// Получить индекс ближайщего элемента одометрической линейки по дистанции
        /// </summary>
        /// <param name="dist">Дистанция поиска, м</param>
        /// <returns>Номер элемента, при дистанции за пределами линейки возвращается -1</returns>
        public int Dist2Number(double dist)
        {
            var dist1000 = Convert.ToInt32(Math.Round(dist * 1000, MidpointRounding.AwayFromZero));

            if (CheckDistRulerByDist(dist1000))
                return numberIndex[dist1000 - distRange.Begin];
            
            return -1;
        }

        /// <summary>
        /// Получить индекс ближайщего элемента одометрической линейки по скану
        /// </summary>
        /// <param name="scan">Номер скана</param>
        /// <returns>Номер элемента, при скане за пределами линейки возвращается -1</returns>
        public int Scan2Number(int scan)
        {
            var minScan = DataPointer[0].Scan;
            var maxScan = DataPointer[Count - 1].Scan;

            if ((scan < minScan) || (scan > maxScan))
                return -1;

            var step = (maxScan - minScan)/(double)(Count-1);

            var index = (scan - minScan)/step;

            return (int) Math.Round(index, MidpointRounding.AwayFromZero);
        }

        private bool CheckDistRulerByDist(int dist1000)
        {
            return distRange.IsInclude(true, dist1000);
        }
    }
}

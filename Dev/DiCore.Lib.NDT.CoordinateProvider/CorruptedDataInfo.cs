using System;
using System.Collections.Generic;
using System.Linq;

namespace DiCore.Lib.NDT.CoordinateProvider
{
    /// <summary>
    /// Объект инфраструктуры подсистемы исправления нарушений в диагностических данных (пропуски, задвойка данных)
    /// </summary>
    public sealed class CorruptedDataInfo
    {
        /// <summary>
        /// массив отсортирован по ключу (скану)
        /// </summary>
        private ConvertCcdToVirtualInfo[] convertCcdToVirtualInfo = new ConvertCcdToVirtualInfo[0];        

        private int minCcdScan;
        private int maxCcdScan;

        private int minVirtualScan;
        private int maxVirtualScan;
        private readonly Func<int, double> ccdScanToOdometer;

        public CorruptedDataInfo(Func<int, double> ccdScanToOdometer)
        {
            this.ccdScanToOdometer = ccdScanToOdometer;
            DeltasInfo = new List<KeyValuePair<int, int>>();
        }

        /// <summary>
        /// Участки коррекции из входного xml файла
        /// </summary>
        public List<KeyValuePair<int, int>> DeltasInfo { get; private set; }

        /// <summary>
        /// Признак наличия данных корректировки
        /// </summary>
        public bool Available { get; private set; }

        public int GetVirtualScan(int ccdScan)
        {
            if (!Available) return ccdScan;
            
            if (convertCcdToVirtualInfo.Where(item => item.CcdScanStart <= ccdScan).Any())
            {
                var targetItem = convertCcdToVirtualInfo.Where(item => item.CcdScanStart <= ccdScan).Last();
                return ccdScan <= targetItem.CcdScanStop
                    ? targetItem.VirtualScanStart
                    : ccdScan + targetItem.SumScanDelta + targetItem.CorruptedInfo;
            }

            return ccdScan;                       
        }

        public int GetCcdScan(int virtualScan)
        {
            if (!Available) return virtualScan;

            if (convertCcdToVirtualInfo.Where(item => item.VirtualScanStart <= virtualScan).Any())
            {
                var targetItem = convertCcdToVirtualInfo.Where(item => item.VirtualScanStart <= virtualScan).Last();
                return virtualScan <= targetItem.VirtualScanStop
                    ? targetItem.CcdScanStart
                    : virtualScan - targetItem.SumScanDelta - targetItem.CorruptedInfo;
            }
            
            return virtualScan;
        }


        public double GetCcdOdometer(double virtualOdometer)
        {
            if (!Available) return virtualOdometer;

            if (convertCcdToVirtualInfo.Where(item => item.OdometerStart <= virtualOdometer).Any())
            {
                var targetItem = convertCcdToVirtualInfo.Where(item => item.VirtualScanStart <= virtualOdometer).Last();
                return virtualOdometer < targetItem.OdometerStart + targetItem.CurrentOdometerDelta
                    ? virtualOdometer - targetItem.SumOdometerDelta
                    : virtualOdometer - targetItem.SumOdometerDelta - targetItem.CurrentOdometerDelta;
            }

            return virtualOdometer;
        }

        public DeltaInfo[] GetCcdScanDeltas(int ccdScan, int count)  // суммарная по сканам (+, -); суммарная по одометрам и текущая коррекция на скан
        {
            if (!Available) return new DeltaInfo[count];

            var start = ccdScan;
            var stop = start + count;

            var firstItem = convertCcdToVirtualInfo.Where(item => item.CcdScanStart <= ccdScan).LastOrDefault(); 
            var targetItems = convertCcdToVirtualInfo.Where(item => item.CcdScanStart < stop && item.CcdScanStart > start).ToList();           

            if (targetItems.Count == 0)
            {
                var deltaInfo = new DeltaInfo(firstItem, ccdScan);
                return Enumerable.Repeat(deltaInfo, count).ToArray();
            }

            targetItems.Insert(0, firstItem);
            
            var cdi = new DeltaInfo[count];
           
            for (int index = 0; index < targetItems.Count; index++)
            {
                var item = targetItems[index];

                var rangeStart = Math.Max(item.CcdScanStart, ccdScan);
                var rangeStop = (index + 1) < targetItems.Count ? targetItems[index + 1].CcdScanStart : ccdScan + count;

                for (var i = rangeStart; i < rangeStop; i++)
                {
                    cdi[i - ccdScan] = new DeltaInfo(item, i);
                }
            }

            return cdi;
        }

        private void Build()
        {
            var count = DeltasInfo.Count;
            Available = count > 0;

            if (count == 0) return;

            DeltasInfo.Sort((item1, item2) => item1.Key - item2.Key);            

            convertCcdToVirtualInfo = new ConvertCcdToVirtualInfo[count];
            var ccdScanDelta = 0;
            var virtualScanDelta = 0;
            var odometerDelta = 0.0;

            for (int i = 0; i < count; i++)
            {
                var source = DeltasInfo[i];
                var ccdScanCurrentDelta = Math.Max(0, -source.Value);
                var virtualScanCurrentDelta = Math.Max(0, source.Value);

                var dest = new ConvertCcdToVirtualInfo
                {
                    CcdScanStart = source.Key,
                    CcdScanDelta = ccdScanDelta,

                    VirtualScanStart = source.Key + virtualScanDelta - ccdScanDelta,
                    VirtualScanDelta = virtualScanDelta,

                    CorruptedInfo = source.Value,
                    SumScanDelta = virtualScanDelta - ccdScanDelta
                };

                dest.CcdScanStop = dest.CcdScanStart + ccdScanCurrentDelta;
                dest.VirtualScanStop = dest.VirtualScanStart + virtualScanCurrentDelta;

                var startCcdOdometr = ccdScanToOdometer(source.Key);
                dest.OdometerStart = startCcdOdometr + odometerDelta;
                dest.SumOdometerDelta = odometerDelta;                

                if (source.Value > 0)
                {
                    dest.CurrentOdometerDelta = source.Value;
                }
                else
                {
                    dest.CurrentOdometerDelta = -(ccdScanToOdometer(source.Key + -source.Value) - startCcdOdometr);
                }                
                
                ccdScanDelta += ccdScanCurrentDelta;
                virtualScanDelta += virtualScanCurrentDelta;
                odometerDelta += dest.CurrentOdometerDelta;

                convertCcdToVirtualInfo[i] = dest;
            }

            minCcdScan = convertCcdToVirtualInfo.First().CcdScanStart;
            minVirtualScan = convertCcdToVirtualInfo.First().VirtualScanStart;
            maxCcdScan = convertCcdToVirtualInfo.Last().CcdScanStart;
            maxVirtualScan = convertCcdToVirtualInfo.Last().VirtualScanStart;
        }

        #region Загрузка и сохранение
        /// <summary>
        /// Загрузить информацию о восстановлении диаг. данных из xml файла
        /// Структура файла:
        ///     <CorruptedDataInfo>
        ///     </CorruptedDataInfo>
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        /// <returns></returns>
        public static CorruptedDataInfo LoadFromXml(string path, Func<int, double> ccdScanToOdometer)
        {
            var cdi = new CorruptedDataInfo(ccdScanToOdometer);

            //cdi.DeltasInfo = new List<KeyValuePair<int, int>>(new[]
            //    {
            //        new KeyValuePair<int, int>(1000, 200), new KeyValuePair<int, int>(2000, -500),
            //        new KeyValuePair<int, int>(3000, -500), new KeyValuePair<int, int>(4000, 1000),                    
            //        new KeyValuePair<int, int>(7, 3), new KeyValuePair<int, int>(11, 3),
            //        new KeyValuePair<int, int>(16, -7), new KeyValuePair<int, int>(25, -4),
            //        new KeyValuePair<int, int>(32, 5), new KeyValuePair<int, int>(33, -4)
            //    });
            //cdi.Build();

            //if (!File.Exists(path)) return cdi;

            //try
            //{
            //    var xmlFile = XElement.Load(path);

            //    var deltasInfo = xmlFile.Elements("CorruptedDataInfo")
            //        .Select(
            //            element =>
            //                new KeyValuePair<int, int>((int)element.Attribute("scan"), (int)element.Attribute("info")))
            //        .OrderBy(item => item.Key).ToArray();

            //    cdi.Build();
            //}
            //catch
            //{
            //}

            return cdi;
        }

        public static void SaveToXml(CorruptedDataInfo source, string path)
        {

        }

        public static string GetDescription(KeyValuePair<int, int> delta)
        {
            return "коррекция: " + delta.Value;
        }
        #endregion
    }

    internal struct ConvertCcdToVirtualInfo
    {
        public int CcdScanStart;
        public int CcdScanStop;
        /// <summary>
        /// Только положительные значения. Накопленное смещение для сканов ccd
        /// </summary>
        public int CcdScanDelta;

        /// <summary>
        /// Информация о восстановлении данных для текущего участка
        /// </summary>
        public int CorruptedInfo;

        public int VirtualScanStart;
        public int VirtualScanStop;
        /// <summary>
        /// Только положительные значения. Накопленное значение добавленных виртуальных сканов
        /// </summary>
        public int VirtualScanDelta;

        /// <summary>
        /// Суммарная коррекция сканов
        /// </summary>
        public int SumScanDelta;

        /// <summary>
        /// Накопленное смещение одометра. Для вставленных данных учитывается идеальный одометр (3,3 мм), 
        /// для вырезанных - реальный с учетом скорости снаряда на интервале коррекции
        /// </summary>
        public double SumOdometerDelta;

        /// <summary>
        /// Виртуальный одометр ccd (с коррекцией)
        /// </summary>
        public double OdometerStart;

        public double CurrentOdometerDelta;

        public static ConvertCcdToVirtualInfo Empty = new ConvertCcdToVirtualInfo();
    }

    public struct DeltaInfo
    {
        /// <summary>
        /// Суммарная коррекция сканов
        /// </summary>
        public int SumScanDelta;
        /// <summary>
        /// Информация о восстановлении данных для текущего участка
        /// </summary>
        public int CorruptedInfo;
        /// <summary>
        /// Накопленное смещение одометра. Для вставленных данных учитывается идеальный одометр (3,3 мм), 
        /// для вырезанных - реальный с учетом скорости снаряда на интервале коррекции
        /// </summary>
        public double SumOdometerDelta;

        internal DeltaInfo(ConvertCcdToVirtualInfo source, int targetCcdScan)
            : this()
        {
            if (source.CcdScanStart == targetCcdScan)
            {
                SumScanDelta = source.SumScanDelta;
                CorruptedInfo = source.CorruptedInfo;
                SumOdometerDelta = source.SumOdometerDelta;
            }
            else
            {
                SumScanDelta = source.SumScanDelta + source.CorruptedInfo;
                CorruptedInfo = 0;
                SumOdometerDelta = source.SumOdometerDelta + source.CurrentOdometerDelta;
            }
        }

        public static DeltaInfo Empty = new DeltaInfo();
    }
}

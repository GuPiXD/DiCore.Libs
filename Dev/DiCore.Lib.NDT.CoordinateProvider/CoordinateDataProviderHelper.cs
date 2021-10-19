using System;
using DiCore.Lib.NDT.Types;

namespace DiCore.Lib.NDT.CoordinateProvider
{
    public static class CoordinateDataProviderHelper
    {
        public static int COORDINATE_ITEM_SIZE;
        internal static int COORDINATE_ITEM_FLOAT_SIZE;

        static unsafe CoordinateDataProviderHelper()
        {
            COORDINATE_ITEM_SIZE = sizeof(CoordinateItem);
            COORDINATE_ITEM_FLOAT_SIZE = sizeof(CoordinateItemFloat);
        }
        
        public static int CalculateSpeedStatistics(this CoordinateDataProviderCrop coordinateDataProviderCrop, out float minSpeed, out float maxSpeed, out float aveSpeed)
        {
            minSpeed = Single.MaxValue;
            maxSpeed = Single.MinValue;

            var step = (int)(10 / coordinateDataProviderCrop.CalcParameters.OdoFactor) / coordinateDataProviderCrop.odometerPeriod;
            var virtualScan = (int)coordinateDataProviderCrop.MinScan;

            var lastDist = coordinateDataProviderCrop.Scan2Dist(virtualScan);
            var lastTime = coordinateDataProviderCrop.VirtualScan2VirtualTimeSlow(virtualScan);

            var speedSumm = 0d;
            virtualScan += step;
            var count = 0;

            while (virtualScan < coordinateDataProviderCrop.MaxScan)
            {
                var dist = coordinateDataProviderCrop.Scan2Dist(virtualScan);
                var time = coordinateDataProviderCrop.VirtualScan2VirtualTimeSlow(virtualScan);

                if (time > lastTime)
                {
                    var speed = (float)(dist - lastDist) / ((time - lastTime) / 1000f);
                    if (speed <= minSpeed)
                        minSpeed = speed;
                    if (speed >= maxSpeed)
                        maxSpeed = speed;

                    speedSumm += speed;
                }
                lastTime = time;
                lastDist = dist;
                virtualScan++;
                count++;
            }

            aveSpeed = (float)(speedSumm / count);

            return step;
        }

        public static SpeedInfo[] GetSpeedInfo(this CoordinateDataProviderCrop coordinateDataProvider, int firstScan, int countScan, int step, Action<int, int> progress = null)
        {
            var values = new SpeedInfo[countScan];

            var stepValue = 100f / countScan;

            for (var i = 0; i < countScan; i++)
            {
                var scan = firstScan + i * step;
                var nextScan = firstScan + (i + 1) * step;
                var dist1 = coordinateDataProvider.Scan2Dist(scan);
                var dist2 = coordinateDataProvider.Scan2Dist(nextScan);
                var time1 = coordinateDataProvider.Scan2Time(scan) / 1000.0;
                var time2 = coordinateDataProvider.Scan2Time(nextScan) / 1000.0;

                values[i] = new SpeedInfo { Distance = (dist1 + dist2) / 2, Time = (time2 + time1) / 2 };

                if (MathHelper.TestFloatEquals(time1, time2))
                    values[i].Speed = 999f;
                else
                    values[i].Speed = (float)((dist2 - dist1) / (time2 - time1));

                if (i % 10000 == 0)
                {
                    progress?.Invoke((int)(i * stepValue), 100);
                }
            }
            return values;
        }

        public static void FillSensorAligningMap(this CoordinateDataProviderCrop providerCrop,
            int scan,
            int count,
            int compressStep,
            float alignmentFactor,
            SensorsByDeltaScanOrdering sensorsByDeltaScanOrdering)
        {
            if (!providerCrop.IsOpened)
                return;

            const int halfSpeedCalcInterval = 5;

            var scanStop = scan + count*compressStep;

            var halfIntervalDistance = providerCrop.Scan2Dist((scan+scanStop)/2);

            var speedCalcIntervalStart = Math.Max(providerCrop.MinDistance, halfIntervalDistance - halfSpeedCalcInterval);
            var speedCalcIntervalStop = Math.Min(providerCrop.MaxDistance, halfIntervalDistance + halfSpeedCalcInterval);
            scan = providerCrop.Dist2Scan(speedCalcIntervalStart);
            scanStop = providerCrop.Dist2Scan(speedCalcIntervalStop);

            //Расстояние между сканами в мм
            var deltaOdometer = (providerCrop.Scan2Dist(scanStop) - providerCrop.Scan2Dist(scan))*1000;
            var deltaScan = scanStop - scan;
            //Время между сканами в мкс
            var deltaTime = (providerCrop.Scan2Time(scanStop) - providerCrop.Scan2Time(scan))*1000;

            var averageOdometer = deltaOdometer / deltaScan;

            //Средняя скорость мм/мкс
            var averageSpeed = deltaOdometer/deltaTime;
            
            if (MathHelper.TestFloatEquals(alignmentFactor, 1, 0.001))
                alignmentFactor = 1;
            
            foreach (var sensor in sensorsByDeltaScanOrdering.Items)
            {
                var dy = sensor.Dy*alignmentFactor;
                var delay = sensor.Delay*averageSpeed;

                sensor.DeltaScan =
                    (short)
                    (Math.Round((dy - delay)/averageOdometer, MidpointRounding.AwayFromZero));
                sensor.AdditionalAligmentInputInDeltaScan =
                    (short)
                    (Math.Round(dy * (alignmentFactor - 1) / averageOdometer,
                        MidpointRounding.AwayFromZero));
            }
         }

        /// <summary>
        /// Заполнение вектора поворота снаряда в мм
        /// </summary>
        /// <param name="providerCrop">Координатный провайдер</param>
        /// <param name="scan">Скан начала</param>
        /// <param name="count">Количество показаний</param>
        /// <param name="compressStep">Шаг в сканах между показаниями</param>
        public static float[] GetRotationVector(this CoordinateDataProviderCrop providerCrop, int scan, int count, int compressStep)
        {
            if (!providerCrop.IsOpened)
                return null;

            var result = new float[count];

            var pipeCircle = providerCrop.CalcParameters.PipeCircle;
            var mmInDegree = pipeCircle / 360;

            var delta = pipeCircle * 2f;
            var firstMm = CoordinateDataProviderCrop.AngleCode2Angle(providerCrop.CalcParameters, providerCrop.firstRecordInFile.Angle) * mmInDegree % pipeCircle;

            for (var i = 0; i < count; i++)
            {
                result[i] = (providerCrop.Scan2Angle(i * compressStep + scan) * mmInDegree - firstMm + delta) % pipeCircle;
            }

            return result;
        }

        /// <summary>
        /// Заполнение вектора поворота снаряда в сенсорах
        /// </summary>
        /// <param name="providerCrop">Координатный провайдер</param>
        /// <param name="scan">Скан начала</param>
        /// <param name="count">Количество показаний</param>
        /// <param name="compressStep">Шаг в сканах между показаниями</param>
        public static int[] GetRotationVectorSensors(this CoordinateDataProviderCrop providerCrop, int scan, int count, int compressStep, int sensorCount)
        {
            if (!providerCrop.IsOpened)
                return null;

            var result = new int[count];

            var pipeCircle = providerCrop.CalcParameters.PipeCircle;
            var mmInDegree = pipeCircle / 360;
            var mmInSensor = pipeCircle / sensorCount;

            var delta = pipeCircle * 2f;
            var firstMm = CoordinateDataProviderCrop.AngleCode2Angle(providerCrop.CalcParameters, providerCrop.firstRecordInFile.Angle) * mmInDegree % pipeCircle;

            for (var i = 0; i < count; i++)
            {
                var deltaMm = (providerCrop.Scan2Angle(i * compressStep + scan) * mmInDegree - firstMm + delta) % pipeCircle;
                result[i] = ((int) (Math.Round(deltaMm / mmInSensor, MidpointRounding.AwayFromZero)));
            }

            // у первого сенсора смещение 0, дальнейшие смещения приводим к интервалу [0,sensorCount)
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = (result[i] - result[0] + sensorCount) % sensorCount;
                if (result[i] > sensorCount)
                {
                    result[i] = result[i] % sensorCount;
                }
                else
                {
                    while (result[i]<0)
                    {
                        result[i] += sensorCount;
                    }
                }
            }

            return result;
        }

        public static float[] GetRotationVectorByAngle(this CoordinateDataProviderCrop providerCrop, int scan, int count, int compressStep)
        {
            if (!providerCrop.IsOpened)
                return null;

            var result = new float[count];

            for (var i = 0; i < count; i++)
                result[i] = (providerCrop.Scan2Angle(i * compressStep + scan));

            var firstValue = result[0];
            for (var i = 0; i < count; i++)
                result[i] = (result[i] - firstValue + 360) % 360;
 
            return result;
        }
    }
}

using System;
using System.Globalization;
using System.Linq;
using Diascan.Utils.DataBuffers;
using DiCore.Lib.NDT.Carrier;

namespace DiCore.Lib.NDT.Types
{
    public static class ParametersHelper
    {
        /// <summary>
        /// Коэффициент перевода из мм в дюймы
        /// </summary>
        public const float MmInInch = 25.4f;

        public static int AngleToMm(float pipeDiamMm, float angle)
        {
            return (int)Math.Round(pipeDiamMm * Math.PI * angle / 360f, MidpointRounding.AwayFromZero);
        }

        public static float MmToAngle(float pipeDiamMm, float mm)
        {
            return (float)(mm * 360f / (pipeDiamMm * Math.PI));
        }

        /// <summary>
        /// Возращает диаметр трубы по ГОСТ в мм
        /// </summary>
        /// <param name="pipeDiamInch">Диаметр в дюймах</param>
        /// <returns>диаметр</returns>
        public static float GetGOSTPipeDiamMm(int pipeDiamInch)
        {
            switch (pipeDiamInch)
            {
                case 6:
                    return 159f;
                case 8:                 //E.R. Файл параметров терминала ОПТ6-8 (В соответствии с ТЗ на прибор)
                    return 219f;
                case 10:
                    return 273f;
                case 12:
                    return 325f;
                case 14:
                    return 377f;
                case 16:
                    return 426f;
                case 20:
                    return 530f;
                case 24:
                    return 630f;
                case 28:
                    return 720f;
                case 32:
                    return 820f;
                case 38:
                    return 965.2f;
                case 40:
                    return 1020f;
                case 42:
                    return 1067f;
                case 44:
                    return 1117.6f;
                case 48:
                    return 1220f;
                default:
                    return pipeDiamInch * MmInInch;
            }

            //switch (pipeDiamINCH)
            //{
            //    case 12:
            //        return 310f;
            //    case 14:
            //        return 361f;
            //    case 20:
            //        return 520f;
            //    case 24:
            //        return 610f;
            //    case 28:
            //        return 705f;
            //    case 32:
            //        return 804f;
            //    case 38:
            //        return 944.6f;
            //    case 40:
            //        return 1000f;
            //    case 42:
            //        return 1046.2f;
            //    case 44:
            //        return 1097f;
            //    case 48:
            //        return 1195f;
            //    default:
            //        return pipeDiamINCH * MM_IN_INCH;
            //}
        }

        public static string BuildDateTimeString(DateTime startDateTime, uint curRelativeTime)
        {
            var curDateTime = startDateTime.AddMilliseconds(curRelativeTime);
            return curDateTime.ToLongDateString() + " " + curDateTime.ToLongTimeString();
        }

        public static string BuildTimeString(DateTime startDateTime, uint curRelativeTime)
        {
            var curDateTime = startDateTime.AddMilliseconds(curRelativeTime);
            return curDateTime.Hour + ":" + curDateTime.Minute.ToString("00") + ":" + curDateTime.Second.ToString("00");
        }

        public static DateTime ParseString2DateTime(string startDateTimeString)
        {
            var result = DateTime.MinValue;
            if (startDateTimeString == null) return result;

            var input = startDateTimeString.Split(new[] { ' ' });
            if (input.Length < 2) return result;
            var timeString = input[0];
            var dateString = input[1];

            var inputTime = timeString.Split(new[] { ':' });
            if (inputTime.Length < 3) return result;

            int hours;
            int minutes;
            int seconds;
            if (!Int32.TryParse(inputTime[0], out hours) || !Int32.TryParse(inputTime[1], out minutes) || !Int32.TryParse(inputTime[2], out seconds))
                return result;

            var inputDate = dateString.Split(new[] { '.' });
            if (inputDate.Length < 3) return result;

            int day;
            int month;
            int year;
            if (!Int32.TryParse(inputDate[0], out day) || !Int32.TryParse(inputDate[1], out month) || !Int32.TryParse(inputDate[2], out year))
                return result;

            if (year > 50) year += 1900;
            else year += 2000;

            return new DateTime(year, month, day, hours, minutes, seconds);
        }

        public static string DecimalSeparatorToParse(string customString)
        {
            if (String.IsNullOrEmpty(customString)) return String.Empty;

            var currentSystemFormatInfo = NumberFormatInfo.CurrentInfo;
            customString = customString.Replace(",", currentSystemFormatInfo.NumberDecimalSeparator);
            customString = customString.Replace(".", currentSystemFormatInfo.NumberDecimalSeparator);

            return customString;
        }

        public static unsafe void InitSensorAligningMap(this VectorBuffer<SensorAligningItem> map, Carrier.Carrier carrier, double pipeCircle, bool isReverse)
        {
            IOrderedEnumerable<Sensor> sensors = null;
            if (isReverse)
                sensors = from sensor in carrier
                    orderby Math.Round(sensor.Dx % carrier.Circle, 2, MidpointRounding.AwayFromZero) descending,
                        sensor.Angle descending
                    select sensor;
            else
                sensors = from sensor in carrier
                    orderby Math.Round(sensor.Dx % carrier.Circle, 2, MidpointRounding.AwayFromZero),
                        sensor.Angle descending
                    select sensor;
            var mapPtr = (SensorAligningItem*)map.Data;

            short index = (short)(isReverse ? 1 : 0);   // чтобы сенсор с наименьшим Dx (в реверсе он последний) встал на нулевой индекс           
            foreach (var sensor in sensors)
            {
                if (isReverse)
                    sensor.Dy = -sensor.Dy;
                var mapIndex = sensor.LogicalNumber - 1;
                if (mapIndex < 0 || mapIndex >= map.Count)
                    throw new OverflowException($"InitSensorAligningMap error. Wrong sensor logical number: {sensor.LogicalNumber}). Map size: {map.Count}");

                mapPtr[mapIndex].DeltaX = (float)(sensor.Dx / carrier.Circle * pipeCircle);
                mapPtr[mapIndex].DeltaY = sensor.Dy;
                mapPtr[mapIndex].TimeDelay = sensor.Delay;
                mapPtr[mapIndex].IndexOnRing = index++;
                mapPtr[mapIndex].IndexOnRing %= carrier.SensorCount;
                mapPtr[mapIndex].SensorRayDirection = (byte) (sensor.Angle > 0 ? enSensorRayDirection.CW : enSensorRayDirection.CCW);
                mapPtr[mapIndex].OpposedSensorNumber = (short)(sensor.OpposedLogicalNumber - 1);
            }
        }

        public static unsafe void FillSensorAligningIndexesByIndexOnRing(this VectorBuffer<SensorAligningItem> map, VectorBuffer<short> indexes)
        {
            var mapPtr = (SensorAligningItem*)map.Data;
            var indexesPtr = (short*)indexes.Data;
            var count = map.Count;

            for (short i = 0; i < count; i++)
            {
                var index = mapPtr[i].IndexOnRing;
                indexesPtr[index] = i;
            }
        }
    }

    [Flags]
    public enum enSensorRayDirection : byte
    {
        Unknown = 0,
        CW = 1,
        CCW = 2,
        CW_CCW = CW | CCW
    }
}

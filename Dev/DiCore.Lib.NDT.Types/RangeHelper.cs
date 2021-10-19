using System;
using System.Runtime.CompilerServices;

namespace DiCore.Lib.NDT.Types
{
    public static class RangeHelper
    {
        #region Функции для типа Range<T>

        /// <summary>
        /// Проверка принадлежности диапазону
        /// </summary>
        /// <param name="range"></param>
        /// <param name="boundaryValueInclude">Учитывать граничные значения</param>
        /// <param name="value">Проверяемое значение</param>
        /// <returns>Результат проверки</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInclude<T>(this Range<T> range, bool boundaryValueInclude, T value) where T : struct, IComparable<T>
        {
            if (boundaryValueInclude)
            {
                return (value.CompareTo(range.Begin) >= 0) && (value.CompareTo(range.End) <= 0);
            }

            return (value.CompareTo(range.Begin) > 0) && (value.CompareTo(range.End) < 0);
        }

        public static bool IsInclude<T>(this Range<T> range, bool leftBoundaryInclude, bool righttBoundaryInclude, T value) where T : struct, IComparable<T>
        {
            if (leftBoundaryInclude && righttBoundaryInclude)
                return (value.CompareTo(range.Begin) >= 0) && (value.CompareTo(range.End) <= 0);
            if (leftBoundaryInclude)
                return (value.CompareTo(range.Begin) >= 0) && (value.CompareTo(range.End) < 0);
            if (righttBoundaryInclude)
                return (value.CompareTo(range.Begin) > 0) && (value.CompareTo(range.End) <= 0);

            return (value.CompareTo(range.Begin) > 0) && (value.CompareTo(range.End) < 0);
        }

        /// <summary>
        /// Проверка принадлежности диапазону (без учета границ)
        /// </summary>
        /// <param name="range"></param>
        /// <param name="value">Проверяемое значение</param>
        /// <returns>Результат проверки</returns>
        public static bool IsInclude<T>(this Range<T> range, T value) where T : struct, IComparable<T>
        {
            return IsInclude(range, false, value);
        }

        /// <summary>
        /// Проверка принадлежности диапазону
        /// </summary>
        /// <param name="range"></param>
        /// <param name="boundaryValueInclude">Учитывать граничные значения</param>
        /// <param name="value">Проверяемое значение</param>
        /// <param name="isSmaller">Меньше диапазона в случае непринадлежности</param>
        /// <returns>Результат проверки</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInclude<T>(this Range<T> range, bool boundaryValueInclude, T value, out bool isSmaller) where T : struct, IComparable<T>
        {
            isSmaller = false;

            if (IsInclude(range, boundaryValueInclude, value))
                return true;

            if (boundaryValueInclude)
            {
                if (value.CompareTo(range.Begin) < 0)
                    isSmaller = true;
            }
            else
            {
                if (value.CompareTo(range.Begin) <= 0)
                    isSmaller = true;
            }

            return false;
        }

        /// <summary>
        /// Проверка принадлежности диапазону с коррекцией (граничные значения учитываются)
        /// </summary>
        /// <param name="range"></param>
        /// <param name="value">Проверяемое значение</param>
        /// <returns>Результат проверки</returns>
        public static bool IsIncludeWithCorrection<T>(this Range<T> range, ref T value) where T : struct, IComparable<T>
        {
            if (IsInclude(range, true, value))
                return true;

            if (value.CompareTo(range.Begin) < 0)
            {
                value = range.Begin;
                return false;
            }

            if (value.CompareTo(range.End) > 0)
            {
                value = range.End;
                return false;
            }
            return false;
        }

        /// <summary>
        /// Проверка принадлежности диапазону c учетом только левой границы (меньшая величина)
        /// </summary>
        /// <param name="range"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsIncludeWithLeftOnly<T>(this Range<T> range, T value) where T : struct, IComparable<T>
        {
            return (value.CompareTo(range.Begin) >= 0) && (value.CompareTo(range.End) < 0);
        }

        /// <summary>
        /// Вычисление расстояния до границы интервала
        /// </summary>
        /// <param name="range"></param>
        /// <param name="value">Проверяемое значение</param>
        /// <returns>значение по умолчанию - внутри интервала, меньше 0 - слева, больше 0 - справа. Значение равно расстоянию от границы</returns>
        //public static T ToBorderSpace<T>(this Range<T> range, T value) where T : struct, IComparable<T>
        //{
        //    if ((value.CompareTo(range.Begin) >= 0) && (value.CompareTo(range.End) <= 0))
        //        return default(T);

        //    var border = value.CompareTo(range.Begin) < 0 ? range.Begin : range.End;

        //    ParameterExpression paramA = Expression.Parameter(typeof(T), "a"),
        //        paramB = Expression.Parameter(typeof(T), "b");
        //    var body = Expression.Subtract(paramA, paramB);
        //    var sub = Expression.Lambda<Func<T, T, T>>(body, paramA, paramB).Compile();
        //    return sub(value, border);
        //}

        public static bool Contains(this Range<float> range, float value)
        {
            return !(value < range.Begin || value > range.End);
        }

        public static bool Contains(this Range<uint> range, uint value)
        {
            return !(value < range.Begin || value > range.End);
        }

        public static bool Contains<T>(this Range<T> range, Range<T> value) where T : struct, IComparable<T>
        {
            return value.Begin.CompareTo(range.Begin) >= 0 && value.End.CompareTo(range.End) <= 0;
        }

        public static bool IsEmpty<T>(this Range<T> range) where T : struct, IComparable<T>
        {
            return (range.Begin.CompareTo(default(T)) == 0) && (range.End.CompareTo(default(T)) == 0);
        }

        public static float Length(this Range<float> range)
        {
            return range.End - range.Begin;
        }

        public static double Length(this Range<double> range)
        {
            return range.End - range.Begin;
        }

        public static int Length(this Range<int> range)
        {
            return range.End - range.Begin;
        }

        public static long Length(this Range<long> range)
        {
            return range.End - range.Begin;
        }

        public static double Average(this Range<double> range)
        {
            return (range.End + range.Begin) / 2;
        }

        public static bool IsIntersect<T>(this Range<T> range, Range<T> source) where T : struct, IComparable<T>
        {
            return range.End.CompareTo(source.Begin) > 0 && source.End.CompareTo(range.Begin) > 0;
        }

        public static bool IsInclude<T>(this Range<T> range, Range<T> source) where T : struct, IComparable<T>
        {
            return source.Begin.CompareTo(range.Begin) > 0 && range.End.CompareTo(source.End) > 0;
        }

        public static Range<double> Add(this Range<double> source, Range<double> target)
        {
            return new Range<double>(Math.Min(source.Begin, target.Begin), Math.Max(source.End, target.End));
        }

        public static Range<T> Normalize<T>(this Range<T> range) where T : struct, IComparable<T>
        {
            return range.Begin.CompareTo(range.End) > 0 ? new Range<T>(range.End, range.Begin) : range;
        }

        public static void InitMinMaxRange<T>(ref Range<T> range, T minValue, T maxValue)
        {
            range.Begin = maxValue;
            range.End = minValue;
        }

        public static void UpdateMinMaxRange<T>(ref Range<T> range, T newValue) where T : IComparable<T>
        {
            if (newValue.CompareTo(range.End) > 0)
                range.End = newValue;

            if (range.Begin.CompareTo(newValue) > 0)
                range.Begin = newValue;
        }

        public static Range<T> CalcMinMax<T>(params Range<T>[] ranges) where T : IComparable<T>
        {
            var minMax = ranges[0];

            for (int i = 1; i < ranges.Length; i++)
            {
                var value = ranges[i];

                if (minMax.Begin.CompareTo(value.Begin) > 0)
                    minMax.Begin = value.Begin;

                if (minMax.End.CompareTo(value.End) < 0)
                    minMax.End = value.End;
            }

            return minMax;
        }

        public static bool Equal<T>(this Range<T> range, Range<T> otherRange) where T : struct, IComparable<T>
        {
            range = range.Normalize();
            otherRange = otherRange.Normalize();
            return range.Begin.CompareTo(otherRange.Begin) == 0 && range.End.CompareTo(otherRange.End) == 0;
        }

        #endregion
    }
}

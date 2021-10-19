using System;

namespace DiCore.Lib.SqlDataQuery.QueryParameter
{
    /// <summary>
    /// Сортировка
    /// </summary>
    public class Sorting
    {
        public string Direction { get; set; }
        public string Column { get; set; }
        public enSortNullPosition NullPosition { get; set; } = enSortNullPosition.None;
    }

    public enum enSortNullPosition
    {
        None = 0,
        First = 1,
        Last = 2
    }

    public static class SortingExtensions
    {
        public static string ToSql(this enSortNullPosition position)
        {
            string nullPosition;

            switch (position)
            {
                case enSortNullPosition.First:
                    nullPosition = " NULLS FIRST";
                    break;

                case enSortNullPosition.Last:
                    nullPosition = " NULLS LAST";
                    break;

                case enSortNullPosition.None:
                default:
                    nullPosition = String.Empty;
                    break;
            }

            return nullPosition;
        }
    }
}
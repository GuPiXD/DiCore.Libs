using System;
using System.Collections.Generic;
using System.Linq;

namespace DiCore.Lib.SqlDataQuery.QueryParameter
{
    /// <summary>
    /// Фильтры для столбца БД
    /// </summary>
    public class ColumnFilters
    {
        public string Logic { get; set; }
        public List<Filter> Filters { get; set; }

        /// <summary>
        /// Добавление фильтра
        /// </summary>
        /// <param name="filter">Фильтр</param>
        public void AddFilter(Filter filter)
        {
            if (Filters == null)
                Filters = new List<Filter>();

            Filters.Add(filter);
        }

        public override string ToString()
        {
            return Filters?.Skip(1).Aggregate(Filters?.First().ToString(), (str, filter) => $"{str} {Logic} {filter}") ?? base.ToString();
        }
    }
}
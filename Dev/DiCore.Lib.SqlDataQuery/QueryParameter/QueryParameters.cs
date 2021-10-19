using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DiCore.Lib.SqlDataQuery.QueryParameter
{
    public class QueryParameters
    {
        /// <summary>
        /// Номер страницы с единицы
        /// </summary>
        public int? Page { get; set; }
        /// <summary>
        /// Размер страницы
        /// </summary>
        public int? PageSize { get; set; }
        public int? Skip { get; set; }
        public int? Take { get; set; }
        /// <summary>
        /// Список сортировок
        /// </summary>
        public List<Sorting> Sortings { get; set; } = new List<Sorting>();
        /// <summary>
        /// Список фильтров
        /// </summary>
        public List<ColumnFilters> Filters { get; set; } = new List<ColumnFilters>();
        /// <summary>
        /// Словарь видимых столбцов
        /// </summary>
        public IDictionary<string, bool> VisibleColumns { get; set; } = new ConcurrentDictionary<string, bool>();

        /// <summary>
        /// Проверка видимости столбца
        /// </summary>
        /// <param name="columnAlias">Псевдоним столбца</param>
        /// <returns>Видимость столбца</returns>
        public bool CheckVisibleColumn(string columnAlias)
        {
            return VisibleColumns.Count == 0 || VisibleColumns.ContainsKey(columnAlias);
        }

        public static enSortNullPosition SortNullPosition { get; set; } = enSortNullPosition.None;
        public static readonly QueryParameters Empty = new QueryParameters();
    }
}
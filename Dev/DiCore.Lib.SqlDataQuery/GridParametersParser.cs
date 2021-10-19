using System.Collections.Generic;
using System.Collections.Specialized;
using DiCore.Lib.SqlDataQuery.QueryParameter;

namespace DiCore.Lib.SqlDataQuery
{
    public class GridParametersParser
    {
        /// <summary>
        /// Парсинг параметров Kendo из запроса
        /// </summary>
        /// <param name="request">Набор параметров запроса</param>
        public static QueryParameters Parse(NameValueCollection request)
        {
            if (request == null)
                return null;

            var result = new QueryParameters();
            
            if (request["visibleColumns"] != null)
            {
                var visibleColumns = request["visibleColumns"].Split(',');
                foreach (var visibleColumn in visibleColumns)
                    result.VisibleColumns.Add(visibleColumn, true);
            }

            if (request["page"] != null)
                result.Page = int.Parse(request["page"]);
            if (request["pageSize"] != null)
                result.PageSize = int.Parse(request["pageSize"]);
            if (request["skip"] != null)
                result.Skip = int.Parse(request["skip"]);
            if (request["take"] != null)
                result.Take = int.Parse(request["take"]);

            result.Sortings = ParseSortings(request);
            result.Filters = ParseFiltering(request);
            return result;
        }

        private static List<Sorting> ParseSortings(NameValueCollection request)
        {
            var x = 0;
            var result = new List<Sorting>();
            while (request[GetSortKey(x, "dir")] != null)
            {
                var direction = request[GetSortKey(x, "dir")];
                var column = request[GetSortKey(x, "field")];
                if (column != null && direction != null)
                {
                    result.Add(new Sorting {Column = column, Direction = direction});
                }
                x++;
            }
            return result;
        }

        private static string GetSortKey(int index, string name)
        {
            return $"sort[{index}][{name}]";
        }

        private static List<ColumnFilters> ParseFiltering(NameValueCollection request)
        {
            var result = new List<ColumnFilters>();
            var x = 0;
            while (CheckColumnFilterPresence(request, x))
            {
                if (CheckSimpleFilterPresence(request, x))
                {
                    var columnFilter = new ColumnFilters();
                    columnFilter.Logic = request[GetColumnLogicKey()];
                    do
                    {
                        columnFilter.AddFilter(GetFilter(request, x));
                        x++;
                    } while (CheckSimpleFilterPresence(request, x));
                    result.Add(columnFilter);
                }
                else
                {
                    var y = 0;
                    var columnFilter = new ColumnFilters();
                    columnFilter.Logic = request[GetComplexFilterLogicKey(x)];
                    while (CheckComplexFilterPresence(request, x, y))
                    {
                        columnFilter.AddFilter(GetComplexFilter(request, x, y));
                        y++;
                    }
                    result.Add(columnFilter);
                    x++;
                }
            }

            return result;
        }

        private static Filter GetComplexFilter(NameValueCollection request, int colIndex, int filterIndex)
        {
            var field = request[GetComplexFilterKey(colIndex, filterIndex, "field")];
            var op = request[GetComplexFilterKey(colIndex, filterIndex, "operator")];
            var value = request[GetComplexFilterKey(colIndex, filterIndex, "value")];
            return new Filter { Field = field, Operator = EnFilterOperationExtensions.FromString(op), Value = value };
        }

        private static Filter GetFilter(NameValueCollection request, int filterIndex)
        {
            var field = request[GetFilterKey(filterIndex, "field")];
            var op = request[GetFilterKey(filterIndex, "operator")];
            var value = request[GetFilterKey(filterIndex, "value")];
            return new Filter { Field = field, Operator = EnFilterOperationExtensions.FromString(op), Value = value };
        }

        private static string GetComplexFilterKey(int colIndex, int filterIndex, string filterName)
        {
            return $"filter[filters][{colIndex}][filters][{filterIndex}][{filterName}]";
        }

        private static string GetFilterKey(int colIndex, string filterName)
        {
            return $"filter[filters][{colIndex}][{filterName}]";
        }

        private static string GetComplexFilterLogicKey(int filterIndex)
        {
            return $"filter[filters][{filterIndex}][logic]";
        }

        private static string GetColumnLogicKey()
        {
            return $"filter[logic]";
        }

        private static bool CheckColumnFilterPresence(NameValueCollection request, int index)
        {
            return CheckSimpleFilterPresence(request, index) || CheckComplexColumnFilterPresence(request, index);
        }

        private static bool CheckSimpleFilterPresence(NameValueCollection request, int index)
        {
            return request[$"filter[filters][{index}][field]"] != null;
        }

        private static bool CheckComplexColumnFilterPresence(NameValueCollection request, int index)
        {
            return request[$"filter[filters][{index}][filters][0][field]"] != null;
        }

        private static bool CheckComplexFilterPresence(NameValueCollection request, int index0, int index1)
        {
            return request[$"filter[filters][{index0}][filters][{index1}][field]"] != null;
        }
    }
}

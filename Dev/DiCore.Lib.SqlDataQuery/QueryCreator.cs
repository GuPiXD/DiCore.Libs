using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DiCore.Lib.SqlDataQuery.QueryParameter;
using DiCore.Lib.SqlDataQuery.SqlCode;
using DiCore.Lib.SqlDataQuery.Utils;

namespace DiCore.Lib.SqlDataQuery
{
    /// <summary>
    /// Запрос к БД
    /// </summary>
    public class QueryCreator
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="queryDescription">Описание запроса к БД</param>
        /// <param name="columnsVisible"></param>
        public QueryCreator(QueryDescription queryDescription, ColumnsVisible columnsVisible = null)
        {
            QueryDescription = queryDescription;
            if (columnsVisible != null)
            {
                this.ColumnsVisible = columnsVisible;
            }
        }

        /// <summary>
        /// Описание запроса к БД
        /// </summary>
        public QueryDescription QueryDescription { get; private set; }

        /// <summary>
        /// Конфигурация столбцов (ограничение выдаваемых на выход столбцов и их видимость)
        /// </summary>
        public ColumnsVisible ColumnsVisible { get; private set; }

        /// <summary>
        /// Генерация текста запроса к БД
        /// </summary>
        /// <param name="requestParameters">Параметры запроса</param>
        /// <returns></returns>
        public string Create(QueryParameters requestParameters)
        {
#if DEBUG
            var stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
            var result = new StringBuilder();

            var where = CreateWhere(requestParameters.Filters, QueryDescription.Conditions);
            var orderBy = CreateOrderBy(requestParameters);
            var paging = CreatePaging(requestParameters);

            result.AppendLine("SELECT");
            AppendColumnList(requestParameters, result);
            result.AppendLine("FROM");
            AppendFromTables(requestParameters, result);
            if (!String.IsNullOrEmpty(where))
                result.AppendLine($"WHERE\r\n{where}");
            if (!String.IsNullOrEmpty(orderBy))
                result.AppendLine($"ORDER BY\r\n\t{orderBy}");

            if (paging.Item1 != -1)
            {
                result.Append($"OFFSET {paging.Item1} ");                
            }

            if (paging.Item2 != -1)
            {
                result.Append($"LIMIT {paging.Item2}");                
            }

            result.Append(";");
#if DEBUG
            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            Debug.WriteLine(ts.TotalMilliseconds, "SqlDataQuery QueryCreator.Create (milliseconds): ");
#endif

            return result.ToString();
        }

        private Tuple<int, int> CreatePaging(QueryParameters requestParameters)
        {
            if (requestParameters.Page.HasValue && requestParameters.PageSize.HasValue)
            {
                return new Tuple<int,int>((requestParameters.Page.Value - 1) * requestParameters.PageSize.Value, requestParameters.PageSize.Value);
            }

            if (requestParameters.Skip.HasValue || requestParameters.Take.HasValue)
            {
                return new Tuple<int,int>(requestParameters.Skip ?? -1, requestParameters.Take ?? -1);
            }

            return new Tuple<int, int>(-1, -1);
        }

        private void AppendColumnList(QueryParameters requestParameters, StringBuilder sb)
        {
            var columns = QueryDescription.Tables.Values.SelectMany(table => table.Columns.Values,
                    (table, column) => new Tuple<Table, BaseColumn>(table, column))
                .Where(x => IncludeColumnToSelect(x.Item2, requestParameters)).ToArray();

            for (var i = 0; i < columns.Length; i++)
            {
                var last = i == columns.Length - 1;
                var table = columns[i].Item1;
                var column = columns[i].Item2;

                switch (column)
                {
                    case Column col:
                        sb.AppendLine($"\t{table.Alias.Quotes()}.{col.Name.Quotes()} AS {col.Alias.Quotes()}{(last ? String.Empty : ",")}");
                        break;

                    case CustomColumn customCol:
                        sb.AppendLine(
                            $"\t{customCol.Code} AS {customCol.Alias.Quotes()}{(last ? String.Empty : ",")}");
                        break;
                }
            }            
        }

        private bool IncludeColumnToSelect(BaseColumn column, QueryParameters requestParameters)
        {
            if (ColumnsVisible != null && (
                !ColumnsVisible.GetConfiguration(column.Alias).Present || !ColumnsVisible.GetConfiguration(column.Alias).Visible))
                return false;

            switch (column.SelectType)
            {
                case EnSelectType.InSelectRequired:
                    return true;

                case EnSelectType.Inner:
                    return false;
            }

            return column is CustomColumn || requestParameters.CheckVisibleColumn(column.Alias);
        }

        private void AppendFromTables(QueryParameters requestParameters, StringBuilder sb)
        {
            if (QueryDescription.MainTable == null)
                throw new Exception("Не задана основная таблица запроса.");

            var mainTable = QueryDescription.MainTable;
            sb.AppendLine($"\t{mainTable.Schema.Quotes()}.{mainTable.Name.Quotes()} AS {mainTable.Alias.Quotes()}");

            var actualJoins = FilterJoins(QueryDescription.Joins, requestParameters);
            foreach (var actualJoin in actualJoins)
            {
                var table1 = QueryDescription.GetTable(actualJoin.Table1Alias);
                if (table1 == null)
                    throw new Exception($"Таблица {actualJoin.Table1Alias}, использованная в объединении, не найдена");
                var column1 = QueryDescription.GetColumn(actualJoin.Table1Alias, actualJoin.Column1Alias) as Column;

                var table2 = QueryDescription.GetTable(actualJoin.Table2Alias);
                if (table2 == null)
                    throw new Exception($"Таблица {actualJoin.Table2Alias}, использованная в объединении, не найдена");
                var column2 = QueryDescription.GetColumn(actualJoin.Table2Alias, actualJoin.Column2Alias) as Column;

                if (column1 == null)
                    throw new Exception($"Использованный в объединении столбец {actualJoin.Column1Alias} отсутствует в таблице {table1.Alias}");
                if (column2 == null)
                    throw new Exception($"Использованный в объединении столбец {actualJoin.Column2Alias} отсутствует в таблице {table2.Alias}");

                sb.AppendLine($"\t{actualJoin.Type.ToSql()} {table2.Schema.Quotes()}.{table2.Name.Quotes()} AS {table2.Alias.Quotes()}\r\n\t\tON " +
                            $"{table2.Alias.Quotes()}.{column2.Name.Quotes()} = {table1.Alias.Quotes()}.{column1.Name.Quotes()}");
            }
        }

        private IList<Join> FilterJoins(IList<Join> joins, QueryParameters requestParameters)
        {
            if (QueryDescription.TableTree != null) return joins.ToList();

            return joins.Where(x => x.Table1Alias == QueryDescription.MainTable.Alias)
                .Union(joins.Where(x => x.Table1Alias != QueryDescription.MainTable.Alias)).ToList();
        }

        private Dictionary<string, string> GetJoinTables(QueryParameters requestParameters)
        {
            var result = (from table in QueryDescription.Tables.Values
                where CheckJoinTable(table, requestParameters)
                select table.Alias).ToDictionary(e => e);

            foreach (var table in result.Values)
            {
                var path = QueryDescription.GetTablePath(table);
                foreach (var node in path)
                {
                    if (!path.Contains(node))
                    {
                        result.Add(node, node);
                    }
                }
            }

            return result;
        }

        private bool CheckJoinTable(Table table, QueryParameters requestParameters)
        {
            return table.Columns.Values.Any(column => IncludeColumnToSelect(column, requestParameters));
        }


        private string CreateOrderBy(QueryParameters requestParameters)
        {
            var sortings = requestParameters.Sortings;

            if (sortings.Count == 0)
                return null;

            var result = new StringBuilder();
            var first = true;
            foreach (var sortingInfo in sortings)
            {
                if (!first)
                    result.Append(", ");
                var baseColumn = QueryDescription.GetColumn(sortingInfo.Column);
                if (baseColumn == null)
                    throw new Exception($"Использованный в OrderBy столбец {sortingInfo.Column} отсутствует в таблицах");


                var columnName = String.Empty;

                if (baseColumn is Column column)
                {
                    columnName = $"{column.TableAlias.Quotes()}.{column.Name.Quotes()}";
                }
                else
                {
                    var customColumn = baseColumn as CustomColumn;
                    columnName = customColumn.Code;
                }

                var nullPosition = String.Empty;
                if (sortingInfo.NullPosition != enSortNullPosition.None)
                {
                    nullPosition = sortingInfo.NullPosition.ToSql();
                }
                else if (QueryParameters.SortNullPosition != enSortNullPosition.None)
                {
                    nullPosition = QueryParameters.SortNullPosition.ToSql();
                }

                var direction = String.IsNullOrEmpty(sortingInfo.Direction) ? String.Empty : $" {sortingInfo.Direction}";

                result.Append($"{columnName}{direction}{nullPosition}");

                first = false;
            }
            return result.ToString();
        }

        private string CreateWhere(IList<ColumnFilters> filters, IList<BaseCondition> conditions)
        {
            var result = new StringBuilder();
            if (filters.Count > 0)
            {
                for (int i = 0; i < filters.Count; i++)
                {
                    var single = filters.Count == 1 || filters[i].Filters?.Count == 1;
                    var last = i == filters.Count - 1;
                    var condition = CreateFilterCondition(filters[i]);

                    result.AppendLine($"{(single ? String.Empty : "(")}{condition}{(single ? String.Empty : ")")}{(last ? String.Empty : " AND ")}");
                }
            }

            if (conditions.Count == 0) return result.ToString();

            result.InBrackets();
            AddConditions(result, conditions);

            return result.ToString();
        }

        private void AddConditions(StringBuilder sb, IList<BaseCondition> conditions)
        {
            foreach (var condition in conditions)
            {
                if (sb.Length > 0 && sb[sb.Length - 1] != ' ')
                    sb.Append(" ");
                sb.Append(condition.ToString(sb.Length == 0));
            }
        }


        private string CreateFilterCondition(ColumnFilters filters)
        {
            var res = new StringBuilder();

            for (var i = 0; i < filters.Filters.Count; i++)
            {
                var first = i == 0;
                var item = filters.Filters[i];

                switch (item)
                {
                    case CustomFilter customFilter:
                        res.Append(
                            first ? customFilter.Clause.Trim() : $" {filters.Logic} {customFilter.Clause.Trim()}");

                        break;

                    case Filter filter:
                        var isNoValueFilter = filter.Operator.IsNoValueFilter();
                        if (!isNoValueFilter && String.IsNullOrEmpty(filter.Value) && !filter.Operator.IsEmptyValueFilter())
                            continue;

                        var column = GetMappingColumn(filter);
                        if (column == null)
                            continue;

                        var op = filter.Operator.ToSql();

                        var value = filter.Value;
                        value = column.Type.ConvertFilterValue(value);
                        value = filter.Operator.ConvertFilterValue(value);

                        if (!column.Type.IsNumber())
                            value = value.WrapUp('\'');

                        string columnInfo;
                        switch (column)
                        {
                            case Column col:
                                columnInfo = $"{column.TableAlias.Quotes()}.{col.Name.Quotes()}";
                                break;

                            case CustomColumn customColumn:
                                columnInfo = customColumn.Code;
                                break;

                            default:
                                throw new NotSupportedException();
                        }

                        res.AppendLine(
                            $"\t\t{(first ? String.Empty : filters.Logic.WrapUp(" "))}{columnInfo} {op}{(isNoValueFilter ? String.Empty : value.JointLeft(" "))}");
                        break;
                }                
            }

            return res.ToString();
        }                

        private BaseColumn GetMappingColumn(Filter filter)
        {
            var columnAlias = filter.Field;
            var column = QueryDescription.GetColumn(columnAlias) as BaseColumn;
            if (column == null)
            {
                //Для сложных вычисляемых фильтров, которые приходят в виде (col1 + col2 < value)
                return new CustomColumn($"filter#{filter.GetHashCode()}", filter.Field);
            }
            if (!QueryDescription.FilterColumnMappings.ContainsKey(column.Alias))
                return column;

            var filterColumn = QueryDescription.FilterColumnMappings[column.Alias];
            return QueryDescription.GetColumn(filterColumn.FilterColumnAlias) as Column;
        }
    }
}

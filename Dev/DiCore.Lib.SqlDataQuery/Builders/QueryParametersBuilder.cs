using System;
using System.Linq.Expressions;
using DiCore.Lib.SqlDataQuery.QueryParameter;

namespace DiCore.Lib.SqlDataQuery.Builders
{
    public static class QueryParametersBuilder
    {
        public static QueryParametersConfig<T> Create<T>()
        {
            return new QueryParametersConfig<T>();
        }
        
        public static QueryParametersConfig<T> Where<T>(this QueryParametersConfig<T> config, Expression<Func<T, bool>> predicate)
        {                                   
            var ft = new FiltersTranslator<T>();
            var filters = ft.Translate(predicate);
            
            config.Parameters.Filters.AddRange(filters);
            return config;
        }

        public static QueryParametersConfig<T> OrderBy<T, TValue>(this QueryParametersConfig<T> config,
            Expression<Func<T, TValue>> order)
        {
            var columnName = order.GetColumnName();
            config.Parameters.Sortings.Add(new Sorting(){Column = columnName});
            return config;
        }

        public static QueryParametersConfig<T> OrderByDescending<T, TValue>(this QueryParametersConfig<T> config,
            Expression<Func<T, TValue>> order)
        {
            var columnName = order.GetColumnName();
            config.Parameters.Sortings.Add(new Sorting(){Column = columnName, Direction = "DESC"});
            return config;
        }

        /// <summary>
        /// Выполняется формирование секции LIMIT {take}
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="config"></param>
        /// <param name="take">Значение LIMIT</param>
        /// <returns></returns>
        public static QueryParametersConfig<T> Take<T>(this QueryParametersConfig<T> config, int take)
        {
            config.Parameters.Take = take;           
            return config;
        }

        /// <summary>
        /// Выполняется формирование секции OFFSET {skip}
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="config"></param>
        /// <param name="skip">Значение OFFSET</param>
        /// <returns></returns>
        public static QueryParametersConfig<T> Skip<T>(this QueryParametersConfig<T> config, int skip)
        {
            config.Parameters.Skip = skip;            
            return config;
        }

        /// <summary>
        /// Paging на сервере.
        /// Выполняется формирование OFFSET {page*pageSize} и LIMIT {pageSize}
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="config"></param>
        /// <param name="page">Номер страницы с 0</param>
        /// <param name="pageSize">Размер страницы</param>
        /// <returns></returns>
        public static QueryParametersConfig<T> Page<T>(this QueryParametersConfig<T> config, int page, int pageSize)
        {
            config.Parameters.Page = page + 1;
            config.Parameters.PageSize = pageSize;
            return config;
        }

        public static QueryParameters Build<T>(this QueryParametersConfig<T> config)
        {
            return config.Parameters;
        }
    }
}

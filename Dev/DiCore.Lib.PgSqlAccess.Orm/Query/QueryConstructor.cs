using DiCore.Lib.SqlDataQuery;
using DiCore.Lib.SqlDataQuery.Builders;
using DiCore.Lib.SqlDataQuery.SqlCode;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace DiCore.Lib.PgSqlAccess.Orm.Query
{
    public class QueryConstructor<T>
    {
        private readonly IDataAdapter dataAdapter;
        private QueryCreatorConfig<T> queryCreatorConfig;
        private QueryCreator queryCreator;

        internal QueryConstructor(IDataAdapter dataAdapter, string mainTableName)
        {
            this.dataAdapter = dataAdapter;
            queryCreatorConfig = QueryCreatorBuilder.Create<T>(dataAdapter.Schema, mainTableName: mainTableName);
        }

        public QueryConstructor<T> MapEntity()
        {
            queryCreatorConfig = queryCreatorConfig.MapEntity();
            return this;
        }

        public QueryConstructor<T> Map()
        {
            queryCreatorConfig = queryCreatorConfig.Map();
            return this;
        }

        public QueryConstructor<T> Map<TValue>(Expression<Func<T, TValue>> mapField, string name = "", EnSelectType selectType = EnSelectType.InSelect, bool distinct = false)
        {
            queryCreatorConfig = queryCreatorConfig.Map(mapField, name: name, selectType: selectType, distinct: distinct);
            return this;
        }


        public QueryConstructor<T> MapJoin<TValue>(Expression<Func<T, TValue>> joinField, Expression<Func<T, TValue>> mainField = null, EnJoinType joinType = EnJoinType.Inner)
        {
            queryCreatorConfig = queryCreatorConfig.MapJoin(joinField, mainField: mainField, joinType: joinType);
            return this;
        }

        public QueryConstructor<T> MapJson<TValue>(Expression<Func<T, TValue>> mapField)
        {
            dataAdapter.TryAddJsonTypeMapper<TValue>();
            queryCreatorConfig = queryCreatorConfig.MapJson(mapField);
            return this;
        }

        internal QueryCreator BuildQueryCreator()
        {
            if (queryCreator == null)
                queryCreator = queryCreatorConfig.Build();
            return queryCreator;
        }
    }
}

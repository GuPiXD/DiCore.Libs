using System;
using System.Collections.Generic;
using System.Text;

namespace DiCore.Lib.PgSqlAccess.Orm.Query
{
    public class QueryConstructorFactory<T>
    {
        private readonly IDataAdapter dataAdapter;

        internal QueryConstructorFactory(IDataAdapter dataAdapter)
        {
            this.dataAdapter = dataAdapter;
        }

        public QueryConstructor<T> Create(string mainTableName = "")
        {
            return new QueryConstructor<T>(dataAdapter, mainTableName);
        }
    }
}

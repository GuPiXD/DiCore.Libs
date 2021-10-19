using System;
using System.Collections.Generic;
using DiCore.Lib.SqlDataQuery.QueryParameter;

namespace DiCore.Lib.SqlDataQuery.Builders
{
    public class QueryParametersConfig<T>
    {
        public QueryParameters Parameters { get; } = new QueryParameters();
    }
}

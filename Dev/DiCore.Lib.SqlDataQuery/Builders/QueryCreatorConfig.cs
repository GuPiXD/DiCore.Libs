using System;
using System.Collections.Generic;
using System.Xml;
using DiCore.Lib.SqlDataQuery.SqlCode;

namespace DiCore.Lib.SqlDataQuery.Builders
{
    public sealed class QueryCreatorConfig<T>
    {
        public QueryDescription QueryDescription { get; } = new QueryDescription();
        public string Schema { get; }

        public IDictionary<Type, TypeDescription> TypeDescriptions { get; } =
            new Dictionary<Type, TypeDescription>();

        public QueryCreatorConfig(string schema)
        {
            Schema = schema;
        }
    }
}
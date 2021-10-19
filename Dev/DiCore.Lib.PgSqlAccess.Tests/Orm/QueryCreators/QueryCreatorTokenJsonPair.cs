using DiCore.Lib.PgSqlAccess.Orm.Query;
using DiCore.Lib.SqlDataQuery;
using DiCore.Lib.SqlDataQuery.Builders;
using DiCore.Lib.TestModels.Models.Generic;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiCore.Lib.PgSqlAccess.Tests.Orm.QueryCreators
{
    public class QueryCreatorTokenJsonPair<TJson1, TJson2> : IQueryContructorConfigurator<Token<TJson1, TJson2>>
    {
        public QueryConstructor<Token<TJson1, TJson2>> ConfigureQueryConstructor(QueryConstructorFactory<Token<TJson1, TJson2>> constructor)
        {
            return constructor
                .Create("Token")
                .MapJson(x => x.DataProcessEvent.Parameters)
                .Map(x => x.Id)
                .Map(x => x.DataProcessEvent)
                .MapJson(x => x.Data)
                .MapJoin(x => x.DataProcessEvent.Id)
                .MapJoin(x => x.DataProcessEvent.EventType.Id)
                ;
        }
    }
}

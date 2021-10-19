using DiCore.Lib.PgSqlAccess.Orm.Query;
using DiCore.Lib.SqlDataQuery;
using DiCore.Lib.SqlDataQuery.Builders;
using DiCore.Lib.TestModels.Models.Special;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiCore.Lib.PgSqlAccess.Tests.Orm.QueryCreators
{
    public class QueryCreatorTokenSpecial : IQueryContructorConfigurator<Token>
    {
        public QueryConstructor<Token> ConfigureQueryConstructor(QueryConstructorFactory<Token> constructor)
        {
            return constructor
                .Create("Token")
                .MapJson(x => x.DataProcessEvent.Parameters)
                .MapJson(x => x.Data)
                .Map(x => x.Id)
                .Map(x => x.DataProcessEvent)                
                .MapJoin(x => x.DataProcessEvent.Id)
                .MapJoin(x => x.DataProcessEvent.EventType.Id)
                ;
        }
    }
}

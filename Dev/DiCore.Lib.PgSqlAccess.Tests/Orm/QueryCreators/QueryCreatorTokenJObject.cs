using DiCore.Lib.PgSqlAccess.Orm.Query;
using DiCore.Lib.SqlDataQuery;
using DiCore.Lib.SqlDataQuery.Builders;
using DiCore.Lib.TestModels.Models.JObject;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiCore.Lib.PgSqlAccess.Tests.Orm.QueryCreators
{
    public class QueryCreatorTokenJObject: IQueryContructorConfigurator<Token>
    {
        public QueryConstructor<Token> ConfigureQueryConstructor(QueryConstructorFactory<Token> constructor)
        {
            return constructor
                .Create()
                .MapJson(x => x.Data)
                .MapJson(x => x.DataProcessEvent.Parameters)
                .Map(x => x.Id)
                .Map(x => x.DataProcessEvent)
                .MapJoin(x => x.DataProcessEvent.Id)
                .MapJoin(x => x.DataProcessEvent.EventType.Id)
                ;
        }
    }
}

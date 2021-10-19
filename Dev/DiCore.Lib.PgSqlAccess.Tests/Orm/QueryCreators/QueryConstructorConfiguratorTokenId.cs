using System;
using System.Collections.Generic;
using System.Text;
using DiCore.Lib.PgSqlAccess.Orm.Query;
using DiCore.Lib.TestModels.Models.Cut;
using DiCore.Lib.TestModels.Models.Simple;

namespace DiCore.Lib.PgSqlAccess.Tests.Orm.QueryCreators
{
    public class QueryConstructorConfiguratorTokenId : IQueryContructorConfigurator<TokenId>
    {
        public QueryConstructor<TokenId> ConfigureQueryConstructor(QueryConstructorFactory<TokenId> constructor)
        {
            return constructor
                .Create(nameof(Token))
                .MapEntity();
        }
    }
}

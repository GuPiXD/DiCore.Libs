using System;
using System.Collections.Generic;
using System.Text;
using DiCore.Lib.PgSqlAccess.Orm.Query;
using DiCore.Lib.SqlDataQuery;
using DiCore.Lib.SqlDataQuery.Builders;
using DiCore.Lib.TestModels.Models.Simple;

namespace DiCore.Lib.PgSqlAccess.Tests.Orm.QueryCreators
{
    public class QueryCreatorArtifactSimple : IQueryContructorConfigurator<Artifact>
    {
        public QueryConstructor<Artifact> ConfigureQueryConstructor(QueryConstructorFactory<Artifact> constructor)
        {
            return constructor
                .Create()
                .Map(x => x)
                .MapJoin(x => x.ArtifactAddonMeasured.Id, x => x.Id)
                .MapJoin(x => x.ArtifactAddonData.Id, x => x.Id)
                .MapJoin(x => x.DiagnosticMethod.Id)
                .MapJoin(x => x.ArtifactAddonMeasured.SurfaceLocation.Id)
                ;
        }
    }
}

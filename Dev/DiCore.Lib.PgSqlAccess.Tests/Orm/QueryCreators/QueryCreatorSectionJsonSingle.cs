using System;
using System.Collections.Generic;
using System.Text;
using DiCore.Lib.PgSqlAccess.Orm.Query;
using DiCore.Lib.SqlDataQuery;
using DiCore.Lib.SqlDataQuery.Builders;
using DiCore.Lib.TestModels.Models.Generic;

namespace DiCore.Lib.PgSqlAccess.Tests.Orm.QueryCreators
{
    public class QueryCreatorSectionJsonSingle<TJson> : IQueryContructorConfigurator<Section<TJson>>
    {
        public QueryConstructor<Section<TJson>> ConfigureQueryConstructor(QueryConstructorFactory<Section<TJson>> constructor)
        {
            return constructor
                .Create("Section")
                .Map(x => x.Id)
                .Map(x => x.TokenId)
                .Map(x => x.Distance)
                .Map(x => x.Number)
                .Map(x => x.SectionType)
                .Map(x => x.AverageWallThickness)
                .Map(x => x.AxialWeldStartAngle)
                .Map(x => x.AxialWeldEndAngle)
                .Map(x => x.PipelineSectionId)
                .Map(x => x.PipeType)
                .Map(x => x.Altitude)
                .Map(x => x.AxialWeldSecondAngle)
                .Map(x => x.vrDeleted)
                .Map(x => x.AxialWeldSecondAngle)
                .MapJoin(x => x.SectionType.Id)
                .MapJoin(x => x.PipeType.Id)
                .MapJson(x => x.Data)
                ;
        }
    }
}

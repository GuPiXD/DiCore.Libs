using DiCore.Lib.PgSqlAccess.Types.Attributes;
using DiCore.Lib.PgSqlAccess.Types.Enum;

namespace DiCore.Lib.PgSqlAccess.Tests.Old.Input
{
    public class TestJsonbInsertInput
    {
        [NpgsqlDbType(EnNpgsqlDbType.Jsonb)]
        public string Value { get; set; }
    }
}

using DiCore.Lib.PgSqlAccess.Types.Attributes;
using DiCore.Lib.PgSqlAccess.Types.Enum;

namespace DiCore.Lib.PgSqlAccess.Tests.Old.Output
{
    public class TestJsonbOutput
    {
        public int Id { get; set; }

        [NpgsqlDbType(EnNpgsqlDbType.Jsonb)]
        public string Data { get; set; }
    }
}

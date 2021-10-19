using System;

namespace DiCore.Lib.PgSqlAccess.Tests.Old.Input
{
    public class TestPassportInsertInput
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int PigTypeId { get; set; }

        public string AssemblyParameters { get; set; }
    }
}

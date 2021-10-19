using System;
using DiCore.Lib.PgSqlAccess.DataAccessObjects;
using DiCore.Lib.PgSqlAccess.Test.SharedModel;
using NUnit.Framework;

namespace DiCore.Lib.PgSqlAccess.Tests.Old
{
    [TestFixture]
    public class TestStoredProcedureInputOutput
    {
        [Test]
        public void TestRunOne()
        {
            void Action()
            {
                var sp = new StoredProcedure<IntModel, IntModel>(Shared.Connection, "public", "TableIntInsert");
                sp.RunOne(new IntModel() {Value = 5}, out var result);
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Value);
            }

            ExecuteActions.SafeExecutor(Action);
        }
    }
}

using System;
using DiCore.Lib.PgSqlAccess.DataAccessObjects;
using DiCore.Lib.PgSqlAccess.Test.SharedModel;
using NUnit.Framework;

namespace DiCore.Lib.PgSqlAccess.Tests.Old
{
    [TestFixture]
    public class StoredProcedureInputTest
    {
        [Test]
        public void TestStoredProcedure()
        {
            void Action()
            {
                IntModel result;
                var sp = new StoredProcedure<IntModel, IntModel>(Shared.Connection, "public", "TableIntInsert");
                sp.RunOne(new IntModel() {Value = 5}, out result);
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Value);
            }

            ExecuteActions.SafeExecutor(Action);
        }

        [Test]
        public void TestStoredProcedureSync()
        {
            void Action()
            {
                var sp = new StoredProcedureInput<IntModel>(Shared.Connection, "public", "TableIntInsertWithoutRet");
                sp.Run(new IntModel() { Value = 5 });
            }

            ExecuteActions.SafeExecutor(Action);

            Assert.Pass();
        }

        [Test]
        public void TestStoredProcedure1()
        {
            void Action()
            {
                IntModel result;
                var sp = new PgStoredProcedure(Shared.Connection, "public", "TableIntInsert");
                sp.ExecuteScalar<int>(new {Value = (int?) 7});
            }

            ExecuteActions.SafeExecutor(Action);
        }
        
    }
}

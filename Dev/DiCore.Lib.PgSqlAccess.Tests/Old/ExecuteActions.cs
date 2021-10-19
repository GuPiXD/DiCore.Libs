using System;
using NUnit.Framework;

namespace DiCore.Lib.PgSqlAccess.Tests.Old
{
    public static class ExecuteActions
    {
        public static void SafeExecutor(Action action)
        {
            SafeExecutor(() => { action(); return 0; });
        }

        public static T SafeExecutor<T>(Func<T> action)
        {
            Shared.Connection.Open();
            try
            {
                return action();
            }
            catch (Exception e)
            {
                Assert.Fail($"Ошибка: {e.Message} {e.Data}");
            }
            finally
            {
                Shared.Connection.Close();
            }

            return default(T);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DiCore.Lib.PgSqlAccess.Orm.Change
{
    public interface IPairChange<TFirst, TSecond> : IChange
    {
        IPairChange<TFirst, TSecond> ChangeBeforeExecute<TValue>(object spObject, Action<TFirst, TSecond, TValue> action, string spName = null);
        IPairChange<TFirst, TSecond> ChangeBeforeExecuteJson<TValue, TJson>(object spObject, Action<TFirst, TSecond, TValue> action, string spName = null);
        Task<ICollection<TOut>> ExecuteScalarAsync<TOut>(Action<TSecond, TOut> actionAfterFirstExecute = null);
        Task<ICollection<(TOut1, TOut2)>> ExecuteScalarAsync<TOut1, TOut2>(Action<TSecond, TOut1> actionAfterFirstExecute = null);
        IPairChange<TFirst, TSecond> SetJsonFirst<TJson>(Expression<Func<TFirst, TJson>> jsonSelector);
        IPairChange<TFirst, TSecond> SetJsonSecond<TJson>(Expression<Func<TSecond, TJson>> jsonSelector);
        IPairChange<TFirst, TSecond> UseTransaction();
    }
}
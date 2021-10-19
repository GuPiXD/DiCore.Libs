using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DiCore.Lib.PgSqlAccess.Orm.Change
{
    public interface ISingleChange<T> : IChange
    {
        ISingleChange<T> ChangeBeforeExecute<TValue>(object spObject, System.Action<T, TValue> action, string spName = null);
        ISingleChange<T> ChangeBeforeExecuteJson<TValue, TJson>(object spObject, System.Action<T, TValue> action, string spName = null);
        Task<ICollection<TOut>> ExecuteScalarAsync<TOut>();
        ISingleChange<T> SetJson<TJson>(Expression<Func<T, TJson>> jsonSelector);
        ISingleChange<T> UseTransaction();
    }
}
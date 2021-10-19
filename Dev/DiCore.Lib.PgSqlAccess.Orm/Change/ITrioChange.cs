using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DiCore.Lib.PgSqlAccess.Orm.Change
{
    public interface ITrioChange<TFirst, TSecond, TThird>
    {
        ITrioChange<TFirst, TSecond, TThird> ChangeBeforeExecute<TValue>(object spObject, Action<TFirst, TSecond, TThird, TValue> action, string spName = null);
        ITrioChange<TFirst, TSecond, TThird> ChangeBeforeExecuteJson<TValue, TJson>(object spObject, Action<TFirst, TSecond, TThird, TValue> action, string spName = null);
        Task<ICollection<TOut>> ExecuteScalarAsync<TOut>(Action<TSecond, TThird, TOut> actionAfterFirstExecute = null);
        Task<ICollection<(TOut1, TOut2, TOut3)>> ExecuteScalarAsync<TOut1, TOut2, TOut3>(Action<TSecond, TThird, TOut1> actionAfterFirstExecute = null, Action<TThird, TOut2> actionAfterSecondExecute = null);
        Task<ICollection<(TOut1, TOut2)>> ExecuteScalarAsync<TOut1, TOut2>(Action<TSecond, TThird, TOut1> actionAfterFirstExecute = null, Action<TThird, TOut2> actionAfterSecondExecute = null);
        ITrioChange<TFirst, TSecond, TThird> SetJsonFirst<TJson>(Expression<Func<TFirst, TJson>> jsonSelector);
        ITrioChange<TFirst, TSecond, TThird> SetJsonSecond<TJson>(Expression<Func<TSecond, TJson>> jsonSelector);
        ITrioChange<TFirst, TSecond, TThird> SetJsonThird<TJson>(Expression<Func<TThird, TJson>> jsonSelector);
        ITrioChange<TFirst, TSecond, TThird> UseTransaction();
    }
}
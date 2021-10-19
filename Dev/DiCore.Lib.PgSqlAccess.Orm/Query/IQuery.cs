using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DiCore.Lib.PgSqlAccess.Orm.Query
{
    public interface IQuery<TQueryModel>
    {
        Task<IEnumerable<TQueryModel>> ExecuteAsync(object input = null, bool byNames = true);
        Task<IEnumerable<TResult>> ExecuteByTypeResultAsync<TResult>(object input = null, bool byNames = true);
        string GetSql();
        string GetSqlByTypeResult<TResult>();
        IQuery<TQueryModel> OrderBy<TVal>(Expression<Func<TQueryModel, TVal>> order);
        IQuery<TQueryModel> OrderByDescending<TVal>(Expression<Func<TQueryModel, TVal>> order);
        IQuery<TQueryModel> Page(int page, int pageSize);
        IQuery<TQueryModel> Skip(int skip);
        IQuery<TQueryModel> Take(int take);
        IQuery<TQueryModel> Where(Expression<Func<TQueryModel, bool>> predicate);
        IQuery<TQueryModel> In<P>(Expression<Func<TQueryModel, P>> property, IEnumerable<P> values);
        IQuery<TQueryModel> NotIn<P>(Expression<Func<TQueryModel, P>> property, IEnumerable<P> values);
    }
}

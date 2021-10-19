using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DiCore.Lib.PgSqlAccess.Orm.Change;
using DiCore.Lib.PgSqlAccess.Orm.Query;

namespace DiCore.Lib.PgSqlAccess.Orm
{
    public interface IDataAdapter
    {
        string Schema { get; set; }

        Task ChangeAsync(IEnumerable inputModels, string spName = null);
        Task ChangeAsync(object inputModel, string spName = null);
        Task ChangeJsonAsync<TJson>(IEnumerable inputModels, string spName = null);
        Task ChangeJsonAsync<TJson>(object inputModel, string spName = null);
        Task<ICollection<TOut>> ChangeJsonWithScalarAsync<TJson, TOut>(IEnumerable inputModels, string spName = null);
        Task<TOut> ChangeJsonWithScalarAsync<TJson, TOut>(object inputModel, string spName = null);
        [Obsolete]
        Task<ICollection<(TOut1, TOut2, TOut3)>> ChangeJsonWithScalarAsync<TJson1, TJson2, TJson3, TOut1, TOut2, TOut3>(IEnumerable<(object, object, object)> inputModels, string spName1 = null, string spName2 = null, string spName3 = null);
        [Obsolete]
        Task<ICollection<(TOut1, TOut2, TOut3)>> ChangeJsonWithScalarAsync<TJson1, TJson2, TOut1, TOut2, TOut3>(IEnumerable<(object, object, object)> inputModels, string spName1 = null, string spName2 = null, string spName3 = null);
        [Obsolete]
        Task<ICollection<(TOut1, TOut2)>> ChangeJsonWithScalarAsync<TJson1, TJson2, TOut1, TOut2>(IEnumerable<(object, object)> inputModels, string spName1 = null, string spName2 = null);
        [Obsolete]
        Task<ICollection<(TOut1, TOut2, TOut3)>> ChangeJsonWithScalarAsync<TJson1, TOut1, TOut2, TOut3>(IEnumerable<(object, object, object)> inputModels, string spName1 = null, string spName2 = null, string spName3 = null);
        [Obsolete]
        Task<ICollection<(TOut1, TOut2)>> ChangeJsonWithScalarAsync<TJson1, TOut1, TOut2>(IEnumerable<(object, object)> inputModels, string spName1 = null, string spName2 = null);
        Task<ICollection<TOut>> ChangeWithScalarAsync<TOut>(IEnumerable inputModels, string spName = null);
        Task<TOut> ChangeWithScalarAsync<TOut>(object inputModel, string spName = null);
        [Obsolete]
        Task<ICollection<(TOut1, TOut2, TOut3)>> ChangeWithScalarAsync<TOut1, TOut2, TOut3>(IEnumerable<(object, object, object)> inputModels, string spName1 = null, string spName2 = null, string spName3 = null);
        [Obsolete]
        Task<ICollection<(TOut1, TOut2)>> ChangeWithScalarAsync<TOut1, TOut2>(IEnumerable<(object, object)> inputModels, string spName1 = null, string spName2 = null);
        ISingleChange<T> CreateChange<T>(IEnumerable<T> inputs);
        ITrioChange<TFirst, TSecond, TThird> CreateChange<TFirst, TSecond, TThird>(IEnumerable<(TFirst, TSecond, TThird)> inputs);
        IPairChange<TFirst, TSecond> CreateChange<TFirst, TSecond>(IEnumerable<(TFirst, TSecond)> inputs);
        IQuery<TQueryModel> CreateQuery<TQueryModel>();
        Task<IEnumerable<TQueryModel>> ExecuteQueryAsync<TQueryModel>(IQuery<TQueryModel> query, object input = null, bool byNames = true);
        Task<IEnumerable<TResult>> ExecuteQueryByTypeResultAsync<TQuery, TResult>(IQuery<TQuery> query, object input = null, bool byNames = true);
        void TryAddJsonTypeMapper<TJson>();
    }
}
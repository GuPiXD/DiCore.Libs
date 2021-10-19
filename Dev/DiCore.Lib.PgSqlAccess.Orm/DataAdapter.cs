using DiCore.Lib.PgSqlAccess.DataAccessObjects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Npgsql;
using System.Collections.Concurrent;
using Newtonsoft.Json.Linq;
using DiCore.Lib.PgSqlAccess.Orm.Change;

namespace DiCore.Lib.PgSqlAccess.Orm
{
    public partial class DataAdapter : IDataAdapter
    {
        protected readonly IConnectionFactory connectionFactory;
        protected readonly Query.IQueryConstructorManager queryConstructorManager;
        private static readonly ConcurrentDictionary<Type, ITypeMapper> jsonTypes = new ConcurrentDictionary<Type, ITypeMapper>();

        public DataAdapter(IConnectionFactory connectionFactory, Query.IQueryConstructorManager queryConstructorManager)
        {
            this.connectionFactory = connectionFactory;
            this.queryConstructorManager = queryConstructorManager;
        }

        public string Schema { get; set; }

        public ISingleChange<T> CreateChange<T>(IEnumerable<T> inputs)
        {
            return new SingleChange<T>(inputs, this, connectionFactory);
        }

        public IPairChange<TFirst, TSecond> CreateChange<TFirst, TSecond>(IEnumerable<(TFirst, TSecond)> inputs)
        {
            return new PairChange<TFirst, TSecond>(inputs, this, connectionFactory);
        }

        public ITrioChange<TFirst, TSecond, TThird> CreateChange<TFirst, TSecond, TThird>(IEnumerable<(TFirst, TSecond, TThird)> inputs)
        {
            return new TrioChange<TFirst, TSecond, TThird>(inputs, this, connectionFactory);
        }

        public Query.IQuery<TQueryModel> CreateQuery<TQueryModel>()
        {
            return new Query.Query<TQueryModel>(this, queryConstructorManager);
        }

        public async Task<IEnumerable<TQueryModel>> ExecuteQueryAsync<TQueryModel>(
            Query.IQuery<TQueryModel> query, object input = null, bool byNames = true)
        {
            var sql = query.GetSql();
            return await InnerExecuteAsync<TQueryModel>(sql, input: input, byNames: byNames).ConfigureAwait(false);
        }

        public async Task<IEnumerable<TResult>> ExecuteQueryByTypeResultAsync<TQuery, TResult>(
            Query.IQuery<TQuery> query, object input = null, bool byNames = true)
        {
            var sql = query.GetSqlByTypeResult<TResult>();
            return await InnerExecuteAsync<TResult>(sql, input: input, byNames: byNames).ConfigureAwait(false);
        }

        private async Task<IEnumerable<TModel>> InnerExecuteAsync<TModel>(string sql, object input, bool byNames)
        {
            var connection = await connectionFactory.GetConnectionAsync().ConfigureAwait(false);
            var pgQuery = new PgQuery(connection, sql);
            var result = await pgQuery.ExecuteReaderCursorAsync<TModel>(input: input, byNames: byNames).ConfigureAwait(false);
            return new EnumerablePgQueryResult<TModel>(result, pgQuery, connection);
        }

        protected string GetStoredProcedureName(Type type)
        {
            var name = type.Name;
            var tableName = name.Contains("`") ? name.Split('`')[0] : name;
            return tableName;
        }

        public void TryAddJsonTypeMapper<TJson>()
        {
            var type = typeof(TJson);
            if (jsonTypes.ContainsKey(type)) return;

            if (type == typeof(JObject))
            {
                var jobjectMapper = new JObjectTypeMapper();
                SqlMapper.AddTypeMapper(jobjectMapper);
                jsonTypes.TryAdd(type, jobjectMapper);
                return;
            }

            var mapper = new JsonTypeMapper<TJson>();
            SqlMapper.AddTypeMapper(mapper);
            jsonTypes.TryAdd(type, mapper);
        }
    }
}

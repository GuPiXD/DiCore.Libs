using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DiCore.Lib.SqlDataQuery.Builders;
using DiCore.Lib.SqlDataQuery.QueryParameter;

namespace DiCore.Lib.PgSqlAccess.Orm.Query
{
    public class Query<TQueryModel> : IQuery<TQueryModel>
    {
        protected readonly IDataAdapter dataAdapter;
        protected readonly IQueryConstructorManager queryCreatorManager;
        protected QueryParametersConfig<TQueryModel> queryParametersConfig = QueryParametersBuilder.Create<TQueryModel>();
        protected QueryParameters queryParameters;

        internal Query(IDataAdapter dataAdapter, IQueryConstructorManager queryCreatorManager)
        {
            this.dataAdapter = dataAdapter;
            this.queryCreatorManager = queryCreatorManager;
        }

        public IQuery<TQueryModel> Where(Expression<Func<TQueryModel, bool>> predicate)
        {
            queryParametersConfig = queryParametersConfig.Where(predicate);
            return this;
        }

        public IQuery<TQueryModel> In<P>(Expression<Func<TQueryModel, P>> property, IEnumerable<P> values)
        {
            var predicate = GetInPredicate(property, values);
            queryParametersConfig = queryParametersConfig.Where(predicate);
            return this;
        }

        public IQuery<TQueryModel> NotIn<P>(Expression<Func<TQueryModel, P>> property, IEnumerable<P> values)
        {
            var predicate = GetNotInPredicate(property, values);
            queryParametersConfig = queryParametersConfig.Where(predicate);
            return this;
        }

        public IQuery<TQueryModel> OrderBy<TVal>(Expression<Func<TQueryModel, TVal>> order)
        {
            queryParametersConfig = queryParametersConfig.OrderBy(order);
            return this;
        }

        public IQuery<TQueryModel> OrderByDescending<TVal>(Expression<Func<TQueryModel, TVal>> order)
        {
            queryParametersConfig = queryParametersConfig.OrderByDescending(order);
            return this;
        }

        public IQuery<TQueryModel> Page(int page, int pageSize)
        {
            queryParametersConfig = queryParametersConfig.Page(page, pageSize);
            return this;
        }

        public IQuery<TQueryModel> Skip(int skip)
        {
            queryParametersConfig = queryParametersConfig.Skip(skip);
            return this;
        }

        public IQuery<TQueryModel> Take(int take)
        {
            queryParametersConfig = queryParametersConfig.Take(take);
            return this;
        }

        public async Task<IEnumerable<TQueryModel>> ExecuteAsync(object input = null, bool byNames = true)
        {
            TryBuildQueryParameters();

            return await dataAdapter
                .ExecuteQueryAsync<TQueryModel>(this, input: input, byNames: byNames)
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<TResult>> ExecuteByTypeResultAsync<TResult>(object input = null, bool byNames = true)
        {
            TryBuildQueryParameters();

            return await dataAdapter
                .ExecuteQueryByTypeResultAsync<TQueryModel,TResult>(this, input: input, byNames: byNames)
                .ConfigureAwait(false);
        }

        public virtual string GetSql()
        {
            TryBuildQueryParameters();

            var qcf = new QueryConstructorFactory<TQueryModel>(dataAdapter);

            var qc = queryCreatorManager
                .СonfigureQueryConstructor<TQueryModel>(qcf)
                .BuildQueryCreator();

            return qc.Create(queryParameters);
        }

        public virtual string GetSqlByTypeResult<TResult>()
        {
            TryBuildQueryParameters();

            var qcf = new QueryConstructorFactory<TResult>(dataAdapter);

            var qc = queryCreatorManager
                .СonfigureQueryConstructor<TResult>(qcf)
                .BuildQueryCreator();

            return qc.Create(queryParameters);
        }

        protected void TryBuildQueryParameters()
        {
            if (queryParameters == null)
                queryParameters = queryParametersConfig.Build();
        }

        private Expression<Func<TQueryModel, bool>> GetInPredicate<P>(Expression<Func<TQueryModel, P>> property, IEnumerable<P> values)
        {
            var body = BuildFilterIn(values.Select(x => GetEquilExpression(property.Body, x, ExpressionType.Equal))
                , Expression.OrElse);
            if (body != null)
                return Expression.Lambda<Func<TQueryModel, bool>>(body, property.Parameters);
            else
                return null;
        }

        private Expression<Func<TQueryModel, bool>> GetNotInPredicate<P>(Expression<Func<TQueryModel, P>> property, IEnumerable<P> values)
        {
            var body = BuildFilterIn(values.Select(x => GetEquilExpression(property.Body, x, ExpressionType.NotEqual))
                , Expression.AndAlso);
            if (body != null)
                return Expression.Lambda<Func<TQueryModel, bool>>(body, property.Parameters);
            else
                return null;
        }

        private BinaryExpression GetEquilExpression<T>(Expression body, T value, ExpressionType expressionType)
        {
            var type = typeof(T);
            if (Nullable.GetUnderlyingType(type) == null)
                return Expression.MakeBinary(expressionType, body, Expression.Constant(value));

            return Expression.MakeBinary(
                expressionType, 
                body, 
                Expression.Convert(Expression.Constant(value), type));
        }

        private static BinaryExpression BuildFilterIn(IEnumerable<BinaryExpression> binaryExpressions, Func<Expression, Expression, BinaryExpression> func)
        {
            BinaryExpression expression = null;
            foreach (var expr in binaryExpressions)
            {
                if (expression == null)
                {
                    expression = expr;
                    continue;
                }

                expression = func(expression, expr);
            }
            return expression;
        }
    }
}

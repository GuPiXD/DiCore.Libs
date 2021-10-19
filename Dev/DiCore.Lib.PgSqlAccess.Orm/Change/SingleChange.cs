using DiCore.Lib.PgSqlAccess.DataAccessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DiCore.Lib.PgSqlAccess.Orm.Change
{
    public class SingleChange<T> : Change, ISingleChange<T>
    {
        protected readonly IEnumerable<T> inputs;

        public SingleChange(
            IEnumerable<T> inputs,
            IDataAdapter dataAdapter,
            IConnectionFactory connectionFactory) : base(dataAdapter, connectionFactory)
        {
            this.inputs = inputs;
        }

        public ISingleChange<T> SetJson<TJson>(Expression<Func<T, TJson>> jsonSelector)
        {
            dataAdapter.TryAddJsonTypeMapper<TJson>();
            return this;
        }
        public ISingleChange<T> UseTransaction()
        {
            useTransaction = true;
            return this;
        }

        public virtual ISingleChange<T> ChangeBeforeExecute<TValue>(
            object spObject,
            Action<T, TValue> action,
            string spName = null)
        {
            var singleChangeExtented =
                new SingleChangeExtended<T, TValue>(this, inputs, dataAdapter, connectionFactory);
            singleChangeExtented.SetFields(spObject, action, spName);
            return singleChangeExtented;
        }

        public virtual ISingleChange<T> ChangeBeforeExecuteJson<TValue, TJson>(
            object spObject,
            Action<T, TValue> action,
            string spName = null)
        {
            dataAdapter.TryAddJsonTypeMapper<TJson>();
            return ChangeBeforeExecute(spObject, action, spName);
        }

        public async Task<ICollection<TOut>> ExecuteScalarAsync<TOut>()
        {

            await SetConnectionAsync().ConfigureAwait(false);
            try
            {
                var result = await InnerExecuteScalarSingleAsync<TOut>().ConfigureAwait(false);

                if (useTransaction)
                    await transaction.CommitAsync().ConfigureAwait(false);

                return result;
            }
            catch (Exception exception)
            {
                if (useTransaction)
                    await transaction.RollbackAsync().ConfigureAwait(false);
                throw exception;
            }
            finally
            {
                if (useTransaction)
                    transaction.Dispose();
                connection.Dispose();
            }
        }

        protected override async Task InnerExecuteNonQueryAsync()
        {
            foreach (var input in inputs)
                await BaseInnerExecuteNonQueryAsync(
                    new object[] { input }, 0).ConfigureAwait(false);
        }

        protected virtual async Task<ICollection<TOut>> InnerExecuteScalarSingleAsync<TOut>()
        {
            var result = new List<TOut>();

            foreach (var input in inputs)
                result.Add(await BaseInnerExecuteScalarAsync<TOut>(
                    input, spNames[0]).ConfigureAwait(false));

            return result;
        }

        protected override string[] GetSpNames()
        {
            return new[] { GetStoredProcedureName(typeof(T)) };
        }


    }
}

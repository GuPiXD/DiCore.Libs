using DiCore.Lib.PgSqlAccess.DataAccessObjects;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DiCore.Lib.PgSqlAccess.Orm.Change
{
    public class PairChange<TFirst, TSecond> : Change, IPairChange<TFirst, TSecond>
    {
        protected readonly IEnumerable<(TFirst, TSecond)> inputs;


        internal PairChange(
            IEnumerable<(TFirst, TSecond)> inputs,
            IDataAdapter dataAdapter,
            IConnectionFactory connectionFactory) : base(dataAdapter, connectionFactory)
        {
            this.inputs = inputs;
        }

        public IPairChange<TFirst, TSecond> SetJsonFirst<TJson>(Expression<Func<TFirst, TJson>> jsonSelector)
        {
            dataAdapter.TryAddJsonTypeMapper<TJson>();
            return this;
        }

        public IPairChange<TFirst, TSecond> SetJsonSecond<TJson>(Expression<Func<TSecond, TJson>> jsonSelector)
        {
            dataAdapter.TryAddJsonTypeMapper<TJson>();
            return this;
        }

        public IPairChange<TFirst, TSecond> UseTransaction()
        {
            useTransaction = true;
            return this;
        }

        public virtual IPairChange<TFirst, TSecond> ChangeBeforeExecute<TValue>(
            object spObject,
            Action<TFirst, TSecond, TValue> action,
            string spName = null)
        {
            var piarChangeExtented =
                new PairChangeExtended<TFirst, TSecond, TValue>(this, inputs, dataAdapter, connectionFactory);
            piarChangeExtented.SetFields(spObject, action, spName);
            return piarChangeExtented;
        }

        public virtual IPairChange<TFirst, TSecond> ChangeBeforeExecuteJson<TValue, TJson>(
            object spObject,
            Action<TFirst, TSecond, TValue> action,
            string spName = null)
        {
            dataAdapter.TryAddJsonTypeMapper<TJson>();
            return ChangeBeforeExecute(spObject, action, spName);
        }

        public async Task<ICollection<TOut>> ExecuteScalarAsync<TOut>(
            Action<TSecond, TOut> actionAfterFirstExecute = null)
        {
            await SetConnectionAsync().ConfigureAwait(false);
            try
            {
                var result = await InnerExecuteScalarSingleAsync(actionAfterFirstExecute)
                    .ConfigureAwait(false);

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

        public async Task<ICollection<(TOut1, TOut2)>> ExecuteScalarAsync<TOut1, TOut2>(
            Action<TSecond, TOut1> actionAfterFirstExecute = null)
        {
            await SetConnectionAsync().ConfigureAwait(false);
            try
            {
                var result = await InnerExecuteScalarPairAsync<TOut1, TOut2>(actionAfterFirstExecute).ConfigureAwait(false);

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

        protected override string[] GetSpNames()
        {
            return new[]
            {
                GetStoredProcedureName(typeof(TFirst)),
                GetStoredProcedureName(typeof(TSecond))
            };
        }

        protected override async Task InnerExecuteNonQueryAsync()
        {
            foreach (var (inputFirst, inputSecond) in inputs)
                await BaseInnerExecuteNonQueryAsync(
                    new object[] { inputFirst, inputSecond }, 0).ConfigureAwait(false);
        }

        protected virtual async Task<ICollection<TOut>> InnerExecuteScalarSingleAsync<TOut>(
            Action<TSecond, TOut> actionAfterFirstExecute)
        {
            var result = new List<TOut>();
            foreach (var (inputFirst, inputSecond) in inputs)
            {
                var scalar = await BaseInnerExecuteScalarAsync<TOut>(
                    inputFirst, spNames[0]).ConfigureAwait(false);

                actionAfterFirstExecute?.Invoke(inputSecond, scalar);

                await BaseInnerExecuteNonQueryAsync(
                    new object[] { inputFirst, inputSecond }, 1).ConfigureAwait(false);

                result.Add(scalar);
            }
            return result;
        }

        protected virtual async Task<ICollection<(TOut1, TOut2)>> InnerExecuteScalarPairAsync<TOut1, TOut2>(
            Action<TSecond, TOut1> actionAfterFirstExecute)
        {
            var result = new List<(TOut1, TOut2)>();
            foreach (var (inputFirst, inputSecond) in inputs)
            {
                var scalarFirst = await BaseInnerExecuteScalarAsync<TOut1>(
                    inputFirst, spNames[0]).ConfigureAwait(false);

                actionAfterFirstExecute?.Invoke(inputSecond, scalarFirst);

                var scalarSecond = await BaseInnerExecuteScalarAsync<TOut2>(
                    inputSecond, spNames[1]).ConfigureAwait(false);

                result.Add((scalarFirst, scalarSecond));
            }
            return result;
        }
    }
}

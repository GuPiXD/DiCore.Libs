using DiCore.Lib.PgSqlAccess.DataAccessObjects;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DiCore.Lib.PgSqlAccess.Orm.Change
{
    public class TrioChange<TFirst, TSecond, TThird> : Change, ITrioChange<TFirst, TSecond, TThird>
    {
        protected readonly IEnumerable<(TFirst, TSecond, TThird)> inputs;

        internal TrioChange(
            IEnumerable<(TFirst, TSecond, TThird)> inputs,
            IDataAdapter dataAdapter,
            IConnectionFactory connectionFactory) : base(dataAdapter, connectionFactory)
        {
            this.inputs = inputs;
        }

        public ITrioChange<TFirst, TSecond, TThird> SetJsonFirst<TJson>(
            Expression<Func<TFirst, TJson>> jsonSelector)
        {
            dataAdapter.TryAddJsonTypeMapper<TJson>();
            return this;
        }

        public ITrioChange<TFirst, TSecond, TThird> SetJsonSecond<TJson>(
            Expression<Func<TSecond, TJson>> jsonSelector)
        {
            dataAdapter.TryAddJsonTypeMapper<TJson>();
            return this;
        }

        public ITrioChange<TFirst, TSecond, TThird> SetJsonThird<TJson>(
            Expression<Func<TThird, TJson>> jsonSelector)
        {
            dataAdapter.TryAddJsonTypeMapper<TJson>();
            return this;
        }

        public ITrioChange<TFirst, TSecond, TThird> UseTransaction()
        {
            useTransaction = true;
            return this;
        }

        public virtual ITrioChange<TFirst, TSecond, TThird> ChangeBeforeExecute<TValue>(
            object spObject,
            Action<TFirst, TSecond, TThird, TValue> action,
            string spName = null)
        {
            var trioChangeExtented =
                new TrioChangeExtended<TFirst, TSecond, TThird, TValue>(this, inputs, dataAdapter, connectionFactory);
            trioChangeExtented.SetFields(spObject, action, spName);
            return trioChangeExtented;
        }

        public ITrioChange<TFirst, TSecond, TThird> ChangeBeforeExecuteJson<TValue, TJson>(
            object spObject,
            Action<TFirst, TSecond, TThird, TValue> action,
            string spName = null)
        {
            dataAdapter.TryAddJsonTypeMapper<TJson>();
            return ChangeBeforeExecute(spObject, action, spName);
        }

        public async Task<ICollection<TOut>> ExecuteScalarAsync<TOut>(
            Action<TSecond, TThird, TOut> actionAfterFirstExecute = null)
        {
            await SetConnectionAsync().ConfigureAwait(false);
            try
            {
                var result = await InnerExecuteScalarSingleAsync(actionAfterFirstExecute);

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
            Action<TSecond, TThird, TOut1> actionAfterFirstExecute = null,
            Action<TThird, TOut2> actionAfterSecondExecute = null)
        {
            await SetConnectionAsync().ConfigureAwait(false);
            try
            {
                var result = await InnerExecuteScalarPairAsync(
                    actionAfterFirstExecute, actionAfterSecondExecute);

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

        public async Task<ICollection<(TOut1, TOut2, TOut3)>> ExecuteScalarAsync<TOut1, TOut2, TOut3>(
            Action<TSecond, TThird, TOut1> actionAfterFirstExecute = null,
            Action<TThird, TOut2> actionAfterSecondExecute = null)
        {

            await SetConnectionAsync().ConfigureAwait(false);
            try
            {
                var result = await InnerExecuteScalarTrioAsync<TOut1, TOut2, TOut3>(
                    actionAfterFirstExecute, actionAfterSecondExecute);

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
                GetStoredProcedureName(typeof(TSecond)),
                GetStoredProcedureName(typeof(TThird)),
            };
        }

        protected override async Task InnerExecuteNonQueryAsync()
        {
            foreach (var (inputFirst, inputSecond, inputThrid) in inputs)
                await BaseInnerExecuteNonQueryAsync(
                    new object[] { inputFirst, inputSecond, inputThrid }, 0).ConfigureAwait(false);
        }

        protected virtual async Task<ICollection<TOut>> InnerExecuteScalarSingleAsync<TOut>(Action<TSecond, TThird, TOut> actionAfterFirstExecute)
        {
            var result = new List<TOut>();
            foreach (var (inputFirst, inputSecond, inputThrid) in inputs)
            {
                var scalar = await BaseInnerExecuteScalarAsync<TOut>(
                    inputFirst, spNames[0]).ConfigureAwait(false);

                actionAfterFirstExecute?.Invoke(inputSecond, inputThrid, scalar);

                await BaseInnerExecuteNonQueryAsync(
                    new object[] { inputFirst, inputSecond, inputThrid }, 1).ConfigureAwait(false);

                result.Add(scalar);
            }
            return result;
        }

        protected virtual async Task<ICollection<(TOut1, TOut2)>> InnerExecuteScalarPairAsync<TOut1, TOut2>(
            Action<TSecond, TThird, TOut1> actionAfterFirstExecute,
            Action<TThird, TOut2> actionAfterSecondExecute)
        {
            var result = new List<(TOut1, TOut2)>();
            foreach (var (inputFirst, inputSecond, inputThrid) in inputs)
            {
                var scalarFirst = await BaseInnerExecuteScalarAsync<TOut1>(
                    inputFirst, spNames[0]).ConfigureAwait(false);

                actionAfterFirstExecute?.Invoke(inputSecond, inputThrid, scalarFirst);

                var scalarSecond = await BaseInnerExecuteScalarAsync<TOut2>(
                    inputFirst, spNames[0]).ConfigureAwait(false);

                actionAfterSecondExecute?.Invoke(inputThrid, scalarSecond);

                await BaseInnerExecuteNonQueryAsync(
                    new object[] { inputFirst, inputSecond, inputThrid }, 2).ConfigureAwait(false);

                result.Add((scalarFirst, scalarSecond));
            }
            return result;
        }

        protected virtual async Task<ICollection<(TOut1, TOut2, TOut3)>> InnerExecuteScalarTrioAsync<TOut1, TOut2, TOut3>(
            Action<TSecond, TThird, TOut1> actionAfterFirstExecute,
            Action<TThird, TOut2> actionAfterSecondExecute)
        {
            var result = new List<(TOut1, TOut2, TOut3)>();
            foreach (var (inputFirst, inputSecond, inputThrid) in inputs)
            {
                var scalarFirst = await BaseInnerExecuteScalarAsync<TOut1>(
                    inputFirst, spNames[0]).ConfigureAwait(false);

                actionAfterFirstExecute?.Invoke(inputSecond, inputThrid, scalarFirst);

                var scalarSecond = await BaseInnerExecuteScalarAsync<TOut2>(
                    inputFirst, spNames[0]).ConfigureAwait(false);

                actionAfterSecondExecute?.Invoke(inputThrid, scalarSecond);

                var scalarThird = await BaseInnerExecuteScalarAsync<TOut3>(
                    inputFirst, spNames[0]).ConfigureAwait(false);

                result.Add((scalarFirst, scalarSecond, scalarThird));
            }
            return result;
        }
    }
}

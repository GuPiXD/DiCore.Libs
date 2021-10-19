using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiCore.Lib.PgSqlAccess.Orm.Change
{
    public class PairChangeExtended<TFirst, TSecond, TValue> : PairChange<TFirst, TSecond>
    {
        private object spObject;
        private Action<TFirst, TSecond, TValue> action;
        private string spName;

        internal PairChangeExtended(
            PairChange<TFirst, TSecond> baseInstance,
            IEnumerable<(TFirst, TSecond)> inputs,
            IDataAdapter dataAdapter,
            IConnectionFactory connectionFactory) : base(inputs, dataAdapter, connectionFactory)
        {
            baseInstance.CopyBase(this);
        }

        internal void SetFields(object spObject, Action<TFirst, TSecond, TValue> action, string spName = null)
        {
            this.spObject = spObject;
            this.action = action;
            this.spName = spName ?? GetStoredProcedureName(spObject.GetType());
        }

        public override IPairChange<TFirst, TSecond> ChangeBeforeExecute<TValueNew>(
            object spObject,
            Action<TFirst, TSecond, TValueNew> action,
            string spName = null)
        {
            throw new Exception("Change has already been added.");
        }

        protected override async Task InnerExecuteNonQueryAsync()
        {
            var changeResult = await BaseInnerExecuteScalarAsync<TValue>(
                spObject, spName).ConfigureAwait(false);

            foreach (var (inputFirst, inputSecond) in inputs)
            {
                action(inputFirst, inputSecond, changeResult);
                await BaseInnerExecuteNonQueryAsync(
                    new object[] { inputFirst, inputSecond }, 0).ConfigureAwait(false);
            }                
        }

        protected override async Task<ICollection<TOut>> InnerExecuteScalarSingleAsync<TOut>(Action<TSecond, TOut> actionAfterFirstExecute)
        {
            var changeResult = await BaseInnerExecuteScalarAsync<TValue>(
                spObject, spName).ConfigureAwait(false);

            var result = new List<TOut>();
            foreach (var (inputFirst, inputSecond) in inputs)
            {
                action(inputFirst, inputSecond, changeResult);

                var scalar = await BaseInnerExecuteScalarAsync<TOut>(
                    inputFirst, spNames[0]).ConfigureAwait(false);

                actionAfterFirstExecute?.Invoke(inputSecond, scalar);

                await BaseInnerExecuteNonQueryAsync(
                    new object[] { inputFirst, inputSecond }, 1).ConfigureAwait(false);

                result.Add(scalar);
            }
            return result;
        }

        protected override async Task<ICollection<(TOut1, TOut2)>> InnerExecuteScalarPairAsync<TOut1, TOut2>(Action<TSecond, TOut1> actionAfterFirstExecute)
        {
            var changeResult = await BaseInnerExecuteScalarAsync<TValue>(
                spObject, spName).ConfigureAwait(false);

            var result = new List<(TOut1, TOut2)>();
            foreach (var (inputFirst, inputSecond) in inputs)
            {
                action(inputFirst, inputSecond, changeResult);

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

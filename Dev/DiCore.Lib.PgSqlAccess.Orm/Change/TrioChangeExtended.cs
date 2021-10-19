using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiCore.Lib.PgSqlAccess.Orm.Change
{
    public class TrioChangeExtended<TFirst, TSecond, TThird, TValue> : TrioChange<TFirst, TSecond, TThird>
    {
        private object spObject;
        private Action<TFirst, TSecond, TThird, TValue> action;
        private string spName;

        internal TrioChangeExtended(
            TrioChange<TFirst, TSecond, TThird> baseInstance,
            IEnumerable<(TFirst, TSecond, TThird)> inputs,
            IDataAdapter dataAdapter,
            IConnectionFactory connectionFactory) : base(inputs, dataAdapter, connectionFactory)
        {
            baseInstance.CopyBase(this);
        }

        internal void SetFields(object spObject, Action<TFirst, TSecond, TThird, TValue> action, string spName = null)
        {
            this.spObject = spObject;
            this.action = action;
            this.spName = spName ?? GetStoredProcedureName(spObject.GetType());
        }

        public override ITrioChange<TFirst, TSecond, TThird> ChangeBeforeExecute<TValueNew>(
            object spObject,
            Action<TFirst, TSecond, TThird, TValueNew> action,
            string spName = null)
        {
            throw new Exception("Change has already been added.");
        }

        protected override async Task InnerExecuteNonQueryAsync()
        {
            var changeResult = await BaseInnerExecuteScalarAsync<TValue>(
                spObject, spName).ConfigureAwait(false);

            foreach (var (inputFirst, inputSecond, inputThrid) in inputs)
            {
                action(inputFirst, inputSecond, inputThrid, changeResult);
                await BaseInnerExecuteNonQueryAsync(
                    new object[] { inputFirst, inputSecond, inputThrid }, 0).ConfigureAwait(false);
            }
                
        }

        protected override async Task<ICollection<TOut>> InnerExecuteScalarSingleAsync<TOut>(
            Action<TSecond, TThird, TOut> actionAfterFirstExecute)
        {
            var changeResult = await BaseInnerExecuteScalarAsync<TValue>(
                spObject, spName).ConfigureAwait(false);
            var result = new List<TOut>();
            foreach (var (inputFirst, inputSecond, inputThrid) in inputs)
            {
                action(inputFirst, inputSecond, inputThrid, changeResult);

                var scalar = await BaseInnerExecuteScalarAsync<TOut>(
                    inputFirst, spNames[0]).ConfigureAwait(false);

                actionAfterFirstExecute?.Invoke(inputSecond, inputThrid, scalar);

                await BaseInnerExecuteNonQueryAsync(
                    new object[] { inputFirst, inputSecond, inputThrid }, 1).ConfigureAwait(false);

                result.Add(scalar);
            }
            return result;
        }

        protected override async Task<ICollection<(TOut1, TOut2)>> InnerExecuteScalarPairAsync<TOut1, TOut2>(
            Action<TSecond, TThird, TOut1> actionAfterFirstExecute,
            Action<TThird, TOut2> actionAfterSecondExecute)
        {
            var changeResult = await BaseInnerExecuteScalarAsync<TValue>(
                spObject, spName).ConfigureAwait(false);
            var result = new List<(TOut1, TOut2)>();
            foreach (var (inputFirst, inputSecond, inputThrid) in inputs)
            {
                action(inputFirst, inputSecond, inputThrid, changeResult);

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

        protected override async Task<ICollection<(TOut1, TOut2, TOut3)>> InnerExecuteScalarTrioAsync<TOut1, TOut2, TOut3>(
            Action<TSecond, TThird, TOut1> actionAfterFirstExecute,
            Action<TThird, TOut2> actionAfterSecondExecute)
        {
            var changeResult = await BaseInnerExecuteScalarAsync<TValue>(
                spObject, spName).ConfigureAwait(false);
            var result = new List<(TOut1, TOut2, TOut3)>();
            foreach (var (inputFirst, inputSecond, inputThrid) in inputs)
            {
                action(inputFirst, inputSecond, inputThrid, changeResult);

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

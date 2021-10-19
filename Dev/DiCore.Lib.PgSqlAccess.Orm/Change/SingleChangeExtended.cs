using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiCore.Lib.PgSqlAccess.Orm.Change
{
    public class SingleChangeExtended<T, TValue> : SingleChange<T>
    {
        private object spObject;
        private Action<T, TValue> action;
        private string spName;

        internal SingleChangeExtended(
            SingleChange<T> baseInstance,
            IEnumerable<T> inputs,
            IDataAdapter dataAdapter,
            IConnectionFactory connectionFactory) : base(inputs, dataAdapter, connectionFactory)
        {
            baseInstance.CopyBase(this);
        }

        internal void SetFields(object spObject, Action<T, TValue> action, string spName = null)
        {
            this.spObject = spObject;
            this.action = action;
            this.spName = spName ?? GetStoredProcedureName(spObject.GetType());
        }

        public override ISingleChange<T> ChangeBeforeExecute<TValueNew>(object spObject, Action<T, TValueNew> action, string spName = null)
        {
            throw new Exception("Change has already been added.");
        }

        protected override async Task InnerExecuteNonQueryAsync()
        {
            var changeResult = await BaseInnerExecuteScalarAsync<TValue>(
                spObject, spName).ConfigureAwait(false);

            foreach (var input in inputs)
            {
                action(input, changeResult);
                await BaseInnerExecuteNonQueryAsync(
                    new object[] { input }, 0).ConfigureAwait(false);
            }
                
        }

        protected override async Task<ICollection<TOut>> InnerExecuteScalarSingleAsync<TOut>()
        {
            var changeResult = await BaseInnerExecuteScalarAsync<TValue>(
                spObject, spName).ConfigureAwait(false);
            var result = new List<TOut>();

            foreach (var input in inputs)
            {
                action(input, changeResult);
                result.Add(await BaseInnerExecuteScalarAsync<TOut>(
                    input, spNames[0]).ConfigureAwait(false));
            }

            return result;
                
        }
    }
}

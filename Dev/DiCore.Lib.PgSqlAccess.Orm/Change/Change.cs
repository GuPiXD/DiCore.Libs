using DiCore.Lib.PgSqlAccess.DataAccessObjects;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiCore.Lib.PgSqlAccess.Orm.Change
{

    public abstract class Change
    {
        protected readonly IDataAdapter dataAdapter;
        protected readonly IConnectionFactory connectionFactory;
        protected string[] spNames;
        protected bool useTransaction = false;
        protected NpgsqlConnection connection;
        protected NpgsqlTransaction transaction;

        internal Change(IDataAdapter dataAdapter, IConnectionFactory connectionFactory)
        {
            this.dataAdapter = dataAdapter;
            this.connectionFactory = connectionFactory;
            spNames = GetSpNames();
        }

        public void SetStoredProcedureName(int index, string name)
        {
            spNames[index] = name;
        }

        public async Task ExecuteNonQueryAsync()
        {
            await SetConnectionAsync().ConfigureAwait(false);
            try
            {
                await InnerExecuteNonQueryAsync().ConfigureAwait(false);

                if (useTransaction)
                    await transaction.CommitAsync().ConfigureAwait(false);
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

        protected abstract string[] GetSpNames();

        protected abstract Task InnerExecuteNonQueryAsync();

        protected async Task SetConnectionAsync()
        {
            connection = await connectionFactory.GetConnectionAsync().ConfigureAwait(false);
            transaction = useTransaction ? connection.BeginTransaction() : null;
        }

        protected async Task BaseInnerExecuteNonQueryAsync(object[] inputs, int skip)
        {
            foreach (var (input, spName) in inputs.Zip(spNames, (i, sp) => (i, sp)).Skip(skip))
                using (var sp = GetPgStoredProcedure(spName))
                    await sp.ExecuteNonQueryAsync(input).ConfigureAwait(false);
        }

        protected async Task<TOut> BaseInnerExecuteScalarAsync<TOut>(object input, string spName)
        {
            using (var sp = GetPgStoredProcedure(spName))
                return await sp.ExecuteScalarAsync<TOut>(input).ConfigureAwait(false);
        }

        protected string GetStoredProcedureName(Type type)
        {
            var name = type.Name;
            var tableName = name.Contains("`") ? name.Split('`')[0] : name;
            return tableName;
        }

        protected internal void CopyBase(Change instance)
        {
            instance.spNames = spNames;
            instance.useTransaction = useTransaction;
            instance.connection = connection;
            instance.transaction = transaction;
        }

        private PgStoredProcedure GetPgStoredProcedure(string spName)
        {
            return useTransaction?
                new PgStoredProcedure(connection, dataAdapter.Schema, spName) :
                new PgStoredProcedure(connection, dataAdapter.Schema, spName, transaction);
        }
    }
}

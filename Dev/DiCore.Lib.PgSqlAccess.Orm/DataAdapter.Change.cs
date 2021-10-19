using DiCore.Lib.PgSqlAccess.DataAccessObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiCore.Lib.PgSqlAccess.Orm
{
    public partial class DataAdapter
    {
        public async Task ChangeAsync(object inputModel, string spName = null)
        {
            using (var connection = await connectionFactory.GetConnectionAsync().ConfigureAwait(false))
            {
                if (spName == null) spName = GetStoredProcedureName(inputModel.GetType());

                using (var sp = new PgStoredProcedure(connection, Schema, spName))
                    await sp.ExecuteNonQueryAsync(inputModel).ConfigureAwait(false);
            }
        }

        public async Task ChangeAsync(IEnumerable inputModels, string spName = null)
        {
            using (var connection = await connectionFactory.GetConnectionAsync().ConfigureAwait(false))
            {
                foreach (var inputModel in inputModels)
                {
                    if (spName == null) spName = GetStoredProcedureName(inputModel.GetType());

                    using (var sp = new PgStoredProcedure(connection, Schema, spName))
                        await sp.ExecuteNonQueryAsync(inputModel).ConfigureAwait(false);
                }
            }
        }

        public async Task<TOut> ChangeWithScalarAsync<TOut>(object inputModel, string spName = null)
        {
            using (var connection = await connectionFactory.GetConnectionAsync().ConfigureAwait(false))
            {
                if (spName == null) spName = GetStoredProcedureName(inputModel.GetType());

                using (var sp = new PgStoredProcedure(connection, Schema, spName))
                    return await sp.ExecuteScalarAsync<TOut>(inputModel).ConfigureAwait(false);
            }
        }

        public async Task<ICollection<TOut>> ChangeWithScalarAsync<TOut>(IEnumerable inputModels, string spName = null)
        {
            var result = new List<TOut>();
            using (var connection = await connectionFactory.GetConnectionAsync().ConfigureAwait(false))
            {
                foreach (var inputModel in inputModels)
                {
                    if (spName == null) spName = GetStoredProcedureName(inputModel.GetType());

                    using (var sp = new PgStoredProcedure(connection, Schema, spName))
                        result.Add(await sp.ExecuteScalarAsync<TOut>(inputModel).ConfigureAwait(false));
                }
                return result;
            }
        }

        [Obsolete]
        public async Task<ICollection<(TOut1, TOut2)>> ChangeWithScalarAsync<TOut1, TOut2>(
            IEnumerable<(object, object)> inputModels,
            string spName1 = null,
            string spName2 = null)
        {
            var result = new List<(TOut1, TOut2)>();
            using (var connection = await connectionFactory.GetConnectionAsync().ConfigureAwait(false))
            {
                foreach (var (inputModel1, inputModel2) in inputModels)
                {
                    TOut1 result1;
                    TOut2 result2;
                    if (spName1 == null) spName1 = GetStoredProcedureName(inputModel1.GetType());
                    if (spName2 == null) spName2 = GetStoredProcedureName(inputModel1.GetType());

                    using (var sp = new PgStoredProcedure(connection, Schema, spName1))
                        result1 = await sp.ExecuteScalarAsync<TOut1>(inputModel1).ConfigureAwait(false);

                    using (var sp = new PgStoredProcedure(connection, Schema, spName2))
                        result2 = await sp.ExecuteScalarAsync<TOut2>(inputModel2).ConfigureAwait(false);

                    result.Add((result1, result2));
                }
                return result;
            }
        }

        [Obsolete]
        public async Task<ICollection<(TOut1, TOut2, TOut3)>> ChangeWithScalarAsync<TOut1, TOut2, TOut3>(
            IEnumerable<(object, object, object)> inputModels,
            string spName1 = null,
            string spName2 = null,
            string spName3 = null)
        {
            var result = new List<(TOut1, TOut2, TOut3)>();
            using (var connection = await connectionFactory.GetConnectionAsync().ConfigureAwait(false))
            {
                foreach (var (inputModel1, inputModel2, inputModel3) in inputModels)
                {
                    TOut1 result1;
                    TOut2 result2;
                    TOut3 result3;

                    if (spName1 == null) spName1 = GetStoredProcedureName(inputModel1.GetType());
                    if (spName2 == null) spName2 = GetStoredProcedureName(inputModel2.GetType());
                    if (spName3 == null) spName3 = GetStoredProcedureName(inputModel3.GetType());

                    using (var sp = new PgStoredProcedure(connection, Schema, spName1))
                        result1 = await sp.ExecuteScalarAsync<TOut1>(inputModel1).ConfigureAwait(false);

                    using (var sp = new PgStoredProcedure(connection, Schema, spName2))
                        result2 = await sp.ExecuteScalarAsync<TOut2>(inputModel2).ConfigureAwait(false);

                    using (var sp = new PgStoredProcedure(connection, Schema, spName3))
                        result3 = await sp.ExecuteScalarAsync<TOut3>(inputModel3).ConfigureAwait(false);
                }
                return result;
            }
        }
    }
}

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
        public async Task ChangeJsonAsync<TJson>(object inputModel, string spName = null)
        {
            TryAddJsonTypeMapper<TJson>();
            await ChangeAsync(inputModel, spName: spName).ConfigureAwait(false);
        }

        public async Task ChangeJsonAsync<TJson>(IEnumerable inputModels, string spName = null)
        {
            TryAddJsonTypeMapper<TJson>();
            await ChangeAsync(inputModels, spName: spName).ConfigureAwait(false);
        }

        public async Task<TOut> ChangeJsonWithScalarAsync<TJson, TOut>(object inputModel, string spName = null)
        {
            TryAddJsonTypeMapper<TJson>();
            return await ChangeWithScalarAsync<TOut>(inputModel, spName: spName).ConfigureAwait(false);
        }

        public async Task<ICollection<TOut>> ChangeJsonWithScalarAsync<TJson, TOut>(IEnumerable inputModels, string spName = null)
        {
            TryAddJsonTypeMapper<TJson>();
            return await ChangeWithScalarAsync<TOut>(inputModels, spName: spName).ConfigureAwait(false);
        }

        [Obsolete]
        public async Task<ICollection<(TOut1, TOut2)>> ChangeJsonWithScalarAsync<TJson1, TOut1, TOut2>(
            IEnumerable<(object, object)> inputModels,
            string spName1 = null,
            string spName2 = null)
        {
            TryAddJsonTypeMapper<TJson1>();
            return await ChangeWithScalarAsync<TOut1, TOut2>(
                inputModels,
                spName1: spName1,
                spName2: spName2).ConfigureAwait(false);
        }

        [Obsolete]
        public async Task<ICollection<(TOut1, TOut2)>> ChangeJsonWithScalarAsync<TJson1, TJson2, TOut1, TOut2>(
            IEnumerable<(object, object)> inputModels,
            string spName1 = null,
            string spName2 = null)
        {
            TryAddJsonTypeMapper<TJson1>();
            TryAddJsonTypeMapper<TJson2>();
            return await ChangeWithScalarAsync<TOut1, TOut2>(
                inputModels,
                spName1: spName1,
                spName2: spName2).ConfigureAwait(false);
        }

        [Obsolete]
        public async Task<ICollection<(TOut1, TOut2, TOut3)>> ChangeJsonWithScalarAsync<TJson1, TOut1, TOut2, TOut3>(
            IEnumerable<(object, object, object)> inputModels,
            string spName1 = null,
            string spName2 = null,
            string spName3 = null)
        {
            TryAddJsonTypeMapper<TJson1>();
            return await ChangeWithScalarAsync<TOut1, TOut2, TOut3>(
                inputModels,
                spName1: spName1,
                spName2: spName2,
                spName3: spName3).ConfigureAwait(false);
        }

        [Obsolete]
        public async Task<ICollection<(TOut1, TOut2, TOut3)>> ChangeJsonWithScalarAsync<TJson1, TJson2, TOut1, TOut2, TOut3>(
            IEnumerable<(object, object, object)> inputModels,
            string spName1 = null,
            string spName2 = null,
            string spName3 = null)
        {
            TryAddJsonTypeMapper<TJson1>();
            TryAddJsonTypeMapper<TJson2>();
            return await ChangeWithScalarAsync<TOut1, TOut2, TOut3>(
                inputModels,
                spName1: spName1,
                spName2: spName2,
                spName3: spName3).ConfigureAwait(false);
        }

        [Obsolete]
        public async Task<ICollection<(TOut1, TOut2, TOut3)>> ChangeJsonWithScalarAsync<TJson1, TJson2, TJson3, TOut1, TOut2, TOut3>(
            IEnumerable<(object, object, object)> inputModels,
            string spName1 = null,
            string spName2 = null,
            string spName3 = null)
        {
            TryAddJsonTypeMapper<TJson1>();
            TryAddJsonTypeMapper<TJson2>();
            TryAddJsonTypeMapper<TJson3>();

            return await ChangeWithScalarAsync<TOut1, TOut2, TOut3>(
                inputModels,
                spName1: spName1,
                spName2: spName2,
                spName3: spName3).ConfigureAwait(false);
        }
    }
}

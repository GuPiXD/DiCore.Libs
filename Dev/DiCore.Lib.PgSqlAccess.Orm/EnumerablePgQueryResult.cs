using DiCore.Lib.PgSqlAccess.DataAccessObjects;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Npgsql;

namespace DiCore.Lib.PgSqlAccess.Orm
{
    internal class EnumerablePgQueryResult<T> : IEnumerable<T>
    {
        private readonly IEnumerable<T> result;
        private readonly PgQuery pgQuery;
        private readonly NpgsqlConnection npgsqlConnection;
        public EnumerablePgQueryResult(IEnumerable<T> result, PgQuery pgQuery, NpgsqlConnection npgsqlConnection)
        {
            this.result = result;
            this.pgQuery = pgQuery;
            this.npgsqlConnection = npgsqlConnection;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new EnumeratorPgQueryResult<T>(result.GetEnumerator(), pgQuery, npgsqlConnection);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new EnumeratorPgQueryResult<T>(result.GetEnumerator(), pgQuery, npgsqlConnection);
        }
    }

    internal class EnumeratorPgQueryResult<T> : IEnumerator<T>, IEnumerator
    {
        private readonly PgQuery pgQuery;
        private readonly IEnumerator<T> enumerator;
        private readonly NpgsqlConnection npgsqlConnection;
        public EnumeratorPgQueryResult(IEnumerator<T> enumerator, PgQuery pgQuery, NpgsqlConnection npgsqlConnection)
        {
            this.enumerator = enumerator;
            this.pgQuery = pgQuery;
            this.npgsqlConnection = npgsqlConnection;
        }
        public T Current => enumerator.Current;

        object IEnumerator.Current => enumerator.Current;

        public void Dispose()
        {
            enumerator.Dispose();
            pgQuery.Dispose();
            npgsqlConnection.Dispose();
        }

        public bool MoveNext()
        {
            return enumerator.MoveNext();
        }

        public void Reset()
        {
            enumerator.Reset();
        }
    }
}

using Npgsql;
using System;
using System.Threading.Tasks;

namespace DiCore.Lib.PgSqlAccess.Orm
{
    public class ConnectionFactory : IConnectionFactory
    {
        private string connectionString;

        public ConnectionFactory(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public void SetNewConnectionString(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public async Task<NpgsqlConnection> GetConnectionAsync()
        {
            var con = new NpgsqlConnection(connectionString);
            await con.OpenAsync().ConfigureAwait(false);
            return con;
        }

        public async Task TestConnectionAsync()
        {
            var con = new NpgsqlConnection(connectionString);
            await con.OpenAsync().ConfigureAwait(false);
            con.Close();
        }
    }
}

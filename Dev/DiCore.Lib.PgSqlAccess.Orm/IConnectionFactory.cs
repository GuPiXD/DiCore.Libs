using System.Threading.Tasks;
using Npgsql;

namespace DiCore.Lib.PgSqlAccess.Orm
{
    public interface IConnectionFactory
    {
        Task<NpgsqlConnection> GetConnectionAsync();
        void SetNewConnectionString(string connectionString);
        Task TestConnectionAsync();
    }
}
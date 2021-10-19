using System.Threading.Tasks;

namespace DiCore.Lib.PgSqlAccess.Orm.Change
{
    public interface IChange
    {
        Task ExecuteNonQueryAsync();
        void SetStoredProcedureName(int index, string name);
    }
}
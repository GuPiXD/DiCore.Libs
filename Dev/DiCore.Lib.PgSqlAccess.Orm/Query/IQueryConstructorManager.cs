namespace DiCore.Lib.PgSqlAccess.Orm.Query
{
    public interface IQueryConstructorManager
    {
        QueryConstructor<T> СonfigureQueryConstructor<T>(QueryConstructorFactory<T> queryConstructor);
    }
}
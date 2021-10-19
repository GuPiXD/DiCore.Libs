namespace DiCore.Lib.PgSqlAccess.Orm.Query
{
    public interface IQueryContructorConfigurator<T>
    {
        QueryConstructor<T> ConfigureQueryConstructor(QueryConstructorFactory<T> constructor);
    }
}

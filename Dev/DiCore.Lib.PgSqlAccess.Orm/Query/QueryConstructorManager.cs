using System;
using System.Collections.Generic;

namespace DiCore.Lib.PgSqlAccess.Orm.Query
{
    public class QueryConstructorManager : IQueryConstructorManager
    {
        private Dictionary<Type, object> queryCofiguratores = new Dictionary<Type, object>();

        public void Add<T>(IQueryContructorConfigurator<T> simpleQueryQreator)
        {
            queryCofiguratores.Add(typeof(T), simpleQueryQreator);
        }

        public QueryConstructor<T> СonfigureQueryConstructor<T>(QueryConstructorFactory<T> constructor)
        {
            var type = typeof(T);
            if (queryCofiguratores.ContainsKey(type))
            {
                return ((IQueryContructorConfigurator<T>)queryCofiguratores[type])
                    .ConfigureQueryConstructor(constructor);
            }

            return ConfigureDefaultQueryConstructor(constructor);
        }

        private QueryConstructor<T> ConfigureDefaultQueryConstructor<T>(QueryConstructorFactory<T> constructor)
        {
            return constructor
                .Create()
                .MapEntity();
        }
    }
}

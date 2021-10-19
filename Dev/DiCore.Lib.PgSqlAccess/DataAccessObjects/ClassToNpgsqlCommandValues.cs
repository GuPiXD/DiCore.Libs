using System;
using System.Linq;
using DiCore.Lib.FastPropertyAccess;
using Npgsql;

namespace DiCore.Lib.PgSqlAccess.DataAccessObjects
{
    /// <summary>
    /// Запись значений свойств экземпляра класса заданного типа в параметры команды
    /// </summary>
    /// <typeparam name="T">Тип</typeparam>
    public class ClassToNpgsqlCommandValues<T>
    {
        public ClassToNpgsqlCommandValues(bool ignoreCase = false)
        {
            FastPropertyAccess = new FastPropertyAccess<T>(EnPropertyUsingMode.Get, null, ignoreCase);
        }

        private IFastPropertyAccess<T> FastPropertyAccess { get; set; }

        /// <summary>
        /// Запись значений свойств экземпляра класса типа T в параметры команды
        /// </summary>
        /// <param name="command">Команда</param>
        /// <param name="instance">Экземпляр класса</param>
        /// <param name="toLowerCase">Преобразовать имя параметра к нижнему регистру</param>
        public void SetParameterValues(NpgsqlCommand command, T instance)
        {
            foreach (var parameter in command.Parameters.ToArray())
            {
                parameter.Value = FastPropertyAccess.Get(instance, parameter.ParameterName) ?? DBNull.Value;
            }
        }

        public void SetPositionalParameterValues(NpgsqlCommand command, T instance)
        {
            var properties = FastPropertyAccess.PropertyNames.ToArray();
            for (var i = 0; i < command.Parameters.Count; i++)
            {
                if (i >= properties.Length)
                    break;
                command.Parameters[i].Value = FastPropertyAccess.Get(instance, properties[i]) ?? DBNull.Value;
            }
        }
    }
}
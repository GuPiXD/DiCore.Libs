using System;

namespace DiCore.Lib.PgSqlAccess.Types.Attributes
{
    /// <summary>
    /// Аттрибут, хранящий наименование поля в БД из которого будет браться значение
    /// </summary>
    public class DbPropertyNameAttribute : Attribute
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="name"></param>
        public DbPropertyNameAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Наименование поля в БД для получения значения
        /// </summary>
        public string Name { get; set; }
    }
}

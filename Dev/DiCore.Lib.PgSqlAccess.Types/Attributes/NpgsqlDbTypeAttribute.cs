using System;
using DiCore.Lib.PgSqlAccess.Types.Enum;

namespace DiCore.Lib.PgSqlAccess.Types.Attributes
{
    /// <summary>
    /// Аттрибут, хранящий тип поля в БД 
    /// (для случаев когда на стороне клиента тип отличается от типа, который должен передаваться в NpgSql)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class NpgsqlDbTypeAttribute : Attribute
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="type"></param>
        public NpgsqlDbTypeAttribute(EnNpgsqlDbType type)
        {
            Type = type;
        }

        /// <summary>
        /// Наименование поля в БД для получения значения
        /// </summary>
        public EnNpgsqlDbType Type { get; set; }
    }
}
using System;
using System.Diagnostics;
using NpgsqlTypes;

namespace DiCore.Lib.PgSqlAccess.Types
{
    /// <summary>
    /// Параметры свойства
    /// </summary>
    [DebuggerDisplay("{Name}")]
    public class PropertyDescription
    {
        /// <summary>
        /// Порядковый номер свойства
        /// </summary>
        public int? Order { get; set; }

        /// <summary>
        /// Тип свойства
        /// </summary>
        public NpgsqlDbType Type { get; set; }

        /// <summary>
        /// Название свойства
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Тип свойства
        /// </summary>
        public Type PropertyType { get; set; }
    }
}
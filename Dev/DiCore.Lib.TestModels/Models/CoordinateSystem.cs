using System;
using System.Collections.Generic;
using System.Text;

namespace DiCore.Lib.TestModels.Models
{
    /// <summary>
    /// Схемы систем координат
    /// </summary>
    public class CoordinateSystem
    {
        /// <summary>
        /// Идентфикатор
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Название системы координат
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Json схема
        /// </summary>
        public string Schema { get; set; }
    }
}

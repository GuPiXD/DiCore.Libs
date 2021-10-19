using System;
using System.Collections.Generic;
using System.Text;

namespace DiCore.Lib.TestModels.Models
{
    /// <summary>
    /// Тип дефекта по POF
    /// </summary>
    public class POFArtifactType
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Название
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Код
        /// </summary>
        public string Code { get; set; }
    }
}

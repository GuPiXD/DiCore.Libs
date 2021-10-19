using System;
using System.Collections.Generic;
using System.Text;

namespace DiCore.Lib.TestModels.Models.Simple
{
    /// <summary>
    /// Артефакт
    /// </summary>
    public class Artifact
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid Id { get; set; }

        public ArtifactAddonMeasured ArtifactAddonMeasured { get; set; }

        public ArtifactAddonData ArtifactAddonData { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Номер дефекта
        /// </summary>
        public string Number { get; set; }

        public ArtifactClass ArtifactClass { get; set; }

        /// <summary>
        /// Артефакт нефтепровода
        /// </summary>
        public Guid? BaseObjectId { get; set; }

        public Guid TokenId { get; set; }

        /// <summary>
        /// Справочник методов диагностики
        /// </summary>
        public DiagnosticMethod DiagnosticMethod { get; set; }

        /// <summary>
        /// Дополнительные данные
        /// </summary>
        public string Data { get; set; }
        public bool vrDeleted { get; set; }
    }
}

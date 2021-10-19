using System;

namespace DiCore.Lib.TestModels.Models
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

        /// <summary>
        /// Описание
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Номер дефекта
        /// </summary>
        public string Number { get; set; }


        public Guid ClassId { get; set; }

        public ArtifactClass Class { get; set; }

        /// <summary>
        /// Артефакт нефтепровода
        /// </summary>
        public Guid? BaseObjectId { get; set; }

        /// <summary>
        /// Идентификатор диагностируемого объекта
        /// </summary>
        public Guid DiagnosticObjectId { get; set; }


        /// <summary>
        /// Справочник методов диагностики
        /// </summary>
        public DiagnosticMethod DiagnosticMethod { get; set; }

        /// <summary>
        /// Дополнительные данные
        /// </summary>
        public string Data { get; set; }
    }
}

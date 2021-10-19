using System;

namespace DiCore.Lib.TestModels.Models
{
    /// <summary>
    /// Диагностические данные
    /// </summary>
    public class DiagnosticData
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Идентификатор типа диагностических данных
        /// </summary>
        public Guid ClassId { get; set; }

        /// <summary>
        /// Хранилище данных
        /// </summary>
        public DiagnosticStorage DiagnosticStorage { get; set; }

        /// <summary>
        /// Ссылка на задание на диагностику
        /// </summary>
        public DiagnosticTarget DiagnosticTarget { get; set; }

        /// <summary>
        /// Путь к диагностическим данным (относительно корня)
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Дополнительные данные
        /// </summary>
        public string Data { get; set; }        
    }
}

using System;

namespace DiCore.Lib.TestModels.Models
{
    /// <summary>
    /// Хранилище данных
    /// </summary>
    public class DiagnosticStorage
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
        /// Корневой путь
        /// </summary>
        public string Path { get; set; }


    }
}

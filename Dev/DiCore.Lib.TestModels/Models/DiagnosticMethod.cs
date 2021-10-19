namespace DiCore.Lib.TestModels.Models
{
    /// <summary>
    /// Справочник методов диагностики
    /// </summary>
    public class DiagnosticMethod
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Название диагностического метода
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание диагностического метода
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Тип диагностики
        /// </summary>
        public int DiagnosticTypeId { get; set; }
    }
}

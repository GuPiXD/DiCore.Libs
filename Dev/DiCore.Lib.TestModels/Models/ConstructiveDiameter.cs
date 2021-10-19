namespace DiCore.Lib.TestModels.Models
{
    /// <summary>
    /// Диаметр
    /// </summary>
    public class ConstructiveDiameter
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Диаметр, мм
        /// </summary>
        public float DiameterMm { get; set; }

        /// <summary>
        /// Диаметр, дюйм
        /// </summary>
        public float DiameterInch { get; set; }

        /// <summary>
        /// Маркировка
        /// </summary>
        public string Marking { get; set; }

        /// <summary>
        /// Российский стандарт
        /// </summary>
        public bool IsGOST { get; set; }
        public float? DiameterForCalc { get; set; }
    }
}

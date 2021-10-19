namespace DiCore.Lib.BiLogic
{
    /// <summary>
    /// Описание артефакта в пределах трубной секции
    /// </summary>
    public class ArtifactLocation
    {
        #region Properties

        /// <summary>
        /// Относительная истанция артефакта в родительской секции, м
        /// </summary>
        public double Distance { get; private set; }

        /// <summary>
        /// Длина артефакта, мм
        /// </summary>
        public int MeasuredLength { get; private set; }

        /// <summary>
        /// Начальный угол артефакта, град
        /// </summary>
        public float AngularPosition { get; private set; }

        /// <summary>
        /// Ширина артефакта, мм
        /// </summary>
        public int MeasuredWidth { get; private set; }

        /// <summary>
        /// Угол продольного шва родительской секции, град (при наличии)
        /// </summary>
        public float? ParentPipeAngleFirst { get; private set; }

        /// <summary>
        /// >Угол продольного шва родительской секции, град (при наличии)
        /// </summary>
        public float? ParentPipeAngleSecond { get; private set; }

        /// <summary>
        /// Длина родительской секции, м
        /// </summary>
        public double ParentPipeLength { get; private set; }

        /// <summary>
        /// Номинальный наружний диаметр родительской секции, мм
        /// </summary>
        public double ParentPipeConstructiveDiameterMm { get; private set; }

        #endregion

        /// <summary>
        ///  Описание артефакта в пределах трубной секции
        /// </summary>
        /// <param name="distance">Относительная истанция артефакта в родительской секции, м</param>
        /// <param name="measuredLength">Длина артефакта, мм</param>
        /// <param name="angularPosition">Начальный угол артефакта, град</param>
        /// <param name="measuredWidth">Ширина артефакта, мм</param>
        /// <param name="parentPipeAngleFirst">Угол продольного шва родительской секции, град (при наличии)</param>
        /// <param name="parentPipeAngleSecond">Угол продольного шва родительской секции, град (при наличии)</param>
        /// <param name="parentPipeLength">Длина родительской секции, м</param>
        /// <param name="parentPipeConstructiveDiameterMm">Номинальный наружний диаметр родительской секции, мм</param>
        public ArtifactLocation(double distance, int measuredLength, float angularPosition, int measuredWidth, float? parentPipeAngleFirst, float? parentPipeAngleSecond, double parentPipeLength, double parentPipeConstructiveDiameterMm)
        {
            Distance = distance;
            MeasuredLength = measuredLength;
            AngularPosition = angularPosition;
            MeasuredWidth = measuredWidth;
            ParentPipeAngleFirst = parentPipeAngleFirst;
            ParentPipeAngleSecond = parentPipeAngleSecond;
            ParentPipeLength = parentPipeLength;
            ParentPipeConstructiveDiameterMm = parentPipeConstructiveDiameterMm;
        }
    }
}

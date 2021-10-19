using System;
using System.Xml;
using Diascan.Utils.IO;
using DiCore.Lib.NDT.Types;

namespace DiCore.Lib.NDT.DataProviders.NAV
{
    internal class NavDeviceParameters: IDeviceParameters
    {
        /// <summary>
        /// Идентификационный номер секции
        /// </summary>
        public int CarrierId { get; set; }

        /// <summary>
        /// Смещение нулевого датчика относительно нулевой точки дефектоскопа (вдоль оси прибора), мм
        /// </summary>
        public double DeltaX { get; set; }

        /// <summary>
        /// Номер датчика в ВМТ
        /// </summary>
        public int SensorTDC { get; set; }

        /// <summary>
        /// Признак реверсного прогона
        /// </summary>
        public bool IsReverse { get; set; }

        /// <summary>
        ///  Длина окружности трубы, мм (внешняя)
        /// </summary>
        public float PipeCircle { get; private set; }

        private int pipeDiameter;
        private float pipeDiameterMm;

        /// <summary>
        /// Диаметр (миллиметры)
        /// </summary>
        public float PipeDiameterMm
        {
            get => pipeDiameterMm;
            set
            {
                if (Math.Abs(PipeDiameterMm) >= 0.00007 && Math.Abs(value) < 0.00007)
                    return;

                pipeDiameterMm = value;
                PipeCircle = (float)Math.Round(PipeDiameterMm * Math.PI, 3, MidpointRounding.AwayFromZero);
            }
        }

        /// <summary>
        /// Диаметр (дюймы)
        /// </summary>
        public int PipeDiameter
        {
            get { return pipeDiameter; }
            set
            {
                if (pipeDiameter == value) return;
                pipeDiameter = value;

                if (Math.Abs(PipeDiameterMm) < 0.00007)
                    PipeDiameterMm = ParametersHelper.GetGOSTPipeDiamMm(pipeDiameter);
            }
        }

        /// <summary>
        /// Одометрический фактор, м
        /// </summary>
        public double OdoFactor { get; set; }

        /// <summary>
        /// Формирование скана
        /// </summary>
        public bool ScanFactor { get; set; }

        /// <summary>
        /// Смещение секции навигации, м
        /// </summary>
        public double NavShift { get; set; }

        /// <summary>
        /// Точность координат
        /// </summary>
        public double NavPrecision { get; set; }

        /// <summary>
        /// Расстояние между передней и задней манжетой
        /// </summary>
        public double DistanceCollar { get; set; }

        public static NavDeviceParameters LoadFromOmni(string omniPath)
        {
            var mpmDeviceParameters = new NavDeviceParameters();
            ReadFromOmni(mpmDeviceParameters, omniPath);
            return mpmDeviceParameters;
        }

        private static bool ReadFromOmni(NavDeviceParameters deviceParameters, string omniPath)
        {
            if (!File.Exists(omniPath)) return false;

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(omniPath);

                var root = xmlDoc.DocumentElement;
                if (root == null)
                    return false;


                deviceParameters.NavShift = 0;
                var node = root.SelectSingleNode("PF_CONSTANT_PARAMETERS");
                var nodeAttr = node?.Attributes?["NAV_SHIFT"];
                if (nodeAttr != null)
                {
                    if (Double.TryParse(nodeAttr.Value, out var navShift))
                        deviceParameters.NavShift = navShift;
                }

                deviceParameters.DistanceCollar = 0;
                node = root.SelectSingleNode("PF_CONSTANT_PARAMETERS");
                nodeAttr = node?.Attributes?["DIST_COLLAR"];
                if (nodeAttr != null)
                {
                    if (Double.TryParse(nodeAttr.Value, out var distCollar))
                        deviceParameters.DistanceCollar = distCollar;
                }

                deviceParameters.NavPrecision = 0.01d;
                node = root.SelectSingleNode("PF_CONSTANT_PARAMETERS");
                nodeAttr = node?.Attributes?["NAV_PRECISION"];
                if (nodeAttr != null)
                {
                    if (Double.TryParse(nodeAttr.Value, out var navPrecision))
                        deviceParameters.NavPrecision = navPrecision;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

using System;
using System.Xml;
using Diascan.Utils.IO;
using DiCore.Lib.NDT.Types;

namespace DiCore.Lib.NDT.DataProviders.MPM
{
    internal class MpmDeviceParameters: IDeviceParameters
    {
        /// <summary>
        /// Представление в виде строки
        /// </summary>
        public override string ToString()
        {
            return CarrierId.ToString();
        }

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
            get { return pipeDiameterMm; }
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
        /// Начальная дистанция показаний секции, м
        /// </summary>
        public double StartDist { get; set; }

        /// <summary>
        /// Конечняя дистанция показаний секции, м
        /// </summary>
        public double EndDist { get; set; }

        /// <summary>
        /// Формирование скана
        /// </summary>
        public bool ScanFactor { get; set; }

        public Calibration Calibration { get; set; }

        public static MpmDeviceParameters LoadFromOmni(string omniPath)
        {
            var mpmDeviceParameters = new MpmDeviceParameters();
            ReadFromOmni(mpmDeviceParameters, omniPath);
            return mpmDeviceParameters;
        }

        private static bool ReadFromOmni(MpmDeviceParameters deviceParameters, string omniPath)
        {
            if (!File.Exists(omniPath)) return false;

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(omniPath);

                var root = xmlDoc.DocumentElement;
                if (root == null)
                    return false;

                deviceParameters.CarrierId = 0;
                var node = root.SelectSingleNode("PF_CONSTANT_PARAMETERS");
                var nodeAttr = node?.Attributes?["CARRIER_ID"];
                if (nodeAttr != null)
                {
                    if (Int32.TryParse(nodeAttr.Value, out var carrierId))
                        deviceParameters.CarrierId = carrierId;
                }

                deviceParameters.DeltaX = 0d;
                node = root.SelectSingleNode("PF_CONSTANT_PARAMETERS");
                nodeAttr = node?.Attributes?["DELTA_X"];
                if (nodeAttr != null)
                {
                    if (Double.TryParse(ParametersHelper.DecimalSeparatorToParse(nodeAttr.Value), out var deltaX))
                        deviceParameters.DeltaX = deltaX;
                }

                deviceParameters.SensorTDC = 0;
                node = root.SelectSingleNode("PF_CONSTANT_PARAMETERS");
                nodeAttr = node?.Attributes?["SENSOR_TDC"];
                if (nodeAttr != null)
                {
                    int sensorTDC;
                    if (Int32.TryParse(nodeAttr.Value, out sensorTDC))
                        deviceParameters.SensorTDC = sensorTDC;
                }

                deviceParameters.IsReverse = false;
                node = root.SelectSingleNode("RUN_CONSTANT_PARAMETERS");
                nodeAttr = node?.Attributes?["IS_REVERSE"];
                if (nodeAttr != null)
                {
                    int isReverse;
                    if (Int32.TryParse(nodeAttr.Value, out isReverse))
                        deviceParameters.IsReverse = isReverse == 1;
                }

                deviceParameters.PipeDiameter = 0;
                node = root.SelectSingleNode("PIPELINE");
                nodeAttr = node?.Attributes?["PIPE_DIAMETR"];
                if (nodeAttr != null)
                {
                    int pipeDiameter;
                    if (Int32.TryParse(nodeAttr.Value, out pipeDiameter))
                        deviceParameters.PipeDiameter = pipeDiameter;
                }

                node = root.SelectSingleNode("PIPELINE");
                nodeAttr = node?.Attributes?["PIPE_DIAMETR_MM"];
                if (nodeAttr != null)
                {
                    float pipeDiameterMm;
                    if (Single.TryParse(ParametersHelper.DecimalSeparatorToParse(nodeAttr.Value), out pipeDiameterMm))
                        deviceParameters.PipeDiameterMm = pipeDiameterMm;
                }

                deviceParameters.OdoFactor = 0d;
                node = root.SelectSingleNode("RUN_CONSTANT_PARAMETERS");
                nodeAttr = node?.Attributes?["ODO_FACTOR"];
                if (nodeAttr != null)
                {
                    double odoFactor;
                    if (Double.TryParse(ParametersHelper.DecimalSeparatorToParse(nodeAttr.Value), out odoFactor))
                        deviceParameters.OdoFactor = odoFactor;
                }

                deviceParameters.StartDist = 0d;
                node = root.SelectSingleNode("INSPECTIONS_PARAMETERS/INSPECTION");
                nodeAttr = node?.Attributes?["MPM_START"];
                if (nodeAttr != null)
                {
                    if (Double.TryParse(ParametersHelper.DecimalSeparatorToParse(nodeAttr.Value), out var startDist))
                        deviceParameters.StartDist = startDist;
                }

                deviceParameters.EndDist = 0d;
                node = root.SelectSingleNode("INSPECTIONS_PARAMETERS/INSPECTION");
                nodeAttr = node?.Attributes?["MPM_END"];
                if (nodeAttr != null)
                {
                    if (Double.TryParse(ParametersHelper.DecimalSeparatorToParse(nodeAttr.Value), out var endDist))
                        deviceParameters.EndDist = endDist;
                }

                deviceParameters.ScanFactor = false;
                node = root.SelectSingleNode("PF_CONSTANT_PARAMETERS");
                nodeAttr = node?.Attributes?["SCANFACTOR"];
                if (nodeAttr != null)
                {
                    if (Int32.TryParse(nodeAttr.Value, out var scanFactor))
                        deviceParameters.ScanFactor = scanFactor == 1;
                }

                deviceParameters.Calibration = Calibration.LoadFromOmni(omniPath);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

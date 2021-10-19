using System;
using System.Xml;
using Diascan.Utils.IO;
using DiCore.Lib.NDT.Types;

namespace DiCore.Lib.NDT.CoordinateProvider
{
    /// <summary>
    /// Константы и величины для расчетов
    /// </summary>
    public class CalcParameters
    {
        private int pipeDiameter;
        private float pipeDiameterMm;

        /// <summary>
        /// Диаметр (дюймы)
        /// </summary>
        public int PipeDiameter
        {
            get => pipeDiameter;
            set
            {
                if (pipeDiameter == value) return;
                pipeDiameter = value;

                if (Math.Abs(PipeDiameterMm) < 0.00007)
                    PipeDiameterMm = ParametersHelper.GetGOSTPipeDiamMm(pipeDiameter);
            }
        }

        /// <summary>
        ///  Длина окружности трубы, мм (внешняя)
        /// </summary>
        public float PipeCircle { get; private set; }

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
        /// Одометрический фактор, м
        /// </summary>
        public double OdoFactor { get; set; }

        /// <summary>
        /// Признак реверсного прогона
        /// </summary>
        public bool IsReverse { get; set; }

        /// <summary>
        /// Период записи координатной информации
        /// </summary>
        public int PeriodCoordinateInfo { get; set; }

        /// <summary>
        /// Тип значения датчика углового положения
        /// </summary>
        public enAngleType TypeAngleSensor { get; set; } //int

   
        /// <summary>
        /// Смещение для прогона по дистанции, м
        /// </summary>
        public double DistanceOffsetRaw { get; set; }

        public double DistanceOffset
        {
            get => DistanceOffsetRaw + AdjustmentSectionOffsetValue;
            set => DistanceOffsetRaw = value - AdjustmentSectionOffsetValue;
        }

        public double AdjustmentSectionOffsetValue { get; set; }

        /// <summary>
        /// Смещение для прогона по углу, мм
        /// </summary>        
        public int AngleOffset { get; set; }

        /// <summary>
        ///  Смещение для прогона по углу, град
        /// </summary>
        public int AngleOffsetDegree
        {
            get
            {
                var dergeeAngle = (AngleOffset * 360.0 / (PipeDiameterMm * Math.PI));
                var offset = (360.0 - dergeeAngle) % 360.0;
                if (offset > 181) offset -= 360;

                return (int)Math.Round(offset, MidpointRounding.AwayFromZero);
            }
            set
            {
                var offset = (360.0 - value) % 360.0;

                AngleOffset = (int)Math.Round(offset * PipeDiameterMm * Math.PI / 360.0, MidpointRounding.AwayFromZero);
            }
        }

        #region --- Параметры NMS ---

        /// <summary>
        /// Смещение маркерной системы, мм
        /// </summary>        
        public double NMSDeltaX { get; set; }

        /// <summary>
        /// Тип привязки маркерной системы
        /// </summary>
        public string NMSType { get; set; }

        #endregion

        public static CalcParameters LoadFromOmni(string omniPath)
        {
            var calcParameters = new CalcParameters();
            ReadFromOmni(calcParameters, omniPath);
            return calcParameters;
        }

        private static bool ReadFromOmni(CalcParameters calcParameters, string omniPath)
        {
            if (!File.Exists(omniPath)) return false;

            try
            {
                var xmlDoc = new XmlDocument();
                // var readAllText = Utils.IO.File.ReadAllText(omniPath);
                xmlDoc.Load(omniPath);
                var root = xmlDoc.DocumentElement;
                if (root == null)
                    return false;

                calcParameters.PipeDiameter = 0;
                var node = root.SelectSingleNode("PIPELINE");
                var carrierAtrr = node?.Attributes?["PIPE_DIAMETR"];
                if (carrierAtrr != null)
                {
                    int pipeDiameter;
                    if (Int32.TryParse(carrierAtrr.Value, out pipeDiameter))
                        calcParameters.PipeDiameter = pipeDiameter;
                }

                calcParameters.PipeDiameterMm = 0f;
                node = root.SelectSingleNode("PIPELINE");
                carrierAtrr = node?.Attributes?["PIPE_DIAMETR_MM"];
                if (carrierAtrr != null)
                {
                    float pipeDiameterMm;
                    if (Single.TryParse(ParametersHelper.DecimalSeparatorToParse(carrierAtrr.Value), out pipeDiameterMm))
                        calcParameters.PipeDiameterMm = pipeDiameterMm;
                }

                calcParameters.OdoFactor = 0d;
                node = root.SelectSingleNode("RUN_CONSTANT_PARAMETERS");
                carrierAtrr = node?.Attributes?["ODO_FACTOR"];
                if (carrierAtrr != null)
                {
                    double odoFactor;
                    if (Double.TryParse(ParametersHelper.DecimalSeparatorToParse(carrierAtrr.Value), out odoFactor))
                        calcParameters.OdoFactor = odoFactor;
                }

                calcParameters.IsReverse = false;
                node = root.SelectSingleNode("RUN_CONSTANT_PARAMETERS");
                carrierAtrr = node?.Attributes?["IS_REVERSE"];
                if (carrierAtrr != null)
                {
                    bool isReverse;
                    if (Boolean.TryParse(carrierAtrr.Value, out isReverse))
                        calcParameters.IsReverse = isReverse;
                }

                calcParameters.PeriodCoordinateInfo = 0;
                node = root.SelectSingleNode("INSPECTIONS_PARAMETERS/INSPECTION");
                carrierAtrr = node?.Attributes?["PERIOD_COORDINATE_INFO"];
                if (carrierAtrr != null)
                {
                    int periodCoordinateInfo;
                    if (Int32.TryParse(carrierAtrr.Value, out periodCoordinateInfo))
                        calcParameters.PeriodCoordinateInfo = periodCoordinateInfo;
                }

                calcParameters.TypeAngleSensor = 0;
                node = root.SelectSingleNode("INSPECTIONS_PARAMETERS/INSPECTION");
                carrierAtrr = node?.Attributes?["TYPE_ANGLE_COORDINATE_INFO"];
                if (carrierAtrr != null)
                {
                    int typeAngleSensor;
                    if (Int32.TryParse(carrierAtrr.Value, out typeAngleSensor))
                        calcParameters.TypeAngleSensor = (enAngleType)typeAngleSensor;
                }

                calcParameters.DistanceOffsetRaw = 0d;
                node = root.SelectSingleNode("RUN_CONSTANT_PARAMETERS");
                carrierAtrr = node?.Attributes?["DISTANCE_OFFSET"];
                if (carrierAtrr != null)
                {
                    double distanceOffset;
                    if (Double.TryParse(ParametersHelper.DecimalSeparatorToParse(carrierAtrr.Value), out distanceOffset))
                        calcParameters.DistanceOffsetRaw = distanceOffset;
                }

                calcParameters.AngleOffset = 0;
                node = root.SelectSingleNode("RUN_CONSTANT_PARAMETERS");
                carrierAtrr = node?.Attributes?["ANGLE_OFFSET"];
                if (carrierAtrr != null)
                {
                    int angleOffset;
                    if (Int32.TryParse(carrierAtrr.Value, out angleOffset))
                        calcParameters.AngleOffset = angleOffset;
                }

                calcParameters.NMSDeltaX = 0d;
                node = root.SelectSingleNode("NMS_CONSTANT_PARAMETERS");
                carrierAtrr = node?.Attributes?["DELTA_X"];
                if (carrierAtrr != null)
                {
                    double nmsDeltaX;
                    if (Double.TryParse(ParametersHelper.DecimalSeparatorToParse(carrierAtrr.Value), out nmsDeltaX))
                        calcParameters.NMSDeltaX = nmsDeltaX;
                }

                calcParameters.NMSType = "";
                node = root.SelectSingleNode("NMS_CONSTANT_PARAMETERS");
                carrierAtrr = node?.Attributes?["TYPE"];
                if (carrierAtrr != null)
                {
                    calcParameters.NMSType = carrierAtrr.Value;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    public enum enAngleType
    {
        Degree,
        AngleCode
    }

}

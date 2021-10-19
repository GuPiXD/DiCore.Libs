using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Diascan.Utils.IO;
using DiCore.Lib.NDT.DataProviders.CDM;
using DiCore.Lib.NDT.Types;

namespace DiCore.Lib.NDT.DataProviders.CDPA
{
    internal class CDpaDeviceParameters : IDeviceParameters
    {
        /// <summary>
        /// Идентификационный номер секции
        /// </summary>
        public int CarrierId { get; set; }
        /// <summary>
        /// Смещение нулевого датчика относительно нулевой точки дефектоскопа (вдоль оси прибора), мм (WM)
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
        /// Размерность показаний датчика
        /// </summary>
        public int SizeSensor { get; set; }

        /// <summary>
        /// Максимальное количество измерений в скане для каждого датчика
        /// </summary>
        public byte MaxCountSignal { get; set; }
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
        /// <summary>
        ///  Длина окружности трубы, мм (внешняя)
        /// </summary>
        public float PipeCircle { get; private set; }

        /// <summary>
        /// Максимальная амплитуда
        /// </summary>
        public float MaxAmplitude { get; set; }

        /// <summary>
        /// Коэф. дискретизации для расчета времени прихода первого эха
        /// </summary>        
        public float TimeDiscretFirst { get; set; }

        /// <summary>
        /// Коэф. дискретизации для расчета времени прихода следующего эха
        /// </summary>
        public float TimeDiscretNext { get; set; }

        /// <summary>
        /// "Слепая зона"
        /// </summary>  
        public float TimeSleepZone { get; set; }

        /// <summary>
        /// Коэффициент пересчета амплитуды
        /// </summary>
        public int KAmplitude { get; set; }

        /// <summary>
        /// Скорость звука в среде m/sec
        /// </summary>
        public float UltrasonicSpeedOil { get; set; }

        /// <summary>
        /// Скорость звука в метале m/sec
        /// </summary>
        public float UltrasonicSpeedMetal { get; set; }
        /// <summary>
        /// Коэффициент размера пакета данных
        /// </summary>
        public float PacketKSize { get; set; }

        /// <summary>
        /// Размерность эхо сигнала
        /// </summary>
        public enRawEchoSize RawEchoSize { get; set; }

        public List<CDpaDirection> CDpaDirections { get; set; }

        public List<CDpaRuleParameters> CDpaRuleParametersList { get; set; } = new List<CDpaRuleParameters>();

        public static CDpaDeviceParameters LoadFromOmni(string omniPath)
        {
            var cdpaDeviceParameters = new CDpaDeviceParameters();
            ReadFromOmni(cdpaDeviceParameters, omniPath);
            return cdpaDeviceParameters;
        }

        private static bool ReadFromOmni(CDpaDeviceParameters deviceParameters, string omniPath)
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
                var node = root.SelectSingleNode("CDF_CONSTANT_PARAMETERS");
                var nodeAttr = node?.Attributes?["CARRIER_ID"];
                if (nodeAttr != null)
                {
                    if (Int32.TryParse(nodeAttr.Value, out var carrierId))
                        deviceParameters.CarrierId = carrierId;
                }

                deviceParameters.DeltaX = 0d;
                node = root.SelectSingleNode("CDF_CONSTANT_PARAMETERS");
                nodeAttr = node?.Attributes?["DELTA_X"];
                if (nodeAttr != null)
                {
                    if (Double.TryParse(ParametersHelper.DecimalSeparatorToParse(nodeAttr.Value), out var deltaX))
                        deviceParameters.DeltaX = deltaX;
                }

                deviceParameters.SensorTDC = 0;
                node = root.SelectSingleNode("CDF_CONSTANT_PARAMETERS");
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
                nodeAttr = node?.Attributes?["CD_START"];
                if (nodeAttr != null)
                {
                    if (Double.TryParse(ParametersHelper.DecimalSeparatorToParse(nodeAttr.Value), out var startDist))
                        deviceParameters.StartDist = startDist;
                }

                deviceParameters.EndDist = 0d;
                node = root.SelectSingleNode("INSPECTIONS_PARAMETERS/INSPECTION");
                nodeAttr = node?.Attributes?["CD_END"];
                if (nodeAttr != null)
                {
                    if (Double.TryParse(ParametersHelper.DecimalSeparatorToParse(nodeAttr.Value), out var endDist))
                        deviceParameters.EndDist = endDist;
                }

                deviceParameters.ScanFactor = false;
                node = root.SelectSingleNode("CDF_CONSTANT_PARAMETERS");
                nodeAttr = node?.Attributes?["SCANFACTOR"];
                if (nodeAttr != null)
                {
                    if (Int32.TryParse(nodeAttr.Value, out var scanFactor))
                        deviceParameters.ScanFactor = scanFactor == 1;
                }

                deviceParameters.TimeSleepZone = 0f;
                node = root.SelectSingleNode("CDF_CONSTANT_PARAMETERS/THRESHOLDS");
                nodeAttr = node?.Attributes?["TIME_SLEEP_ZONE"];
                if (nodeAttr != null)
                {
                    if (Single.TryParse(ParametersHelper.DecimalSeparatorToParse(nodeAttr.Value), out var timeSleepZone))
                        deviceParameters.TimeSleepZone = timeSleepZone;
                }

                deviceParameters.TimeDiscretFirst = 0f;
                node = root.SelectSingleNode("CDF_CONSTANT_PARAMETERS/THRESHOLDS");
                nodeAttr = node?.Attributes?["TIME_DISCRET_FIRST"];
                if (nodeAttr != null)
                {
                    if (Single.TryParse(ParametersHelper.DecimalSeparatorToParse(nodeAttr.Value), out var timeDiscretFirst))
                        deviceParameters.TimeDiscretFirst = timeDiscretFirst;
                }

                deviceParameters.TimeDiscretNext = 0f;
                node = root.SelectSingleNode("CDF_CONSTANT_PARAMETERS/THRESHOLDS");
                nodeAttr = node?.Attributes?["TIME_DISCRET_NEXT"];
                if (nodeAttr != null)
                {
                    if (Single.TryParse(ParametersHelper.DecimalSeparatorToParse(nodeAttr.Value), out var timeDiscretNext))
                        deviceParameters.TimeDiscretNext = timeDiscretNext;
                }

                deviceParameters.KAmplitude = 4;
                node = root.SelectSingleNode("CDF_CONSTANT_PARAMETERS");
                nodeAttr = node?.Attributes?["K_AMPLITUDE"];
                if (nodeAttr != null)
                {
                    if (Int32.TryParse(nodeAttr.Value, out var kAmplitude))
                        deviceParameters.KAmplitude = kAmplitude;
                }

                deviceParameters.MaxCountSignal = 64;
                node = root.SelectSingleNode("CDF_CONSTANT_PARAMETERS/THRESHOLDS");
                nodeAttr = node?.Attributes?["MAX_COUNT_SIGNAL"];
                if (nodeAttr != null)
                {
                    if (Byte.TryParse(nodeAttr.Value, out var maxCountSignale))
                        deviceParameters.MaxCountSignal = maxCountSignale;
                }

                deviceParameters.UltrasonicSpeedOil = 1360;
                node = root.SelectSingleNode("PIPELINE");
                nodeAttr = node?.Attributes?["USS_IN_PRODUCT"];
                if (nodeAttr != null)
                {
                    if (float.TryParse(nodeAttr.Value, out var ultrasonicSpeedOil))
                        deviceParameters.UltrasonicSpeedOil = ultrasonicSpeedOil;
                }

                deviceParameters.UltrasonicSpeedMetal = 3230;
                node = root.SelectSingleNode("PIPELINE");
                nodeAttr = node?.Attributes?["USS_IN_PIPE_TRANSVERSE"];
                if (nodeAttr != null)
                {
                    if (float.TryParse(nodeAttr.Value, out var ultrasonicSpeedMetal))
                        deviceParameters.UltrasonicSpeedMetal = ultrasonicSpeedMetal;
                }

                deviceParameters.PacketKSize = 4;
                node = root.SelectSingleNode("CDF_CONSTANT_PARAMETERS");
                nodeAttr = node?.Attributes?["K_SIZE"];
                if (nodeAttr != null)
                {
                    if (float.TryParse(nodeAttr.Value, out var packetKSize))
                        deviceParameters.PacketKSize = packetKSize;
                }

                deviceParameters.RawEchoSize = enRawEchoSize._32bit;
                node = root.SelectSingleNode("CDF_CONSTANT_PARAMETERS");
                nodeAttr = node?.Attributes?["SIZE_UZB_SENSOR_OMNI"];
                if (nodeAttr != null)
                {
                    int rawEchoSize;
                    if (Int32.TryParse(nodeAttr.Value, out rawEchoSize))
                        deviceParameters.RawEchoSize = (enRawEchoSize)rawEchoSize;
                }

                var nodes = node.SelectNodes($"CDF_RULES/RULE");
                if (nodes == null) return true;

                foreach (XmlNode nodeItem in nodes)
                {
                    var attribute = nodeItem?.Attributes?["ID"];
                    if (attribute == null) continue;
                    var strId = string.Empty;
                    if (String.IsNullOrEmpty(attribute.Value)) continue;
                    else strId = attribute.Value;

                    attribute = nodeItem.Attributes?["FOCUS_DEPTH"];
                    if (attribute == null) continue;
                    if (!float.TryParse(ParametersHelper.DecimalSeparatorToParse(attribute.Value), out var focusDepth)) continue;

                    attribute = nodeItem.Attributes?["ANGLE_METALL"];
                    if (attribute == null) continue;
                    if (!Single.TryParse(ParametersHelper.DecimalSeparatorToParse(attribute.Value), out var angleInMetal)) continue;

                    attribute = nodeItem.Attributes?["ANGLE_PRODUCT"];
                    if (attribute == null) continue;
                    if (!Single.TryParse(ParametersHelper.DecimalSeparatorToParse(attribute.Value), out var angleInProduct)) continue;

                    deviceParameters.CDpaRuleParametersList.Add(new CDpaRuleParameters()
                    {
                        StrId = strId,
                        FocusDepth = focusDepth,
                        AngleInMetal = angleInMetal,
                        AngleInProduct = angleInProduct,
                    });
                }

                deviceParameters.CDpaDirections = CDpaDirectionHelper.LoadFromOmni(omniPath);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
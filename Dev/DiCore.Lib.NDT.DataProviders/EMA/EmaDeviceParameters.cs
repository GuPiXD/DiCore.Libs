using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Diascan.Utils.IO;
using DiCore.Lib.NDT.Types;

namespace DiCore.Lib.NDT.DataProviders.EMA
{
    internal class EmaDeviceParameters: IDeviceParameters
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
        /// Максимальное количество сигналов от одного датчика в скане
        /// </summary>
        public int MaxCountSignal { get; set; }

        /// <summary>
        /// Размерность показаний датчика
        /// </summary>
        public int SizeSensor { get; set; }

        /// <summary>
        /// "Слепая зона"
        /// </summary>  
        public float TimeSleepZone { get; set; }

        /// <summary>
        /// Коэффициент пересчета амплитуды
        /// </summary>
        public int KAmplitude { get; set; }

        /// <summary>
        /// Коэф. дискретизации для расчета времени прихода эха
        /// </summary>
        public float TimeDiscret { get; set; }
        
        public List<EmaRule> EmaRules { get; set; }

        public List<EmaTimeRule> EmaTimeRules { get; set; }

        /// <summary>
        /// Тип эхосигналов
        /// </summary>
        public enEchoType EchoType { get; set; }

        public static EmaDeviceParameters LoadFromOmni(string omniPath)
        {
            var emaDeviceParameters = new EmaDeviceParameters();
            ReadFromOmni(emaDeviceParameters, omniPath);
            return emaDeviceParameters;
        }

        private static bool ReadFromOmni(EmaDeviceParameters deviceParameters, string omniPath)
        {
            if (!File.Exists(omniPath)) return false;

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(omniPath);

                var root = xmlDoc.DocumentElement;
                if (root == null)
                    return false;

                #region node EMA_CONSTANT_PARAMETERS
                var node = root.SelectSingleNode("EMA_CONSTANT_PARAMETERS");
                deviceParameters.CarrierId = 0;
                var nodeAttr = node?.Attributes?["CARRIER_ID"];
                if (nodeAttr != null)
                {
                    if (int.TryParse(nodeAttr.Value, out var carrierId))
                        deviceParameters.CarrierId = carrierId;
                }

                deviceParameters.DeltaX = 0d;
                nodeAttr = node?.Attributes?["DELTA_X"];
                if (nodeAttr != null)
                {
                    if (double.TryParse(ParametersHelper.DecimalSeparatorToParse(nodeAttr.Value), out var deltaX))
                        deviceParameters.DeltaX = deltaX;
                }

                deviceParameters.SensorTDC = 0;
                nodeAttr = node?.Attributes?["SENSOR_TDC"];
                if (nodeAttr != null)
                {
                    if (int.TryParse(nodeAttr.Value, out var sensorTDC))
                        deviceParameters.SensorTDC = sensorTDC;
                }
                #endregion

                #region node PIPELINE
                node = root.SelectSingleNode("PIPELINE");
                deviceParameters.PipeDiameter = 0;
                nodeAttr = node?.Attributes?["PIPE_DIAMETR"];
                if (nodeAttr != null)
                {
                    if (int.TryParse(nodeAttr.Value, out var pipeDiameter))
                        deviceParameters.PipeDiameter = pipeDiameter;
                }

                nodeAttr = node?.Attributes?["PIPE_DIAMETR_MM"];
                if (nodeAttr != null)
                {
                    if (float.TryParse(ParametersHelper.DecimalSeparatorToParse(nodeAttr.Value), out var pipeDiameterMm))
                        deviceParameters.PipeDiameterMm = pipeDiameterMm;
                }
                #endregion

                #region node RUN_CONSTANT_PARAMETERS
                node = root.SelectSingleNode("RUN_CONSTANT_PARAMETERS");

                deviceParameters.IsReverse = false;
                nodeAttr = node?.Attributes?["IS_REVERSE"];
                if (nodeAttr != null)
                {
                    if (int.TryParse(nodeAttr.Value, out var isReverse))
                        deviceParameters.IsReverse = isReverse == 1;
                }

                deviceParameters.OdoFactor = 0d;
                nodeAttr = node?.Attributes?["ODO_FACTOR"];
                if (nodeAttr != null)
                {
                    if (double.TryParse(ParametersHelper.DecimalSeparatorToParse(nodeAttr.Value), out var odoFactor))
                        deviceParameters.OdoFactor = odoFactor;
                }
                #endregion

                #region node INSPECTIONS_PARAMETERS/INSPECTION
                node = root.SelectSingleNode("INSPECTIONS_PARAMETERS/INSPECTION");
                deviceParameters.StartDist = 0d;
                nodeAttr = node?.Attributes?["MPM_START"];
                if (nodeAttr != null)
                {
                    if (double.TryParse(ParametersHelper.DecimalSeparatorToParse(nodeAttr.Value), out var startDist))
                        deviceParameters.StartDist = startDist;
                }

                deviceParameters.EndDist = 0d;
                nodeAttr = node?.Attributes?["EMA_END"];
                if (nodeAttr != null)
                {
                    if (double.TryParse(ParametersHelper.DecimalSeparatorToParse(nodeAttr.Value), out var endDist))
                        deviceParameters.EndDist = endDist;
                }
                #endregion

                #region node EMA_CONSTANT_PARAMETERS
                node = root.SelectSingleNode("EMA_CONSTANT_PARAMETERS");

                deviceParameters.ScanFactor = false;
                nodeAttr = node?.Attributes?["SCANFACTOR"];
                if (nodeAttr != null)
                {
                    if (int.TryParse(nodeAttr.Value, out var scanFactor))
                        deviceParameters.ScanFactor = scanFactor == 1;
                }

                deviceParameters.MaxCountSignal = 32;
                nodeAttr = node?.Attributes?["MAX_COUNT_SIGNAL"];
                if (nodeAttr != null)
                {
                    if (int.TryParse(nodeAttr.Value, out var maxCountSignale))
                        deviceParameters.MaxCountSignal = maxCountSignale;
                }

                deviceParameters.SizeSensor = 16;
                nodeAttr = node?.Attributes?["SIZE_UZB_SENSOR_OMNI"];
                if (nodeAttr != null)
                {
                    if (int.TryParse(nodeAttr.Value, out var sizeSensor))
                        deviceParameters.SizeSensor = sizeSensor;
                }

                nodeAttr = node?.Attributes?["TYPE_ECHO"];
                if (nodeAttr != null)
                    if (int.TryParse(nodeAttr.Value, out var id))
                        deviceParameters.EchoType = id >= 0 ? (enEchoType)id : enEchoType.Full;
                    else
                        deviceParameters.EchoType = enEchoType.Full;
                #endregion

                #region node EMA_CONSTANT_PARAMETERS/THRESHOLDS
                node = root.SelectSingleNode("EMA_CONSTANT_PARAMETERS/THRESHOLDS");
                deviceParameters.TimeSleepZone = 0f;
                nodeAttr = node?.Attributes?["TIME_SLEEP_ZONE"];
                if (nodeAttr != null)
                {
                    if (float.TryParse(ParametersHelper.DecimalSeparatorToParse(nodeAttr.Value), out var timeSleepZone))
                        deviceParameters.TimeSleepZone = timeSleepZone;
                }

                deviceParameters.KAmplitude = 4;
                nodeAttr = node?.Attributes?["K_AMPLITUDE"];
                if (nodeAttr != null)
                {
                    if (Int32.TryParse(nodeAttr.Value, out var kAmplitude))
                        deviceParameters.KAmplitude = kAmplitude;
                }

                deviceParameters.TimeDiscret = 0;
                nodeAttr = node?.Attributes?["TIME_DISCRET"];
                if (nodeAttr != null)
                {
                    if (float.TryParse(nodeAttr.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var timeDiscret))
                        deviceParameters.TimeDiscret = timeDiscret;
                }
                #endregion

                deviceParameters.EmaRules     = EmaRulesHelper.LoadFromOmni(omniPath);
                deviceParameters.EmaTimeRules = EmaTimeRulesLoad.LoadFromOmni(omniPath);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

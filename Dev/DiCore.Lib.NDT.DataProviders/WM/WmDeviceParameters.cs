using System;
using System.Xml;
using Diascan.Utils.IO;
using DiCore.Lib.NDT.Types;

namespace DiCore.Lib.NDT.DataProviders.WM
{
    public class WmDeviceParameters :  IDeviceParameters
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

        private int pipeDiameter;
        private float pipeDiameterMm;

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
        ///  Длина окружности трубы, мм (внешняя)
        /// </summary>
        public float PipeCircle { get; private set; }

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
        /// Модернизированный WM
        /// </summary>
        public enUSDModel UsdModel { get; set; }

        /// <summary>
        /// Размер пакета данных от датчика
        /// </summary>
        public int BlockSize { get; set; }

        /// <summary>
        /// Максимальное количество измерений в скане для каждого датчика
        /// </summary>
        public byte MaxCountSignal { get; set; }

        /// <summary>
        /// Размерность эхо сигнала
        /// </summary>
        public enRawEchoSize RawEchoSize { get; set; }

        /// <summary>
        /// Коэф. дискретизации для расчета времени прихода первого эха
        /// </summary>
        public float TimeDiscretFirst { get; set; }

        /// <summary>
        /// "Слепая зона"
        /// </summary>
        public float TimeSleepZone { get; set; }

        /// <summary>
        /// "Слепая зона следующих эхо сигналов"
        /// </summary>
        public float TimeSleepZoneNext { get; set; }

        /// <summary>
        /// Коэф. дискретизации для расчета времени прихода следующего эха
        /// </summary>
        public float TimeDiscretNext { get; set; }

        /// <summary>
        /// Мультипликатор амплитуды сигнала WM32
        /// </summary>
        public int AmpMultiplicator { get; set; }

        /// <summary>
        /// Скорость звука в среде m/sec
        /// </summary>
        public float UltrasonicSpeedOil { get; set; }

        /// <summary>
        /// Скорость звука в метале m/sec
        /// </summary>
        public float UltrasonicSpeedMetal { get; set; }

        /// <summary>
        /// Диагностические данные являются WM32 или нет
        /// </summary>
        public bool IsWM32 { get; set; }

        /// <summary>
        /// Формирование скана WM
        /// </summary>
        public bool ScanFactor { get; set; }

        public ThresholdsParameters ThresholdsParameters { get; set; }


        public static WmDeviceParameters LoadFromOmni(string omniPath)
        {
            var wmDeviceParameters = new WmDeviceParameters();
            ReadFromOmni(wmDeviceParameters, omniPath);
            return wmDeviceParameters;
        }

        private static bool ReadFromOmni(WmDeviceParameters deviceParameters, string omniPath)
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

                #region RUN_CONSTANT_PARAMETERS
                deviceParameters.IsReverse = false;
                var node = root.SelectSingleNode("RUN_CONSTANT_PARAMETERS");
                var nodeAttr = node?.Attributes?["IS_REVERSE"];
                if (nodeAttr != null)
                {
                    int isReverse;
                    if (Int32.TryParse(nodeAttr.Value, out isReverse))
                        deviceParameters.IsReverse = isReverse == 1;
                }

                deviceParameters.OdoFactor = 0d;
                nodeAttr = node?.Attributes?["ODO_FACTOR"];
                if (nodeAttr != null)
                {
                    double odoFactor;
                    if (Double.TryParse(ParametersHelper.DecimalSeparatorToParse(nodeAttr.Value), out odoFactor))
                        deviceParameters.OdoFactor = odoFactor;
                }
                #endregion

                #region PIPELINE
                deviceParameters.PipeDiameter = 0;
                node = root.SelectSingleNode("PIPELINE");
                nodeAttr = node?.Attributes?["PIPE_DIAMETR"];
                if (nodeAttr != null)
                {
                    int pipeDiameter;
                    if (Int32.TryParse(nodeAttr.Value, out pipeDiameter))
                        deviceParameters.PipeDiameter = pipeDiameter;
                }

                nodeAttr = node?.Attributes?["PIPE_DIAMETR_MM"];
                if (nodeAttr != null)
                {
                    float pipeDiameterMm;
                    if (Single.TryParse(ParametersHelper.DecimalSeparatorToParse(nodeAttr.Value), out pipeDiameterMm))
                        deviceParameters.PipeDiameterMm = pipeDiameterMm;
                }

                deviceParameters.UltrasonicSpeedOil = 1360f;
                nodeAttr = node?.Attributes?["USS_IN_PRODUCT"];
                if (nodeAttr != null)
                {
                    float ultrasonicSpeedOil;
                    if (Single.TryParse(ParametersHelper.DecimalSeparatorToParse(nodeAttr.Value), out ultrasonicSpeedOil))
                        deviceParameters.UltrasonicSpeedOil = ultrasonicSpeedOil;
                }

                deviceParameters.UltrasonicSpeedMetal = 5960f;
                nodeAttr = node?.Attributes?["USS_IN_PIPE_LONGITUDINAL"];
                if (nodeAttr != null)
                {
                    float ultrasonicSpeedMetal;
                    if (Single.TryParse(ParametersHelper.DecimalSeparatorToParse(nodeAttr.Value), out ultrasonicSpeedMetal))
                        deviceParameters.UltrasonicSpeedMetal = ultrasonicSpeedMetal;
                }
                #endregion

                #region INSPECTIONS_PARAMETERS/INSPECTION
                deviceParameters.StartDist = 0d;
                node = root.SelectSingleNode("INSPECTIONS_PARAMETERS/INSPECTION");
                nodeAttr = node?.Attributes?["WM_START"];
                if (nodeAttr != null)
                {
                    double startDist;
                    if (Double.TryParse(ParametersHelper.DecimalSeparatorToParse(nodeAttr.Value), out startDist))
                        deviceParameters.StartDist = startDist;
                }

                deviceParameters.EndDist = 0d;
                nodeAttr = node?.Attributes?["WM_END"];
                if (nodeAttr != null)
                {
                    double endDist;
                    if (Double.TryParse(ParametersHelper.DecimalSeparatorToParse(nodeAttr.Value), out endDist))
                        deviceParameters.EndDist = endDist;
                }
                #endregion

                #region WM_CONSTANT_PARAMETERS
                deviceParameters.CarrierId = 0;
                node = root.SelectSingleNode("WM_CONSTANT_PARAMETERS");
                nodeAttr = node?.Attributes?["CARRIER_ID"];
                if (nodeAttr != null)
                {
                    if (Int32.TryParse(nodeAttr.Value, out var carrierId))
                        deviceParameters.CarrierId = carrierId;
                }

                deviceParameters.DeltaX = 0d;
                nodeAttr = node?.Attributes?["DELTA_X"];
                if (nodeAttr != null)
                {
                    if (Double.TryParse(ParametersHelper.DecimalSeparatorToParse(nodeAttr.Value), out var deltaX))
                        deviceParameters.DeltaX = deltaX;
                }

                deviceParameters.SensorTDC = 0;
                nodeAttr = node?.Attributes?["SENSOR_TDC"];
                if (nodeAttr != null)
                {
                    int sensorTDC;
                    if (Int32.TryParse(nodeAttr.Value, out sensorTDC))
                        deviceParameters.SensorTDC = sensorTDC;
                }

                deviceParameters.UsdModel = enUSDModel.DMU;
                nodeAttr = node?.Attributes?["USD_MODEL"];
                if (nodeAttr != null)
                {
                    int usdModel;
                    if (Int32.TryParse(nodeAttr.Value, out usdModel))
                        deviceParameters.UsdModel = (enUSDModel)usdModel;
                }

                deviceParameters.BlockSize = 2;
                nodeAttr = node?.Attributes?["BLOCK_SIZE"];
                if (nodeAttr != null)
                {
                    int blockSize;
                    if (Int32.TryParse(nodeAttr.Value, out blockSize))
                        deviceParameters.BlockSize = blockSize;
                }

                deviceParameters.RawEchoSize = enRawEchoSize._16bit;
                nodeAttr = node?.Attributes?["SIZE_UZB_SENSOR_OMNI"];
                if (nodeAttr != null)
                {
                    int rawEchoSize;
                    if (Int32.TryParse(nodeAttr.Value, out rawEchoSize))
                        deviceParameters.RawEchoSize = (enRawEchoSize)rawEchoSize;
                }

                deviceParameters.AmpMultiplicator = 1;
                nodeAttr = node?.Attributes?["AMP_MULTIPLIER"];
                if (nodeAttr != null)
                {
                    int ampMultiplicator;
                    if (Int32.TryParse(nodeAttr.Value, out ampMultiplicator))
                        deviceParameters.AmpMultiplicator = ampMultiplicator;
                }

                deviceParameters.ScanFactor = false;
                nodeAttr = node?.Attributes?["SCANFACTOR"];
                if (nodeAttr != null)
                {
                    int scanFactor;
                    if (Int32.TryParse(nodeAttr.Value, out scanFactor))
                        deviceParameters.ScanFactor = scanFactor == 1;
                }

                deviceParameters.IsWM32 = false;
                nodeAttr = node?.Attributes?["WM32"];
                if (nodeAttr != null)
                {
                    int isWM32;
                    if (Int32.TryParse(nodeAttr.Value, out isWM32))
                        deviceParameters.IsWM32 = isWM32 == 1;
                }
                #endregion

                #region "WM_CONSTANT_PARAMETERS/THRESHOLDS
                deviceParameters.MaxCountSignal = 32;
                node = root.SelectSingleNode("WM_CONSTANT_PARAMETERS/THRESHOLDS");
                nodeAttr = node?.Attributes?["MAX_COUNT_SIGNAL"];
                if (nodeAttr != null)
                {
                    byte countSignal;
                    if (Byte.TryParse(nodeAttr.Value, out countSignal))
                        deviceParameters.MaxCountSignal = countSignal;
                }

                deviceParameters.TimeDiscretFirst = 0f;
                nodeAttr = node?.Attributes?["TIME_DISCRET_FIRST"];
                if (nodeAttr != null)
                {
                    float timeDiscretFirst;
                    if (Single.TryParse(ParametersHelper.DecimalSeparatorToParse(nodeAttr.Value), out timeDiscretFirst))
                        deviceParameters.TimeDiscretFirst = timeDiscretFirst;
                }

                deviceParameters.TimeSleepZone = 0f;
                nodeAttr = node?.Attributes?["TIME_SLEEP_ZONE"];
                if (nodeAttr != null)
                {
                    float timeSleepZone;
                    if (Single.TryParse(ParametersHelper.DecimalSeparatorToParse(nodeAttr.Value), out timeSleepZone))
                        deviceParameters.TimeSleepZone = timeSleepZone;
                }

                deviceParameters.TimeSleepZoneNext = 0f;
                nodeAttr = node?.Attributes?["TIME_SLEEP_NEXT"];
                if (nodeAttr != null)
                {
                    float timeSleepZoneNext;
                    if (Single.TryParse(ParametersHelper.DecimalSeparatorToParse(nodeAttr.Value), out timeSleepZoneNext))
                        deviceParameters.TimeSleepZoneNext = timeSleepZoneNext;
                }

                deviceParameters.TimeDiscretNext = 0f;
                nodeAttr = node?.Attributes?["TIME_DISCRET_NEXT"];
                if (nodeAttr != null)
                {
                    float timeDiscretNext;
                    if (Single.TryParse(ParametersHelper.DecimalSeparatorToParse(nodeAttr.Value), out timeDiscretNext))
                        deviceParameters.TimeDiscretNext = timeDiscretNext;
                }
                #endregion
                
                deviceParameters.ThresholdsParameters = ThresholdsParameters.LoadFromOmni(omniPath);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

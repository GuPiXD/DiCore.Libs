using System;
using System.Xml;
using Diascan.NDT.Enums;
using Diascan.Utils.IO;
using DiCore.Lib.NDT.Types;

namespace DiCore.Lib.NDT.DataProviders.MFL
{
    internal class MflDeviceParameters : IDeviceParameters
    {
        /// <summary>
        /// Идентификационный номер секции
        /// </summary>
        public int CarrierId { get; set; }

        /// <summary>
        /// MflId = ДМУ = CarrierDetail
        /// </summary>
        public int MflId { get; set; }

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
                PipeCircle = (float) Math.Round(PipeDiameterMm * Math.PI, 3, MidpointRounding.AwayFromZero);
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
        /// Размерность показаний датчика в байтах
        /// </summary>
        public int SizeSensor { get; set; }

        /// <summary>
        /// Формирование скана
        /// </summary>
        public bool ScanFactor { get; set; }

        /// <summary>
        /// Номинальное смещение показаний датчиков
        /// </summary>
        public float NominalOffset { get; set; }

        /// <summary>
        /// Коэффициент чувствительности датчика
        /// </summary>
        public float FactorSensitivity { get; set; }

        public SensorsCalibrations SensorsCalibrations { get; set; }

        public SensorsMap SensorsMap { get; set; }

        public static MflDeviceParameters LoadFromOmni(string omniPath, DataType dataType)
        {
            var mflDeviceParameters = new MflDeviceParameters();
            ReadFromOmni(mflDeviceParameters, omniPath, dataType);
            return mflDeviceParameters;
        }

        private static bool ReadFromOmni(MflDeviceParameters deviceParameters, string omniPath, DataType dataType)
        {
            if (!File.Exists(omniPath)) return false;

            var rootNodePath = $"MFL_CONSTANT_PARAMETERS/T{DataTypeToOmniPosition(dataType)}";

            try
            {
                var xmlDoc = new XmlDocument();
                // var readAllText = Utils.IO.File.ReadAllText(omniPath);
                xmlDoc.Load(omniPath);
                var root = xmlDoc.DocumentElement;
                if (root == null)
                    return false;

                deviceParameters.CarrierId = 0;
                var node = root.SelectSingleNode(rootNodePath);
                var carrierAtrr = node?.Attributes?["CARRIER_ID"];
                if (carrierAtrr != null)
                {
                    if (Int32.TryParse(carrierAtrr.Value, out var carrierId))
                        deviceParameters.CarrierId = carrierId;
                }

                deviceParameters.DeltaX = 0d;
                node = root.SelectSingleNode(rootNodePath);
                carrierAtrr = node?.Attributes?["DELTA_X"];
                if (carrierAtrr != null)
                {
                    if (Double.TryParse(ParametersHelper.DecimalSeparatorToParse(carrierAtrr.Value), out var deltaX))
                        deviceParameters.DeltaX = deltaX;
                }

                deviceParameters.SensorTDC = 0;
                node = root.SelectSingleNode(rootNodePath);
                carrierAtrr = node?.Attributes?["SENSOR_TDC"];
                if (carrierAtrr != null)
                {
                    if (Int32.TryParse(carrierAtrr.Value, out var sensorTDC))
                        deviceParameters.SensorTDC = sensorTDC;
                }

                deviceParameters.IsReverse = false;
                node = root.SelectSingleNode("RUN_CONSTANT_PARAMETERS");
                carrierAtrr = node?.Attributes?["IS_REVERSE"];
                if (carrierAtrr != null)
                {
                    if (Int32.TryParse(carrierAtrr.Value, out var isReverse))
                        deviceParameters.IsReverse = isReverse == 1;
                }

                deviceParameters.PipeDiameter = 0;
                node = root.SelectSingleNode("PIPELINE");
                carrierAtrr = node?.Attributes?["PIPE_DIAMETR"];
                if (carrierAtrr != null)
                {
                    if (Int32.TryParse(carrierAtrr.Value, out var pipeDiameter))
                        deviceParameters.PipeDiameter = pipeDiameter;
                }

                node = root.SelectSingleNode("PIPELINE");
                carrierAtrr = node?.Attributes?["PIPE_DIAMETR_MM"];
                if (carrierAtrr != null)
                {
                    if (Single.TryParse(ParametersHelper.DecimalSeparatorToParse(carrierAtrr.Value),
                        out var pipeDiameterMm))
                        deviceParameters.PipeDiameterMm = pipeDiameterMm;
                }

                deviceParameters.OdoFactor = 0d;
                node = root.SelectSingleNode("RUN_CONSTANT_PARAMETERS");
                carrierAtrr = node?.Attributes?["ODO_FACTOR"];
                if (carrierAtrr != null)
                {
                    if (Double.TryParse(ParametersHelper.DecimalSeparatorToParse(carrierAtrr.Value), out var odoFactor))
                        deviceParameters.OdoFactor = odoFactor;
                }

                deviceParameters.StartDist = 0d;
                node = root.SelectSingleNode("INSPECTIONS_PARAMETERS/INSPECTION");
                carrierAtrr = node?.Attributes?["MFL_START"];
                if (carrierAtrr != null)
                {
                    if (Double.TryParse(ParametersHelper.DecimalSeparatorToParse(carrierAtrr.Value), out var startDist))
                        deviceParameters.StartDist = startDist;
                }

                deviceParameters.EndDist = 0d;
                node = root.SelectSingleNode("INSPECTIONS_PARAMETERS/INSPECTION");
                carrierAtrr = node?.Attributes?["MFL_END"];
                if (carrierAtrr != null)
                {
                    if (Double.TryParse(ParametersHelper.DecimalSeparatorToParse(carrierAtrr.Value), out var endDist))
                        deviceParameters.EndDist = endDist;
                }

                deviceParameters.ScanFactor = false;
                node = root.SelectSingleNode(rootNodePath);
                carrierAtrr = node?.Attributes?["SCANFACTOR"];
                if (carrierAtrr != null)
                {
                    if (Int32.TryParse(carrierAtrr.Value, out var scanFactor))
                        deviceParameters.ScanFactor = scanFactor == 1;
                }

                deviceParameters.SizeSensor = 0;
                node = root.SelectSingleNode(rootNodePath);
                carrierAtrr = node?.Attributes?["SIZESENSOR"];
                if (carrierAtrr != null)
                {
                    if (Int32.TryParse(carrierAtrr.Value, out var sizeSensor))
                        deviceParameters.SizeSensor = sizeSensor;
                }

                deviceParameters.NominalOffset = 0;
                node = root.SelectSingleNode(rootNodePath);
                carrierAtrr = node?.Attributes?["NOMINAL_OFFSET"];
                if (carrierAtrr != null)
                {
                    if (Single.TryParse(ParametersHelper.DecimalSeparatorToParse(carrierAtrr.Value), out var nominalOffset))
                        deviceParameters.NominalOffset = nominalOffset;
                }

                deviceParameters.FactorSensitivity = 0;
                node = root.SelectSingleNode(rootNodePath);
                carrierAtrr = node?.Attributes?["FACTOR_SENSITIVITY"];
                if (carrierAtrr != null)
                {
                    if (Single.TryParse(ParametersHelper.DecimalSeparatorToParse(carrierAtrr.Value),
                        out var factorSensitivity))
                        deviceParameters.FactorSensitivity = factorSensitivity;
                }

                deviceParameters.SensorsCalibrations = SensorsCalibrations.LoadFromOmni(omniPath, DataTypeToOmniPosition(dataType));

                deviceParameters.SensorsMap = SensorsMap.LoadFromOmni(omniPath, DataTypeToOmniPosition(dataType));

                deviceParameters.MflId = 0;
                node = root.SelectSingleNode("MFL_CONSTANT_PARAMETERS");
                carrierAtrr = node?.Attributes?["MFL_ID"];
                if (carrierAtrr != null)
                {
                    if (Int32.TryParse(carrierAtrr.Value, out var mflId))
                        deviceParameters.MflId = mflId;
                }


                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static int DataTypeToOmniPosition(DataType dataType)
        {
            switch (dataType)
            {
                default:
                case DataType.MflT1:
                    return 1;
                case DataType.MflT11:
                    return 11;
                case DataType.MflT2:
                    return 2;
                case DataType.MflT22:
                    return 22;
                case DataType.MflT3:
                    return 3;
                case DataType.MflT31:
                    return 31;
                case DataType.MflT32:
                    return 32;
                case DataType.MflT33:
                    return 33;
                case DataType.MflT34:
                    return 34;
                case DataType.TfiT4:
                    return 4;
                case DataType.TfiT41:
                    return 41;
            }
        }
    }
}

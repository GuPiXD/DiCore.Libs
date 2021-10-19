using System;
using System.Collections.Generic;
using System.Xml;
using Diascan.Utils.IO;
using DiCore.Lib.NDT.Types;

namespace DiCore.Lib.NDT.DataProviders.MFL
{
    internal class SensorCalibration
    {
        public float NominalOffset { get; set; }
        public float FactorSensitivity { get; set; }
        public int Number { get; set; }
    }

    internal class SensorsCalibrations: List<SensorCalibration>
    {
        public static SensorsCalibrations LoadFromOmni(string omniPath, int dataTypeOmniPosition)
        {
            var sensorCalibrations = new SensorsCalibrations();
            ReadFromOmni(sensorCalibrations, omniPath, dataTypeOmniPosition);
            return sensorCalibrations;
        }

        private static bool ReadFromOmni(SensorsCalibrations sensorCalibrations, string omniPath,
            int dataTypeOmniPosition)
        {
            if (!File.Exists(omniPath)) return false;

            var rootNodePath = $"MFL_CONSTANT_PARAMETERS/T{dataTypeOmniPosition}/CALIBRATON_SENSORS/SENSOR";
            try
            {
                var xmlDoc = new XmlDocument();
                // var readAllText = Utils.IO.File.ReadAllText(omniPath);
                xmlDoc.Load(omniPath);
                var root = xmlDoc.DocumentElement;
                if (root == null)
                    return false;

                var nodes = xmlDoc.SelectNodes(rootNodePath);
                if (nodes == null) return true;

                foreach (XmlNode node in nodes)
                {
                    var attribute = node?.Attributes?["NUMBER"];
                    if (attribute == null) continue;
                    if (!Int32.TryParse(attribute.Value, out var number)) continue;

                    attribute = node.Attributes?["NOMINAL_OFFSET_S"];
                    if (attribute == null) continue;
                    if (Single.TryParse(ParametersHelper.DecimalSeparatorToParse(attribute.Value),
                        out var nominalOffset)) continue;

                    attribute = node.Attributes?["FACTOR_SENSITIVITY_S"];
                    if (attribute == null) continue;
                    if (Single.TryParse(ParametersHelper.DecimalSeparatorToParse(attribute.Value),
                        out var factorSensitivity)) continue;

                    sensorCalibrations.Add(new SensorCalibration
                    {
                        Number = number,
                        NominalOffset = nominalOffset,
                        FactorSensitivity = factorSensitivity
                    });
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

using System;
using System.Collections.Generic;
using System.Xml;
using Diascan.Utils.IO;

namespace DiCore.Lib.NDT.DataProviders.MFL
{
    internal class SensorMap
    {
        /// <summary>
        /// Номер блока датчиков
        /// </summary>
        public string SensorBlockSerialNumber { get; set; }

        /// <summary>
        /// Позиция датчика на блоке датчиков
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Номер датчика
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Идентификатор исполнения блока датчиков
        /// </summary>
        public int ModificationId { get; set; } = -1;
    }

    internal class SensorsMap : List<SensorMap>
    {
        public static SensorsMap LoadFromOmni(string omniPath, int dataTypeOmniPosition)
        {
            var sensorCalibrations = new SensorsMap();
            ReadFromOmni(sensorCalibrations, omniPath, dataTypeOmniPosition);
            return sensorCalibrations;
        }

        private static bool ReadFromOmni(SensorsMap sensorsMap, string omniPath,
            int dataTypeOmniPosition)
        {
            if (!File.Exists(omniPath)) return false;

            var rootNodePath = $"MFL_CONSTANT_PARAMETERS/T{dataTypeOmniPosition}/SENSOR_MAP/SENSOR";
            try
            {
                var xmlDoc = new XmlDocument();

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

                    attribute = node.Attributes?["SERIAL_BD"];
                    if (String.IsNullOrEmpty(attribute?.Value)) continue;
                    var sensorBlockSerialNumber = attribute.Value;

                    attribute = node.Attributes?["MODIFICATION_BD"];
                    if (attribute == null) continue;
                    if (!Int32.TryParse(attribute.Value, out var modificationId)) continue;

                    attribute = node.Attributes?["POSITION"];
                    if (attribute == null) continue;
                    if (!Int32.TryParse(attribute.Value, out var position)) continue;

                    sensorsMap.Add(new SensorMap
                    {
                        Number = number,
                        SensorBlockSerialNumber = sensorBlockSerialNumber,
                        ModificationId = modificationId,
                        Position = position
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

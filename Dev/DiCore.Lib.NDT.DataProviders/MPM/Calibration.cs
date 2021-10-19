using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Diascan.Utils.IO;

namespace DiCore.Lib.NDT.DataProviders.MPM
{
    internal class Calibration
    {
        public List<Lever> Levers { get; set; }
        public List<Plate> Plates { get; set; }


        public static Calibration LoadFromOmni(string omniPath)
        {
            var calibration = new Calibration();

            ReadFromOmni(calibration, omniPath);

            return calibration;
        }

        private static bool ReadFromOmni(Calibration calibration, string omniPath)
        {
            if (!File.Exists(omniPath)) return false;

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(omniPath);

                var root = xmlDoc.DocumentElement;
                if (root == null)
                    return false;

                var node = root.SelectSingleNode("PF_CONSTANT_PARAMETERS/CALIBRATION/PLATES");
                calibration.Plates = new List<Plate>();

                foreach (XmlElement item in node)
                {
                    var plate = new Plate();
                    
                    var nodeAttr = item?.Attributes?["INDEX"];
                    if (nodeAttr != null)
                    {
                        if (int.TryParse(nodeAttr.Value, out var index))
                            plate.Index = index;
                    }

                    nodeAttr = item?.Attributes?["REAL_TH"];
                    if (nodeAttr != null)
                    {
                        if (float.TryParse(nodeAttr.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var realThInvar))
                            plate.RealTh = realThInvar;
                        if (float.TryParse(nodeAttr.Value, NumberStyles.Float, CultureInfo.CurrentCulture, out var realTh))
                            plate.RealTh = realTh;
                    }

                    nodeAttr = item?.Attributes?["DEVIATION"];
                    if (nodeAttr != null)
                    {
                        if (float.TryParse(nodeAttr.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var deviationInvar))
                            plate.Deviation = deviationInvar;
                        if (float.TryParse(nodeAttr.Value, NumberStyles.Float, CultureInfo.CurrentCulture, out var deviation))
                            plate.Deviation = deviation;
                    }

                    calibration.Plates.Add(plate);
                }

                node = root.SelectSingleNode("PF_CONSTANT_PARAMETERS/CALIBRATION/LEVERS");
                calibration.Levers = new List<Lever>();

                foreach (XmlElement item in node)
                {
                    var lever = new Lever();

                    var nodeAttr = item?.Attributes?["VAL"];
                    if (nodeAttr != null)
                    {
                        if (int.TryParse(nodeAttr.Value, out var val))
                            lever.Val = val;
                    }

                    nodeAttr = item?.Attributes?["SIGNAL"];
                    if (nodeAttr != null)
                        lever.Signal = nodeAttr.Value;

                    calibration.Levers.Add(lever);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    internal class Lever
    {
        public int Val { get; set; }
        public string Signal { get; set; }
    }

    internal class Plate
    {
        public int Index { get; set; }
        public float RealTh { get; set; }
        public float Deviation { get; set; }
    }
}

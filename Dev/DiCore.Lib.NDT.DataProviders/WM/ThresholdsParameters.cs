using System;
using System.Collections.Generic;
using System.Xml;
using Diascan.Utils.IO;
using DiCore.Lib.NDT.Types;

namespace DiCore.Lib.NDT.DataProviders.WM
{
    public class ThresholdsParameters
    {
        public float Delta_T_Max { get; private set; }
        public float T_Max { get; private set; }
        public float Value_K { get; private set; }
        public float Value_T { get; private set; }
        public float AmplSOMinCode { get; private set; }
        public float AmplSOMaxCode { get; private set; }
        public List<Threshold> Thresholds { get; private set; }
        public static ThresholdsParameters LoadFromOmni(string omniPath)
        {
            var thresholdsParameters = new ThresholdsParameters();
            ReadFromOmni(thresholdsParameters, omniPath);
            return thresholdsParameters;
        }

        private static bool ReadFromOmni(ThresholdsParameters thresholdsParameters, string omniPath)
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

                thresholdsParameters.Delta_T_Max = 0f;
                var node = root.SelectSingleNode("WM_CONSTANT_PARAMETERS/THRESHOLDS");
                var carrierAtrr = node?.Attributes?["DELTA_T_MAX"];
                if (carrierAtrr != null)
                {
                    float deltaTMax;
                    if (Single.TryParse(ParametersHelper.DecimalSeparatorToParse(carrierAtrr.Value), out deltaTMax))
                        thresholdsParameters.Delta_T_Max = deltaTMax;
                }
                
                thresholdsParameters.T_Max = 0f;
                node = root.SelectSingleNode("WM_CONSTANT_PARAMETERS/THRESHOLDS");
                carrierAtrr = node?.Attributes?["T_MAX"];
                if (carrierAtrr != null)
                {
                    float tMax;
                    if (Single.TryParse(ParametersHelper.DecimalSeparatorToParse(carrierAtrr.Value), out tMax))
                        thresholdsParameters.T_Max = tMax;
                }   

                thresholdsParameters.Value_K = 0f;
                node = root.SelectSingleNode("WM_CONSTANT_PARAMETERS/THRESHOLDS");
                carrierAtrr = node?.Attributes?["VALUE_K"];
                if (carrierAtrr != null)
                {
                    float valueK;
                    if (Single.TryParse(ParametersHelper.DecimalSeparatorToParse(carrierAtrr.Value), out valueK))
                        thresholdsParameters.Value_K = valueK;
                }

                thresholdsParameters.Value_T = 0f;
                node = root.SelectSingleNode("WM_CONSTANT_PARAMETERS/THRESHOLDS");
                carrierAtrr = node?.Attributes?["VALUE_T"];
                if (carrierAtrr != null)
                {
                    float valueT;
                    if (Single.TryParse(ParametersHelper.DecimalSeparatorToParse(carrierAtrr.Value), out valueT))
                        thresholdsParameters.Value_T = valueT;
                }

                thresholdsParameters.AmplSOMinCode = 0f;
                node = root.SelectSingleNode("WM_CONSTANT_PARAMETERS/THRESHOLDS");
                carrierAtrr = node?.Attributes?["AMPL_SO_MIN"];
                if (carrierAtrr != null)
                {
                    float amplSOMinCode;
                    if (Single.TryParse(ParametersHelper.DecimalSeparatorToParse(carrierAtrr.Value), out amplSOMinCode))
                        thresholdsParameters.AmplSOMinCode = amplSOMinCode;
                }

                thresholdsParameters.AmplSOMaxCode = 0f;
                node = root.SelectSingleNode("WM_CONSTANT_PARAMETERS/THRESHOLDS");
                carrierAtrr = node?.Attributes?["AMPL_SO_MAX"];
                if (carrierAtrr != null)
                {
                    float amplSOMaxCode;
                    if (Single.TryParse(ParametersHelper.DecimalSeparatorToParse(carrierAtrr.Value), out amplSOMaxCode))
                        thresholdsParameters.AmplSOMaxCode = amplSOMaxCode;
                }

                thresholdsParameters.Thresholds = new List<Threshold>();
                node = root.SelectSingleNode("WM_CONSTANT_PARAMETERS/THRESHOLDS");

                var startThIndex = 0;
                    while (true)
                    {
                        float time; int ampl;
                        startThIndex++;

                        carrierAtrr = node?.Attributes?[$"THRESHOLD_{startThIndex}_TIME"];
                      
                        if (carrierAtrr == null)
                            break;

                        if (!Single.TryParse(ParametersHelper.DecimalSeparatorToParse(carrierAtrr.Value), out time))
                            break;

                        carrierAtrr = node?.Attributes?[$"THRESHOLD_{startThIndex}_AMPL"];
                       
                        if (carrierAtrr == null)
                            break;

                        if (!Int32.TryParse(carrierAtrr.Value, out ampl))
                                break;

                        thresholdsParameters.Thresholds.Add(new Threshold{Ampl = ampl, Time = time});
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        internal static bool CheckThresholds(IEnumerable<Threshold> thresholds)
        {
            var summaryAmpl = 0;
            var summaryTime = 0f;

            foreach (var threshold in thresholds)
            {
                summaryAmpl += threshold.Ampl;
                summaryTime += threshold.Time;
            }

            if (summaryAmpl <= 0)
                return false;

            return summaryTime > 0.1f;
        }
    }

    public class Threshold
    {
        public float Time { get; set; }
        public int Ampl { get; set; }
    }

}

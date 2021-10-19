using System;
using System.Collections.Generic;
using System.Xml;
using Diascan.Utils.IO;

namespace DiCore.Lib.NDT.DataProviders.EMA
{
    public class EmaTimeRulesLoad
    {
        public static List<EmaTimeRule> LoadFromOmni(string omniPath)
        {
            var result = new List<EmaTimeRule>();
            ReadFromOmni(result, omniPath);
            return result;
        }

        private static void ReadFromOmni(List<EmaTimeRule> emaRules, string omniPath)
        {
            if (!File.Exists(omniPath)) return ;

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(omniPath);

                var root = xmlDoc.DocumentElement;
                if (root == null)
                    return;

                var node = root.SelectSingleNode("EMA_CONSTANT_PARAMETERS/THRESHOLDS");

                foreach (XmlElement item in node)
                {
                    var rule = new EmaTimeRule();

                    var nodeAttr = item?.Attributes?["ID_RULE"];
                    if (nodeAttr != null)
                        if (int.TryParse(nodeAttr.Value, out var id))
                            rule.Id = id >= 0 ? (EmaRuleEnum)Math.Pow(2, id) : EmaRuleEnum.NC;
                        else
                            rule.Id = EmaRuleEnum.NC;

                    nodeAttr = item?.Attributes?["RULE_NAME"];
                    if (nodeAttr != null)
                        if (string.IsNullOrEmpty(nodeAttr.Value))
                            rule.Name = string.Empty;
                        else
                            rule.Name = nodeAttr.Value;

                    nodeAttr = item?.Attributes?["TIME_DISCRET"];
                    if (nodeAttr != null)
                        if (float.TryParse(nodeAttr.Value, out var timeDiscrete))
                            rule.TimeDiscrete = timeDiscrete;
                        else
                            rule.TimeDiscrete = 0.0f;

                    nodeAttr = item?.Attributes?["TIME_SLEEP_ZONE"];
                    if (nodeAttr != null)
                        if (float.TryParse(nodeAttr.Value, out var timeSleepZone))
                            rule.TimeSleepZone = timeSleepZone;
                        else
                            rule.TimeSleepZone = 0.0f;

                    emaRules.Add(rule);
                }
            }
            catch (Exception) { }
        }
    }

    public class EmaTimeRule
    {
        public EmaRuleEnum Id            { get; set; }
        public string      Name          { get; set; }
        public float       TimeDiscrete  { get; set; }
        public float       TimeSleepZone { get; set; }
    }
}
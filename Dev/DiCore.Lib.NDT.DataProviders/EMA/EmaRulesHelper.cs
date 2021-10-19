using System;
using System.Collections.Generic;
using System.Xml;
using Diascan.Utils.IO;

namespace DiCore.Lib.NDT.DataProviders.EMA
{
    internal class EmaRulesHelper
    {
        public static List<EmaRule> LoadFromOmni(string omniPath)
        {
            var result = new List<EmaRule>();
            ReadFromOmni(result, omniPath);
            return result;
        }

        private static bool ReadFromOmni(List<EmaRule> emaRules, string omniPath)
        {
            if (!File.Exists(omniPath)) return false;

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(omniPath);

                var root = xmlDoc.DocumentElement;
                if (root == null)
                    return false;

                var node = root.SelectSingleNode("EMA_CONSTANT_PARAMETERS/EMA_RULES");
                
                foreach (XmlElement item in node)
                {
                    var rule = new EmaRule();

                    var nodeAttr = item?.Attributes?["ID"];
                    if (nodeAttr != null)
                    {
                        if (int.TryParse(nodeAttr.Value, out var id))
                            rule.Id = id >= 0 ? (EmaRuleEnum)Math.Pow(2, id) : EmaRuleEnum.NC;
                    }

                    nodeAttr = item?.Attributes?["DISTANCE"];
                    if (nodeAttr != null)
                    {
                        if (float.TryParse(nodeAttr.Value, out var dist))
                            rule.Distance = dist;
                    }

                    emaRules.Add(rule);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    internal class EmaRule
    {
        public EmaRuleEnum Id { get; set; }
        public float Distance { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Xml;
using Diascan.Utils.IO;
using DiCore.Lib.NDT.DataProviders.CDM;
using DiCore.Lib.NDT.Types;

namespace DiCore.Lib.NDT.DataProviders.CDPA
{
    public class CDpaDirectionHelper
    {
        private static CDpaDirection Create(int id, float angle)
        {
            var directionName = enDirectionName.None;

            if (MathHelper.TestFloatEquals(angle, 0f) || MathHelper.TestFloatEquals(angle, 180f))
            {
                directionName = enDirectionName.Cdc;
            }
            else if (MathHelper.TestFloatEquals(angle, 30f) || MathHelper.TestFloatEquals(angle, 210f))
            {
                directionName = enDirectionName.Cds;
            }
            else if (MathHelper.TestFloatEquals(angle, 60f))
            {
                directionName = enDirectionName.Cdh;
            }
            else if (MathHelper.TestFloatEquals(angle, 90f) || MathHelper.TestFloatEquals(angle, 270f))
            {
                directionName = enDirectionName.Cdl;
            }
            else if (MathHelper.TestFloatEquals(angle, 150f))
            {
                directionName = enDirectionName.Cdg;
            }
            else if (MathHelper.TestFloatEquals(angle, 300f))
            {
                directionName = enDirectionName.Cdf;
            }

            return new CDpaDirection(id, angle) { DirectionName = directionName };
        }

        internal static List<CDpaDirection> LoadFromOmni(string omniPath)
        {
            var result = new List<CDpaDirection>();
            ReadFromOmni(result, omniPath);
            return result;
        }

        private static bool ReadFromOmni(List<CDpaDirection> result, string omniPath)
        {
            if (!File.Exists(omniPath)) return false;

            var rootNodePath = $"RUN/CDF_CONSTANT_PARAMETERS/DIRECTIONS/DIRECTION";

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
                    var attribute = node?.Attributes?["VALUE"];
                    if (attribute == null) continue;
                    if (!Single.TryParse(ParametersHelper.DecimalSeparatorToParse(attribute.Value), out var value))
                        continue;

                    attribute = node.Attributes?["ANGLE2_ID"];
                    if (attribute == null) continue;
                    if (!Int32.TryParse(attribute.Value, out var angleId)) continue;

                    result.Add(Create(angleId, value));
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
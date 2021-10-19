using DiCore.Lib.NDT.Types;

namespace DiCore.Lib.NDT.CoordinateProvider
{
    public static class DistRulerHelper
    {
        public static bool IsEmpty(this DistRuleItem ruleItem)
        {
            return (ruleItem.Scan == -1) && MathHelper.TestFloatEquals(ruleItem.Dist, -1d) && (ruleItem.Time == 0);
        }
    }
}

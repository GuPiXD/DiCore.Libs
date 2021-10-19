using System.Collections.Generic;

namespace DiCore.Lib.NDT.DataProviders.WM
{
    public class Calibrations
    {
        public Calibration[] Values;
    }

    public class Calibration
    {
        public double Distance;

        public float[] SensorValueOffset;
    }

    internal static class WtCorrection
    {
        private static Dictionary<int, float> _wtCorrections
            = new Dictionary<int, float>
            {
                {16, -0.2f},
                {277, 0.2f},
                {1477, 0.2f},
                {1677, 0.2f},
                {2016, -0.2f},
                {2077, 0.2f},
                {2816, -0.2f},
                {3016, -0.2f},
                {3216, -0.2f},
                {4016, -0.2f},
                {4216, -0.2f},
                {4817, -0.2f},
                {602077, 0.2f},
                {803077, 0.2f},
                {1405077, 0.2f},
                {1606077, 0.2f},
                {2008077, 0.2f},
                {2812077, 0.2f},
                {3214077, 0.2f},
                {4018077, 0.2f},
                {4219077, 0.2f},
                {4822077, 0.2f},
                {100400003, 0.2f},
                {100400004, 0.2f},
                {120400003, 0.2f},
                {120400004, 0.2f},
                {140400002, 0.2f},
                {140400003, 0.2f},
                {160400002, 0.2f},
                {160400003, 0.2f},
                {200400002, 0.2f},
                {240400008, 0.2f},
                {280400102, 0.2f},
                {320400102, 0.2f}
            };

        internal static float GetWtCorrectionByCarrierId(int carrierId)
        {
            var wtCorrection = 0f;
            if (_wtCorrections.ContainsKey(carrierId))
            {
                wtCorrection = _wtCorrections[carrierId];
            }

            return wtCorrection;
        }
    }
}

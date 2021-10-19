using System;

namespace DiCore.Lib.NDT.Types
{
    public static class MathHelper
    {
        public static bool TestFloatEquals(double a, double b)
        {
            return Math.Abs(a - b) < double.Epsilon;
        }

        public static bool TestFloatEquals(double a, double b, double epsilon)
        {
            return Math.Abs(a - b) < epsilon;
        }

        public static bool TestDoubleEquals(double a, double b, double epsilon = double.Epsilon)
        {
            return Math.Abs(a - b) < epsilon;
        }

        public static bool IntersectPoints(double a, double b, double epsilon)
        {
            return Math.Abs(a - b) <= epsilon;
        }

        public static double AngleToRadian(double value)
        {
            return value * Math.PI / 180;
        }

        public static double RadianToAngle(double value)
        {
            return value * 180 / Math.PI;
        }
    }
}

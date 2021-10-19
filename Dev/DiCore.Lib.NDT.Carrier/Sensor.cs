using System;

namespace DiCore.Lib.NDT.Carrier
{
    public class Sensor : IEquatable<Sensor>
    {
        public bool Primary { get; set; }
        public short LogicalNumber { get; set; }
        public short PhysicalNumber { get; set; }
        public short OpposedLogicalNumber { get; set; }
        public short GroupNumber { get; set; }
        public short SkiNumber { get; set; }
        public int Delay { get; set; }
        public float Dx { get; set; }
        public float Dy { get; set; }
        public float Angle { get; set; }
        public float Angle2 { get; set; }
        public int Bodynum { get; set; }

        public float SinAngle2 { get; set; }
        public float CosAngle2 { get; set; }
        public byte DirectionCode { get; set; }
        public short DirectionInitialCode { get; set; }

        public bool Equals(Sensor other)
        {
            if (Object.ReferenceEquals(other, null)) return false;

            if (Object.ReferenceEquals(this, other)) return true;

            return DirectionInitialCode.Equals(other.DirectionInitialCode);
        }

        public override int GetHashCode()
        {
            return DirectionInitialCode.GetHashCode();
        }
    }
}

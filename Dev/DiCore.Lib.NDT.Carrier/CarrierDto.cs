using System;

namespace DiCore.Lib.NDT.Carrier
{
    public class CarrierDto
    {
        public Guid Id { get; set; }
        public Guid ClassId { get; set; } = Loader.SensorCarrierId;
        public int OldId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public float Circle { get; set; }
        public byte PigType { get; set; }
        public byte Diametr { get; set; }
        public float? MinSpeed { get; set; }
        public float? MaxSpeed { get; set; }
        public bool? Polymorph { get; set; }
        public SensorDto[] Items { get; set; }
    }
}

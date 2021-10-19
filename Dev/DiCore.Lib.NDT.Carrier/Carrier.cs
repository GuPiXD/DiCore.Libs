using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DiCore.Lib.NDT.Carrier
{
    public class Carrier : IEnumerable<Sensor>
    {
        public Guid ID { get; protected set; }
        public int OldId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public float Circle { get; set; }
        public byte PigType { get; set; }
        public byte Diametr { get; set; }
        public float? MinSpeed { get; set; }
        public float? MaxSpeed { get; set; }
        public bool? Polymorph { get; set; }

        private readonly Sensor[] m_sensors = new Sensor[0];

        public Carrier(Guid id)
        {
            ID = id;
        }

        public Carrier(int oldId)
        {
            OldId = oldId;
        }

        public Carrier(Guid id, Sensor[] items)
        {
            ID = id;
            m_sensors = items.OrderBy(item => item.LogicalNumber).ToArray();
        }

        public Carrier() :
            this(0)
        { }

        public Sensor this[int index]
        {
            get => m_sensors[index];
            set => m_sensors[index] = value;
        }

        public short SensorCount => (short)m_sensors.Length;

        #region Implementation of IEnumerable

        public IEnumerator<Sensor> GetEnumerator()
        {
            return ((IEnumerable<Sensor>)m_sensors).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}

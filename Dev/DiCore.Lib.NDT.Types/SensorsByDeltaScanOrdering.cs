using System.Collections.Generic;
using System.Linq;
using DiCore.Lib.NDT.Carrier;

namespace DiCore.Lib.NDT.Types
{
    public class SensorsByDeltaScanOrdering
    {
        private readonly List<SensorsByDeltaScanOrderingContainer> items = new List<SensorsByDeltaScanOrderingContainer>();

        public List<SensorsByDeltaScanOrderingContainer> Items => items;

        public SensorsByDeltaScanOrdering(IEnumerable<Sensor> carrier, bool all = false)
        {
            var sensors = (from sensor in carrier orderby sensor.Dy  select sensor).ToArray();

            foreach (var sensor in sensors)
            {
                var createNew = true;

                var localSensor = sensor;
                if (!all)
                {
                    foreach (var t in items.Where(t => MathHelper.TestFloatEquals(localSensor.Dy, t.Dy))
                            .Where(t => localSensor.Delay == t.Delay))
                    {
                        t.SensorNumbers.Add(sensor.LogicalNumber - 1);
                        createNew = false;
                        break;
                    }
                }
                if (!createNew) continue;

                items.Add(new SensorsByDeltaScanOrderingContainer(sensor.Dy, sensor.Delay, sensor.LogicalNumber - 1));
            }
        }
    }

    public class SensorsByDeltaScanOrderingContainer
    {
        public float Dy { get; }
        public int Delay { get; }
        public int DeltaScan { get; set; }
        public int AdditionalAligmentInputInDeltaScan { get; set; }
        public readonly List<int> SensorNumbers;

        public SensorsByDeltaScanOrderingContainer(float dy, int delay, int sensorNumber)
        {
            SensorNumbers = new List<int> { sensorNumber };
            Dy = dy;
            Delay = delay;
            DeltaScan = 0;
            AdditionalAligmentInputInDeltaScan = 0;
        }
    }
}

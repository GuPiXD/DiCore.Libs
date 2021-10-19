using System.Runtime.Serialization;

namespace DiCore.Lib.NDT.DataProviders.EMA
{
    [DataContract]
    internal class EmaCalibration
    {
        [DataMember]
        public float Value { get; set; }
        [DataMember]
        public int Sensor { get; set; }

        public EmaCalibration(float value, int sensor)
        {
            Value = value;
            Sensor = sensor;
        }
    }
}

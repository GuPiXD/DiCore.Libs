using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiCore.Lib.NDT.Types
{
    public interface ICoordinateProvider
    {
        float GetAngle(float targetSensorAngleMm, float sensorTdcAngleMm, double dist);
        float AngleOffset(float comparerAngleMm, double dist);        
    }
}

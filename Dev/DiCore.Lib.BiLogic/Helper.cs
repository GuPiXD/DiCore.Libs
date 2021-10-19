using System;
using System.Collections.Generic;

namespace DiCore.Lib.BiLogic
{
    public static class Helper
    {
        /// <summary>
        /// Получение списка примыканий ко швам (продольному и/или поперечнему)
        /// </summary>
        /// <param name="artifactLocation">Описание расположения артефакта в пределах трубной секции</param>
        /// <param name="distanceCriteria">Критерий примыкания ко шву (мм)</param>
        /// <returns>Список фактических примыканий</returns>
        public static List<WeldTypes> GetJoiningToWelds(this ArtifactLocation artifactLocation, double distanceCriteria)
        {
            var result = new List<WeldTypes>(2);

            var minLengthToCrossSeam = Math.Min(artifactLocation.Distance,
                artifactLocation.ParentPipeLength - artifactLocation.Distance -
                artifactLocation.MeasuredLength / 1000d)*1000;

            if (minLengthToCrossSeam <= distanceCriteria)
                result.Add(WeldTypes.Cross);

            if (!artifactLocation.ParentPipeAngleFirst.HasValue) return result;

            if (ProcessLongitudinalWeld(artifactLocation, artifactLocation.ParentPipeAngleFirst.Value) <
                distanceCriteria)
            {
                result.Add(WeldTypes.Longitudinal);
                return result;
            }

            if (!artifactLocation.ParentPipeAngleSecond.HasValue) return result;

            if (ProcessLongitudinalWeld(artifactLocation, artifactLocation.ParentPipeAngleSecond.Value) < distanceCriteria)
                result.Add(WeldTypes.Longitudinal);

            return result;
        }

        private static double ProcessLongitudinalWeld(ArtifactLocation artifactLocation, float longitudinalWeldAngle)
        {
            var artifactAngularWidth = 360f*artifactLocation.MeasuredWidth/
                                       (artifactLocation.ParentPipeConstructiveDiameterMm*Math.PI);

            var topToFirstWeldDistance = (longitudinalWeldAngle - artifactLocation.AngularPosition +
                                          360)%360;

            if (topToFirstWeldDistance < artifactAngularWidth)
                return 0d;

            var bottomToFirstWeldDistance = (longitudinalWeldAngle -
                                             artifactLocation.AngularPosition - artifactAngularWidth +
                                             360) % 360;

            topToFirstWeldDistance = topToFirstWeldDistance > 180
                ? 360 - topToFirstWeldDistance
                : topToFirstWeldDistance;

            bottomToFirstWeldDistance = bottomToFirstWeldDistance > 180
                ? 360 - bottomToFirstWeldDistance
                : bottomToFirstWeldDistance;

            return Math.Min(topToFirstWeldDistance, bottomToFirstWeldDistance) / 360d *
                   (artifactLocation.ParentPipeConstructiveDiameterMm * Math.PI);
        }
    }
}

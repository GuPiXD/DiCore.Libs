using System;

namespace DiCore.Lib.TestModels.Models
{
    public class ArtifactAddonMeasured
    {
        public Guid Id { get; set; }
        public float? AngularPosition { get; set; }
        public float? KeypointAngularPosition { get; set; }
        public int? MeasuredLength { get; set; }
        public short? MeasuredWidth { get; set; }
        public float? MeasuredDepth { get; set; }
        public float? FeatureWallThickness { get; set; }
        public int? EffectiveLength { get; set; }
        public short? EffectiveWidth { get; set; }
        public SurfaceLocation SurfaceLocation { get; set; }
    }
}
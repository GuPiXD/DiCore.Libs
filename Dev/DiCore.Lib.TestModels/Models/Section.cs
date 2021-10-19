using System;

namespace DiCore.Lib.TestModels.Models
{
    public class Section
    {
        public Guid Id { get; set; }
        public Guid DiagnosticTargetId { get; set; }
        public double Distance { get; set; }
        public int? Number { get; set; }
        public float Length { get; set; }
        //public int SectionTypeId { get; set; }
        public SectionType SectionType { get; set; }
        public float? AverageWallThickness { get; set; }
        public float? AxialWeldStartAngle { get; set; }
        public float? AxialWeldEndAngle { get; set; }
        public Guid PipelineSectionId { get; set; }
       // public int PipeTypeId { get; set; }
        public PipeType PipeType { get; set; }
        public float? Altitude { get; set; }
        public float? AxialWeldSecondAngle { get; set; }
        public string Data { get; set; }
    }

    public class SectionCrop
    {
        public Guid Id { get; set; }
        public Guid DiagnosticTargetId { get; set; }
        public double Distance { get; set; }
        public int? Number { get; set; }
        public float Length { get; set; }
        public int SectionTypeId { get; set; }
        public Guid PipelineSectionId { get; set; }
        public int PipeTypeId { get; set; }
    }

    public class SectionWithArtifacts : Section
    {
        public Artifact[] Artifacts { get; set; }
    }
}

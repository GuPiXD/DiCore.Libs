using System;

namespace DiCore.Lib.TestModels.Models
{
    public class ArtifactClass
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int? Code { get; set; }
        public Guid? DefectTypeId { get; set; }
        public Guid? EquipmentClassId { get; set; }
        public Guid? SchemaId { get; set; }
    }
}

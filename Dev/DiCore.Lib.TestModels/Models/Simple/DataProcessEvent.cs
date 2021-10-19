using System;
using System.Collections.Generic;
using System.Text;

namespace DiCore.Lib.TestModels.Models.Simple
{
    public class DataProcessEvent
    {
        public Guid Id { get; set; }
        public DateTime DateCreate { get; set; }
        public Guid ParentId { get; set; }
        public string Description { get; set; }
        public Guid CreatorId { get; set; }
        public EventType EventType { get; set; }
        public string Parameters { get; set; }
    }
}

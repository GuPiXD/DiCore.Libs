using System;
using System.Collections.Generic;
using System.Text;
using NS = Newtonsoft.Json.Linq;

namespace DiCore.Lib.TestModels.Models.JObject
{
    public class DataProcessEvent
    {
        public Guid Id { get; set; }
        public DateTime DateCreate { get; set; }
        public Guid? ParentId { get; set; }
        public string Description { get; set; }
        public Guid CreatorId { get; set; }
        public EventType EventType { get; set; }
        public NS.JObject Parameters { get; set; }
    }
}

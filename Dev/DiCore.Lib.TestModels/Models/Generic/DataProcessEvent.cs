using System;
using System.Collections.Generic;
using System.Text;

namespace DiCore.Lib.TestModels.Models.Generic
{
    public class DataProcessEvent<TJson>
    {
        public Guid Id { get; set; }
        public DateTime DateCreate { get; set; }
        public Guid? ParentId { get; set; }
        public string Description { get; set; }
        public Guid CreatorId { get; set; }
        public EventType EventType { get; set; }
        public TJson Parameters { get; set; }
    }
}

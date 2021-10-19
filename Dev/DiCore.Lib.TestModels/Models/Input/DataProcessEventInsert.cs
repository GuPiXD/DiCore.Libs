using System;
using System.Collections.Generic;
using System.Text;

namespace DiCore.Lib.TestModels.Models.Input
{
    public class DataProcessEventInsert<TJson>
    {
        public DateTime DateCreate { get; set; }
        public Guid? ParentId { get; set; }
        public string Description { get; set; }
        public Guid? CreatorId { get; set; }
        public int EventTypeId { get; set; }
        public TJson Parameters { get; set; }
    }
}

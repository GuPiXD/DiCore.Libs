using System;
using System.Collections.Generic;

namespace DiCore.Lib.RestClient.TestCore.Api.Models
{
    public class TestDataManyInner
    {
        public string Name { get; set; }
        public int Len { get; set; }
        public double Angle { get; set; }
        public Guid Uid { get; set; }
        public bool Flag { get; set; }
        public DateTime Timespan { get; set; }
        public Dictionary<string, string> Files { get; set; }
        public string[] StringArray { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using NS = Newtonsoft.Json.Linq;

namespace DiCore.Lib.TestModels.Models.JObject
{
    public class Token
    {
        public Guid Id { get; set; }
        public DataProcessEvent DataProcessEvent { get; set; }
        public NS.JObject Data { get; set; }
    }
}

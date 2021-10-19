using System;
using System.Collections.Generic;
using System.Text;

namespace DiCore.Lib.TestModels.Models.Simple
{
    public class Token
    {
        public Guid Id { get; set; }
        public DataProcessEvent DataProcessEvent { get; set; }
        public string Data { get; set; }
    }
}

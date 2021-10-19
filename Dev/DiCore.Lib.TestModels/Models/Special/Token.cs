using DiCore.Lib.TestModels.Models.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiCore.Lib.TestModels.Models.Special
{
    public class Token
    {
        public Guid Id { get; set; }
        public DataProcessEvent DataProcessEvent { get; set; }
        public TokenDataJson Data { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace DiCore.Lib.TestModels.Models.Generic
{
    public class Token<TJson, TDPEJson>
    {
        public Guid Id { get; set; }
        public DataProcessEvent<TDPEJson> DataProcessEvent { get; set; }
        public TJson Data { get; set; }
    }
}

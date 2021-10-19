using System;
using System.Collections.Generic;
using System.Text;

namespace DiCore.Lib.TestModels.Models.Input
{
    public class TokenInsert<TJson>
    {
        public Guid DataProcessEventId { get; set; }

        public TJson Data { get; set; }
    }
}

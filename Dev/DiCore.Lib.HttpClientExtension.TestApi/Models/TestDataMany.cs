using System;
using System.Collections.Generic;

namespace DiCore.Lib.RestClient.TestCore.Api.Models
{
    public class TestDataMany : TestDataManyInner
    {
        public TestDataManyInner InnerModel { get; set; }
    }
}
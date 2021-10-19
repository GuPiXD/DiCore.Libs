using System;
using System.Collections.Generic;
using System.Xml;
using Diascan.Utils.IO;
using DiCore.Lib.NDT.Types;
namespace DiCore.Lib.NDT.DataProviders.CDPA
{
    internal class CDpaRuleParameters
    {
        public string StrId { get; set; }

        public short Id => short.Parse(StrId);

        public float FocusDepth { get; set; }

        public float AngleInMetal { get; set; }

        public float AngleInProduct { get; set; }
    }
}
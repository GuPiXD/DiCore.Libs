using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiCore.Lib.PgSqlAccess.Test.SharedModel
{

    public class FullModel
    {
        public int Id { get; set; }
        public int Value1 { get; set; }
        public int Value2 { get; set; }
        public int Value3 { get; set; }
        public int Value4 { get; set; }
    }

    public class FullModelL
    {
        public int Id { get; set; }
        public int Value1 { get; set; }
        public int Value2 { get; set; }
    }

    public class FullModelLR
    {
        public int Id { get; set; }
        public int Value3 { get; set; }
        public int Value1 { get; set; }
    }

    public class FullModelTM
    {
        public int Id { get; set; }
        public int Value1 { get; set; }
        public int Value2 { get; set; }
        public int Value3 { get; set; }
        public int Value4 { get; set; }
        public int Value5 { get; set; }
        public int Value6 { get; set; }
    }

}

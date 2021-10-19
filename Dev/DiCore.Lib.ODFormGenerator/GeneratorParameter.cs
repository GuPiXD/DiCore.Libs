using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiCore.Lib.ODFormGenerator
{
    public class GeneratorParameter
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Format { get; set; }
        public string Unit { get; set; }
        public string Range { get; set; }
        public string Comment { get; set; }
        public string DirectoryName { get; set; }
    }
}

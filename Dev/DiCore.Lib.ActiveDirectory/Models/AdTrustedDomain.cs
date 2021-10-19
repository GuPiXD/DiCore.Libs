using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiCore.Lib.ActiveDirectory.Models
{
    public class AdTrustedDomain
    {
        public string DnsName { get; set; }
        public string NetBiosName { get; set; }
    }
}

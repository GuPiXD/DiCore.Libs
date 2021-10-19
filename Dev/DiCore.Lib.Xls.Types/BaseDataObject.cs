using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiCore.Lib.Xls.Types
{
    public class BaseDataObject
    {
        public virtual object ToThisType()
        {
            return this;
        }
    }
}

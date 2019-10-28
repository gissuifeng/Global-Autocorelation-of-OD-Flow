using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autocorrelation_Flow_Demo01.Lib
{
    public class WCValueSet
    {
        public double wcValue;
        public double wValue;
        public double cValue;

        public WCValueSet(double wcValue, double wValue, double cValue)
        {
            this.wcValue = wcValue;
            this.wValue = wValue;
            this.cValue = cValue;
        }
    }
}

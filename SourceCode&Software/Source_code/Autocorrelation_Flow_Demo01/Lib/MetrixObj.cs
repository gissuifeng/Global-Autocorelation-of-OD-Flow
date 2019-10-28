using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autocorrelation_Flow_Demo01.Lib
{
    public class MetrixObj
    {
        public int SourceID;
        public int NearID;

        public MetrixObj(int sourceID, int nearID)
        {
            SourceID = sourceID;
            NearID = nearID;
        }

        public override string ToString()
        {
            return "oid: " + SourceID + ", did: " + NearID;
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autocorrelation_Flow_Demo01.Lib
{
    public class ODObj
    {
        public int fid;
        public int oid;
        public int did;
        public int val;

        public ODObj(int fid, int oid, int did, int val)
        {
            this.fid = fid;
            this.oid = oid;
            this.did = did;
            this.val = val;
        }

        public override string ToString()
        {
            return "oid: " + oid + ", did: " + did;
        }
    }
}

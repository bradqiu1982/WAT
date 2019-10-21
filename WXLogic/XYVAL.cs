using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WXLogic
{
    public class XYVAL
    {
        public XYVAL()
        {
            X = "";
            Y = "";
            Val = 0;
            unit = 0;
        }

        public XYVAL(string x, string y,string u, double v)
        {
            X = x;
            Y = y;
            Val = v;
            unit = UT.O2I(u);
        }

        public string X { set; get; }
        public string Y { set; get; }
        public double Val { set; get; }
        public int unit { set; get; }
    }
}

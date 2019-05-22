using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class WATPassFailUnit : WATProbeTestData
    {
        public static List<WATPassFailUnit> GetPFUnitData(string RP, string DCDName,List<SpecBinPassFail> speclist,List<WATProbeTestDataFiltered> filterdata,List<WATCouponStats> coupondata)
        {
            var ret = new List<WATPassFailUnit>();
            return ret;
        }
        
        public string ParamName { set; get; }
        public string Eval_PN { set; get; }
        public string Bin_PN { set; get; }
        public string DCDName { set; get; }
        public string UpperLimit { set; get; }
        public string LowLimit { set; get; }
        public string FailType { set; get; }
    }
}
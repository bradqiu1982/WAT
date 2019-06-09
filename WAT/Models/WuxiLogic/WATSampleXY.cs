using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class WATSampleXY
    {
        public static List<WATSampleXY> GetSampleXYByCouponGroup(string coupongroup)
        {
            var ret = new List<WATSampleXY>();
            return ret;
        }

        public string CouponID { set; get; }
        public string ChannelInfo { set; get; }
        public string X { set; get; }
        public string Y { set; get; }
    }
}
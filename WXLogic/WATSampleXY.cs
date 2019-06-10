using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WXLogic
{
    public class WATSampleXY
    {
        public static List<WATSampleXY> GetSampleXYByCouponGroup(string coupongroup)
        {
            var ret = new List<WATSampleXY>();
            var watcfg = WXCfg.GetSysCfg();
            if (watcfg.ContainsKey("OGPCONNSTR") && watcfg.ContainsKey("OGPSQLSTR"))
            {
                try {
                    var ogpconn = DBUtility.GetConnector(watcfg["OGPCONNSTR"]);
                    var sql = watcfg["OGPSQLSTR"];
                    sql = sql.Replace("<coupongroup>", coupongroup);

                    var dbret = DBUtility.ExeSqlWithRes(ogpconn, sql);
                    DBUtility.CloseConnector(ogpconn);

                    foreach (var line in dbret)
                    {
                        var tempvm = new WATSampleXY();
                        tempvm.CouponID = UT.O2S(line[0]);
                        tempvm.ChannelInfo = UT.O2S(line[1]);
                        tempvm.X = UT.O2S(line[2]);
                        tempvm.Y = UT.O2S(line[3]);
                        ret.Add(tempvm);
                    }
                } catch (Exception ex) { }
            }

            return ret;
        }

        WATSampleXY()
        {
            CouponID = "";
            ChannelInfo = "";
            X = "";
            Y = "";
        }

        public string CouponID { set; get; }
        public string ChannelInfo { set; get; }
        public string X { set; get; }
        public string Y { set; get; }
    }
}
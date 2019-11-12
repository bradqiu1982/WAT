using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WAT.Models
{
    public class WATOGPVM
    {
        public static List<string> GetAllOGPWafers(Controller ctrl)
        {
            var retdict = new Dictionary<string, bool>();
            var syscfg = CfgUtility.GetSysConfig(ctrl);
            var ogpconn = DBUtility.GetConnector(syscfg["OGPCONNSTR"]);
            var sql = "  select distinct SN from [AIProjects].[dbo].[CouponData] where len(SN) > 13";
            var dbret = DBUtility.ExeSqlWithRes(ogpconn, sql);
            DBUtility.CloseConnector(ogpconn);

            foreach (var line in dbret)
            {
                var sn = UT.O2S(line[0]);
                sn = sn.Substring(0, sn.Length - 5);
                if (!retdict.ContainsKey(sn))
                { retdict.Add(sn, true); }
            }

            return retdict.Keys.ToList();
        }

        public static List<object> GetOGPData(string wafer, Controller ctrl)
        {
            var ret = new List<object>();
            var syscfg = CfgUtility.GetSysConfig(ctrl);
            var ogpconn = DBUtility.GetConnector(syscfg["OGPCONNSTR"]);
            var sql = "select SN,[Index],X,Y from [AIProjects].[dbo].[CouponData] where SN like '<wafer>%'";
            sql = sql.Replace("<wafer>", wafer);

            var dbret = DBUtility.ExeSqlWithRes(ogpconn, sql);
            DBUtility.CloseConnector(ogpconn);

            foreach (var line in dbret)
            {
                ret.Add(new
                {
                    CouponID = UT.O2S(line[0]),
                    ChannelInfo = UT.O2S(line[1]),
                    X = UT.O2S(line[2]),
                    Y = UT.O2S(line[3])
                });
            }

            return ret;
        }
    }
}
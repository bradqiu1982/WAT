using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WAT.Models
{
    public class WuxiWATData4MG
    {
        public static List<WuxiWATData4MG> GetData(string coupongroup1,Controller ctrl)
        {
            var syscfg = CfgUtility.GetSysConfig(ctrl);


            var ret = new List<WuxiWATData4MG>();

            var CouponGroup = "";
            if (coupongroup1.Length < 12)
            {
                return ret;
            }
            else
            {
                CouponGroup = coupongroup1.Substring(0, 12).ToUpper();
            }

            var oringaldata = WXLogic.WXOriginalWATData.GetData(CouponGroup);
            var wafernum = CouponGroup.Split(new string[] { "E", "e" }, StringSplitOptions.RemoveEmptyEntries)[0];
            var probedata = WXLogic.WXProbeData.GetData(wafernum);

            var probedict = new Dictionary<string, WXLogic.WXProbeData>();
            foreach (var item in probedata)
            {
                var key = item.X + ":" + item.Y;
                if (!probedict.ContainsKey(key))
                {
                    probedict.Add(key, item);
                }
            }

            var ignoredict = WXWATIgnoreDie.RetrieveIgnoreDieDict(wafernum);

            foreach (var item in oringaldata)
            {
                ret.Add(new WuxiWATData4MG(item, probedict,syscfg,ignoredict));
            }

            ret.Sort(delegate (WuxiWATData4MG obj1, WuxiWATData4MG obj2)
            {
                if (obj1.RP > obj2.RP)
                { return 1; }
                else if (obj1.RP < obj2.RP)
                { return -1; }
                else {
                    if (string.Compare(obj1.CouponID, obj2.CouponID) > 0)
                    { return 1; }
                    else if (string.Compare(obj1.CouponID, obj2.CouponID) < 0)
                    { return -1; }
                    else
                    {
                        var ch1 = UT.O2I(obj1.CH);
                        var ch2 = UT.O2I(obj2.CH);
                        return ch1.CompareTo(ch2);
                    } 
                }
            });

            return ret;
        }

        private string ValueCheck(string name, string val, Dictionary<string, string> specdict)
        {
            if (specdict.ContainsKey(name))
            {
                var lowhigh = specdict[name].Split(new string[] { ",", ":" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                var dval = UT.O2D(val);
                var low = UT.O2D(lowhigh[0]);
                var high = UT.O2D(lowhigh[1]);
                if (dval < low || dval > high)
                { return "FAILDATA"; }
                else
                { return string.Empty; }
            }
            else
            { return string.Empty; }
        }

        public WuxiWATData4MG(WXLogic.WXOriginalWATData item, Dictionary<string, WXLogic.WXProbeData> probedict
            ,Dictionary<string,string> specdict,Dictionary<string,WXWATIgnoreDie> ignoredict)
        {
            var key = item.X + ":" + item.Y;

            CouponID = item.Containername;
            CH = item.ChannelInfo;
            X = item.X;
            Y = item.Y;
            BVR_LD_A = UT.O2D(item.BVR_LD_A).ToString();
            BVR_LD_A_ST = ValueCheck("BVR_LD_A", item.BVR_LD_A, specdict);

            PO_LD_W = UT.O2D(item.PO_LD_W).ToString();
            PO_LD_W_ST = ValueCheck("PO_LD_W", item.PO_LD_W, specdict);

            VF_LD_V = UT.O2D(item.VF_LD_V).ToString();
            VF_LD_V_ST = ValueCheck("VF_LD_V", item.VF_LD_V, specdict);

            SLOPE_WperA = UT.O2D(item.SLOPE_WperA).ToString();
            SLOPE_WperA_ST = ValueCheck("SLOPE_WperA", item.SLOPE_WperA, specdict);

            THOLD_A = UT.O2D(item.THOLD_A).ToString();
            THOLD_A_ST = ValueCheck("THOLD_A", item.THOLD_A, specdict);

            R_LD_ohm = UT.O2D(item.R_LD_ohm).ToString();
            R_LD_ohm_ST = ValueCheck("R_LD_ohm", item.R_LD_ohm, specdict);

            IMAX_A = UT.O2D(item.IMAX_A).ToString();
            IMAX_A_ST = ValueCheck("IMAX_A", item.IMAX_A, specdict);

            if (probedict.ContainsKey(key))
            {
                Ith = probedict[key].Ith;
                SlopEff = probedict[key].SlopEff;
                SeriesR = probedict[key].SeriesR;
            }
            else
            {
                Ith = "";
                SlopEff = "";
                SeriesR = "";
            }

            RP = UT.O2I(item.RP);
            TestStep = item.TestStep;

            if (ignoredict.ContainsKey(key))
            {
                IgnoredFlag = "COMMENTLINE";
                Comment = ignoredict[key].Reason;
                Operator = ignoredict[key].UserName;
            }
            else
            {
                IgnoredFlag = "";
                Comment = "";
                Operator = "";
            }

        }


        public WuxiWATData4MG()
        {
            CouponID = "";
            CH = "";
            X = "";
            Y = "";
            BVR_LD_A = "";
            PO_LD_W = "";
            VF_LD_V = "";
            SLOPE_WperA = "";
            THOLD_A = "";
            R_LD_ohm = "";
            IMAX_A = "";
            Ith = "";
            SlopEff = "";
            SeriesR = "";
            IgnoredFlag = "";
            Comment = "";
            Operator = "";
            RP = 0;
            TestStep = "";

            BVR_LD_A_ST = "";
            PO_LD_W_ST = "";
            VF_LD_V_ST = "";
            SLOPE_WperA_ST = "";
            THOLD_A_ST = "";
            R_LD_ohm_ST = "";
            IMAX_A_ST = "";
        }

        public string CouponID { set; get; }
        public string CH { set; get; }
        public string X { set; get; }
        public string Y { set; get; }
        public string BVR_LD_A { set; get; }
        public string PO_LD_W { set; get; }
        public string VF_LD_V { set; get; }
        public string SLOPE_WperA { set; get; }
        public string THOLD_A { set; get; }
        public string R_LD_ohm { set; get; }
        public string IMAX_A { set; get; }
        public string Ith { set; get; }
        public string SlopEff { set; get; }
        public string SeriesR { set; get; }

        public string BVR_LD_A_ST { set; get; }
        public string PO_LD_W_ST { set; get; }
        public string VF_LD_V_ST { set; get; }
        public string SLOPE_WperA_ST { set; get; }
        public string THOLD_A_ST { set; get; }
        public string R_LD_ohm_ST { set; get; }
        public string IMAX_A_ST { set; get; }

        public int RP { set; get; }
        public string TestStep { set; get; }


        public string IgnoredFlag { set; get; }
        public string Comment { set; get; }
        public string Operator { set; get; }
        
    }
}
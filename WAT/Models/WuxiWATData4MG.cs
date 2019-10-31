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

        private static string GetJudgementFromTestStep(string step)
        {
            if (string.Compare(step, "PRLL_VCSEL_Post_Burn_in_Test", true) == 0)
            { return "POSTBIJUDGEMENT"; }
            else if (string.Compare(step, "PRLL_Post_HTOL1_Test", true) == 0)
            { return "POSTHTOL1JUDGEMENT"; }
            else if (string.Compare(step, "PRLL_Post_HTOL2_Test", true) == 0)
            { return "POSTHTOL2JUDGEMENT"; }
            else
            { return ""; }
        }

        private static string GetRPFromTestStep(string step)
        {
            if (string.Compare(step, "PRLL_VCSEL_Post_Burn_in_Test", true) == 0)
            { return "RP01"; }
            else if (string.Compare(step, "PRLL_Post_HTOL1_Test", true) == 0)
            { return "RP02"; }
            else if (string.Compare(step, "PRLL_Post_HTOL2_Test", true) == 0)
            { return "RP03"; }
            else
            { return "RP00"; }
        }

        private static List<string> GetWATResultFromLogic(WuxiWATData4MG tempvm, string jdstep,bool allowmovemapfile = false)
        {
            var failmode = "";
            var result = "";
            try
            {
                var WLG = new WXLogic.WXWATLogic();
                WLG.AllowToMoveMapFile = allowmovemapfile;
                var ret = WLG.WATPassFail(tempvm.CouponID + "E08", jdstep);

                if (!string.IsNullOrEmpty(ret.AppErrorMsg))
                { result = ret.AppErrorMsg; }
                else
                {
                    if (ret.TestPass)
                    { result = "PASS"; }
                    else
                    {
                        if (ret.ScrapIt)
                        { result = "SCRAP"; }
                        else
                        {
                            if (ret.NeedRetest)
                            { result = "FAIL/RETESTABLE"; }
                            else
                            { result = "FAIL/NON-RETESTABLE"; }
                        }//end else
                    }//end else
                }

                if (ret.ValueCollect.ContainsKey("fail mode") && !string.IsNullOrEmpty(ret.ValueCollect["fail mode"]))
                {
                    var mddict = new Dictionary<string, bool>();
                    var unitstr = ret.ValueCollect["fail mode"].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var u in unitstr)
                    {
                        var md = u.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[2];
                        if (!mddict.ContainsKey(md))
                        { mddict.Add(md, true); }
                    }
                    failmode = string.Join(",", mddict.Keys);
                }
            }
            catch (Exception e) { }

            var retval = new List<string>();
            retval.Add(result);
            retval.Add(failmode);
            return retval;
        }

        private static List<string> GetWATResultFromDB(WuxiWATData4MG tempvm,string jdstep)
        {
            var ret = new List<string>();
            var sql = "select result,failuremode from WATResult where wafer = @wafer and teststep = @teststep";
            var dict = new Dictionary<string, string>();
            dict.Add("@wafer", tempvm.CouponID);
            dict.Add("@teststep", jdstep);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                ret.Add(UT.O2S(line[0]));
                ret.Add(UT.O2S(line[1]));
                break;
            }
            return ret;
        }

        private static void StoreWATResult(string wafer, string teststep, string result, string failuremode)
        {
            var dict = new Dictionary<string, string>();
            var sql = "delete from WATResult where wafer = @wafer and teststep = @teststep";
            dict.Add("@wafer", wafer);
            dict.Add("@teststep", teststep);
            DBUtility.ExeLocalSqlNoRes(sql, dict);

            sql = "insert into WATResult(wafer,teststep,result,failuremode) values(@wafer,@teststep,@result,@failuremode)";
            dict.Add("@result",result);
            dict.Add("@failuremode",failuremode);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        private static void RefreshWATTestResult(WuxiWATData4MG tempvm)
        {
            var jdstep = GetJudgementFromTestStep(tempvm.TestStep);
            if (string.IsNullOrEmpty(jdstep))
            { return; }

            if (UT.O2T(tempvm.TestTime) > DateTime.Now.AddDays(-10))
            {
                var ret = GetWATResultFromLogic(tempvm, jdstep,false);
                StoreWATResult(tempvm.CouponID, jdstep, ret[0], ret[1]);

                tempvm.ReTest = ret[0];
                tempvm.FailureStr = ret[1];
                tempvm.FailureShortStr = tempvm.FailureStr;
                if (tempvm.FailureStr.Length > 20)
                { tempvm.FailureShortStr = tempvm.FailureStr.Substring(0, 20); }
            }
            else
            {
                var ret = GetWATResultFromDB(tempvm, jdstep);
                if (ret.Count == 0)
                {
                    ret = GetWATResultFromLogic(tempvm, jdstep,false);
                    StoreWATResult(tempvm.CouponID, jdstep, ret[0], ret[1]);
                    tempvm.ReTest = ret[0];
                    tempvm.FailureStr = ret[1];
                    tempvm.FailureShortStr = tempvm.FailureStr;
                    if (tempvm.FailureStr.Length > 20)
                    { tempvm.FailureShortStr = tempvm.FailureStr.Substring(0, 20); }
                }
                else
                {
                    tempvm.ReTest = ret[0];
                    tempvm.FailureStr = ret[1];
                    tempvm.FailureShortStr = tempvm.FailureStr;
                    if (tempvm.FailureStr.Length > 20)
                    { tempvm.FailureShortStr = tempvm.FailureStr.Substring(0, 20); }
                }
            }
        }

        private static void GetWATTestResult(WuxiWATData4MG tempvm)
        {
            var jdstep = GetJudgementFromTestStep(tempvm.TestStep);
            if (string.IsNullOrEmpty(jdstep))
            { return; }

            var ret = GetWATResultFromDB(tempvm, jdstep);
            if (ret.Count == 0)
            {
                ret = GetWATResultFromLogic(tempvm, jdstep);
                StoreWATResult(tempvm.CouponID, jdstep, ret[0], ret[1]);
                tempvm.ReTest = ret[0];
                tempvm.FailureStr = ret[1];
                tempvm.FailureShortStr = tempvm.FailureStr;
                if (tempvm.FailureStr.Length > 20)
                { tempvm.FailureShortStr = tempvm.FailureStr.Substring(0, 20); }
            }
            else
            {
                tempvm.ReTest = ret[0];
                tempvm.FailureStr = ret[1];
                tempvm.FailureShortStr = tempvm.FailureStr;
                if (tempvm.FailureStr.Length > 20)
                { tempvm.FailureShortStr = tempvm.FailureStr.Substring(0, 20); }
            }
        }

        public static List<WuxiWATData4MG> GetWATStatus()
        {
            var ret = new List<WuxiWATData4MG>();

            var sql = @"select distinct left(Containername,9),TestStep,MAX(TestTimeStamp) latesttime from insite.dbo.ProductionResult
                         where len(Containername) = 20 and Containername not like '17%' and Containername like '%E08%' group by left(Containername,9),TestStep order by latesttime desc,left(Containername,9)";

            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            var dictdata = new Dictionary<string, List<WuxiWATData4MG>>();

            foreach (var line in dbret)
            {
                var wafer = UT.O2S(line[0]);
                var TestStep = UT.O2S(line[1]);
                var TestTime = UT.O2T(line[2]).ToString("yyyy-MM-dd HH:mm:ss");
                var tempvm = new WuxiWATData4MG();
                tempvm.CouponID = wafer;
                tempvm.TestStep = TestStep;
                tempvm.TestTime = TestTime;
                tempvm.RPStr = GetRPFromTestStep(tempvm.TestStep);
                if (dictdata.ContainsKey(wafer))
                { dictdata[wafer].Add(tempvm); }
                else
                {
                    var tmplist = new List<WuxiWATData4MG>();
                    tmplist.Add(tempvm);
                    dictdata.Add(wafer, tmplist);
                }
            }

            foreach (var kv in dictdata)
            {
                var tmplist = kv.Value;
                tmplist.Sort(delegate (WuxiWATData4MG obj1, WuxiWATData4MG obj2)
                {
                    return UT.O2I(obj2.RPStr.Replace("RP0", "")).CompareTo(UT.O2I(obj1.RPStr.Replace("RP0", "")));
                });
                var tempvm = tmplist[0];
                GetWATTestResult(tempvm);
                ret.Add(tempvm);
            }

            return ret;
        }

        public static List<WuxiWATData4MG> RefreshWATStatusDaily()
        {
            var ret = new List<WuxiWATData4MG>();

            var sql = @"select distinct left(Containername,9),TestStep,MAX(TestTimeStamp) latesttime from insite.dbo.ProductionResult
                         where len(Containername) = 20 and Containername not like '17%' group by left(Containername,9),TestStep order by latesttime desc,left(Containername,9)";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            var wdict = new Dictionary<string, string>();
            foreach (var line in dbret)
            {
                var wafer = UT.O2S(line[0]);
                if (wdict.ContainsKey(wafer))
                { continue; }
                wdict.Add(wafer, wafer);

                var TestStep = UT.O2S(line[1]);
                var TestTime = UT.O2T(line[2]).ToString("yyyy-MM-dd HH:mm:ss");
                var tempvm = new WuxiWATData4MG();
                tempvm.CouponID = wafer;
                tempvm.TestStep = TestStep;
                tempvm.TestTime = TestTime;
                RefreshWATTestResult(tempvm);
                ret.Add(tempvm);
            }

            return ret;
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

            TestTime = item.TestTimeStamp.ToString("yyyy-MM-dd HH:mm:ss");
            ReTest = "";
            FailureShortStr = "";
            FailureStr = "";
            RPStr = "";
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

            TestTime = "";
            ReTest = "";
            FailureShortStr = "";
            FailureStr = "";
            RPStr = "";
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
        public string TestTime { set; get; }

        public string ReTest { set; get; }
        public string FailureShortStr { set; get; }
        public string FailureStr { set; get; }
        public string RPStr { set; get; }
    }
}
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

            var CouponGroup = GetCouponGroup(coupongroup1.ToUpper().Trim());

            if (string.IsNullOrEmpty(CouponGroup))
            {
                return ret;
            }

            var oringaldata = WXLogic.WXOriginalWATData.GetData(CouponGroup);
            var wafernum = CouponGroup.Split(new string[] { "E", "R" }, StringSplitOptions.RemoveEmptyEntries)[0];
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

        private static string GetCouponGroup(string coupongroup1)
        {
            var CouponGroup = "";
            try
            {
                if (coupongroup1.Length < 12 || (!coupongroup1.Contains("E") && !coupongroup1.Contains("R")))
                { return string.Empty; }
                else
                {
                    var len = 0;
                    if (coupongroup1.Contains("E"))
                    { len = coupongroup1.IndexOf("E") + 3; }
                    else
                    { len = coupongroup1.IndexOf("R") + 3; }

                    if (coupongroup1.Length < len)
                    { return string.Empty; }
                    else
                    { CouponGroup = coupongroup1.Substring(0, len); }
                }
            }
            catch (Exception ex) { return string.Empty; }
            return CouponGroup;
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

                var fdict = new Dictionary<string, int>();
                var ftab = (List<WXLogic.WXWATFailureMode>)ret.DataTables[1];
                foreach(var f in ftab)
                {
                    if (string.Compare(f.Failure, "PASS", true) != 0)
                    {
                        if (!fdict.ContainsKey(f.Failure.ToUpper()))
                        { fdict.Add(f.Failure, 1); }
                        else
                        { fdict[f.Failure] += 1; }
                    }
                }
                foreach (var kv in fdict)
                { failmode += kv.Key + ":" + kv.Value + ";"; }
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
                //tempvm.FailureShortStr = tempvm.FailureStr;
                //if (tempvm.FailureStr.Length > 20)
                //{ tempvm.FailureShortStr = tempvm.FailureStr.Substring(0, 20); }
            }
            else
            {
                tempvm.ReTest = ret[0];
                tempvm.FailureStr = ret[1];
                //tempvm.FailureShortStr = tempvm.FailureStr;
                //if (tempvm.FailureStr.Length > 20)
                //{ tempvm.FailureShortStr = tempvm.FailureStr.Substring(0, 20); }
            }

            if (!string.IsNullOrEmpty(tempvm.FailureStr))
            {
                var kvlist = tempvm.FailureStr.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                foreach (var kv in kvlist)
                {
                    var kvs = kv.ToUpper().Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                    if (kvs[0].Contains("DVF"))
                    {
                        if (kvs.Length >= 2)
                            tempvm.DVF = UT.O2I(kvs[1]);
                        else
                            tempvm.DVF = 1;
                    }
                    else if (kvs[0].Contains("LOWPOWERLOWLEAKAGE"))
                    {
                        if (kvs.Length >= 2)
                            tempvm.LPW = UT.O2I(kvs[1]);
                        else
                            tempvm.LPW = 1;
                    }
                    else if (kvs[0].Contains("DISLOCATION"))
                    {
                        if (kvs.Length >= 2)
                            tempvm.DIS = UT.O2I(kvs[1]);
                        else
                            tempvm.DIS = 1;
                    }
                    else if (kvs[0].Contains("WEAROUT"))
                    {
                        if (kvs.Length >= 2)
                            tempvm.WOT = UT.O2I(kvs[1]);
                        else
                            tempvm.WOT = 1;
                    }
                    else if (kvs[0].Contains("DLAM"))
                    {
                        if (kvs.Length >= 2)
                            tempvm.DLA = UT.O2I(kvs[1]);
                        else
                            tempvm.DLA = 1;
                    }
                }//end foreach
            }//end if
        }


        private static List<List<object>> GetWUXIWATWaferStepData()
        {
            var sql = @"select distinct left(Containername,9),TestStep,MAX(TestTimeStamp) latesttime from insite.dbo.ProductionResult
                         where len(Containername) = 20 and Containername not like '17%' and Containername like '%E08%' group by left(Containername,9),TestStep order by latesttime desc,left(Containername,9)";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            sql = @"select distinct left(Containername,13),TestStep,MAX(TestTimeStamp) latesttime from insite.dbo.ProductionResult
                         where len(Containername) = 24 and Containername like '%E08%' group by left(Containername,13),TestStep order by latesttime desc,left(Containername,13)";
            var dbret1 = DBUtility.ExeLocalSqlWithRes(sql);
            dbret.AddRange(dbret1);
            return dbret;
        }

        public static Dictionary<string, bool> GetKOYODict(List<string> wflist, Controller ctrl)
        {
            var syscfg = CfgUtility.GetSysConfig(ctrl);
            var koyopns = syscfg["KOYOPNS"];
            var wfcond = "('" + string.Join("','", wflist)+"')";

            var koyodict = new Dictionary<string, bool>();

            var sql = @"select c.containername,pb.ProductName from insite.insite.container c with (nolock)
                inner join insite.insite.Product p with (nolock) on c.ProductID = P.ProductID
                inner join insite.insite.ProductBase pb with (nolock) on p.ProductID = pb.RevOfRcdID
                where c.containername in <wfcond>";
            sql = sql.Replace("<wfcond>", wfcond);
            var dbret = DBUtility.ExeAllenSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var wf = UT.O2S(line[0]).ToUpper();
                var pn = UT.O2S(line[1]).ToUpper();
                if (koyopns.Contains(pn))
                {
                    if (!koyodict.ContainsKey(wf))
                    { koyodict.Add(wf, true); }
                }
            }

            return koyodict;
        }

        public static void GetVcselType(List<WuxiWATData4MG> datalist,Controller ctrl)
        {
            var wflist = new List<string>();
            foreach (var d in datalist)
            {
                wflist.Add(d.CouponID);
            }

            var koyodict = GetKOYODict(wflist, ctrl);
            foreach (var d in datalist)
            {
                if (koyodict.ContainsKey(d.CouponID.ToUpper()))
                { d.VType = "KOYO"; }
            }
        }

        public static List<WuxiWATData4MG> GetWATStatus(Controller ctrl)
        {
            var ret = new List<WuxiWATData4MG>();
            var dictdata = new Dictionary<string, List<WuxiWATData4MG>>();

            var dbret = GetWUXIWATWaferStepData();
            foreach (var line in dbret)
            {
                if (UT.O2T(line[2]) < DateTime.Parse("2019-10-01 00:00:00"))
                { continue; };

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
                tempvm.VArray = WXLogic.WATSampleXY.GetArrayFromAllenSherman(tempvm.CouponID);
                ret.Add(tempvm);
            }

            GetVcselType(ret,ctrl);

            return ret;
        }

        public static List<WuxiWATData4MG> RefreshWATStatusDaily()
        {
            var ret = new List<WuxiWATData4MG>();
            var wdict = new Dictionary<string, string>();

            var dbret = GetWUXIWATWaferStepData();

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
            VType = "";
            VArray = "";

            DVF = 0;
            LPW = 0;
            DIS = 0;
            WOT = 0;
            DLA = 0;
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
            VType = "";
            VArray = "";

            DVF = 0;
            LPW = 0;
            DIS = 0;
            WOT = 0;
            DLA = 0;
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
        public string VType { set; get; }
        public string VArray { set; get; }

        public int DVF { set; get; }
        public int LPW { set; get; }
        public int DIS { set; get; }
        public int WOT { set; get; }
        public int DLA { set; get; }
    }
}
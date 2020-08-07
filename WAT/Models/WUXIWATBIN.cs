using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WAT.Models
{
    public class WUXIWATBIN
    {
        private static List<WUXIWATBIN> GetWaferRes(string wf)
        {
            var cond = @" and  (wafer like '%E08%'  or wafer like '%R08%'  or wafer like '%T08%' 
                                or wafer like '%E09%'  or wafer like '%R09%'  or wafer like '%T09%') ";

            var sql = @"select distinct wafer,result,CONVERT(datetime,AppVal1) as tm,teststep FROM [WAT].[dbo].[WATResult] where left(wafer,len(wafer)-3) = '" + wf+"' "+cond+ " order by CONVERT(datetime,AppVal1) DESC";
            var ret = new List<WUXIWATBIN>();

            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var tempvm = new WUXIWATBIN();
                tempvm.Wafer = UT.O2S(line[0]);
                tempvm.Status = UT.O2S(line[1]).ToUpper();
                tempvm.Bin = UT.T2S(line[2]);
                tempvm.PN = UT.O2S(line[3]);
                ret.Add(tempvm);
            }
            return ret;
        }

        private static List<WUXIWATBIN> BinData(string wafer,Dictionary<string,string> pndict,Dictionary<string,string> wfproddict)
        {
            var ret = new List<WUXIWATBIN>();
            if (wafer.Length == 13)
            {
                var sql = "select wafer,bin,bincount from [WAT].[dbo].[WaferPassBinData] where WAFER = @wafer";
                var dict = new Dictionary<string, string>();
                dict.Add("@wafer", wafer);
                var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
                foreach (var line in dbret)
                {
                    var wf = UT.O2S(line[0]);
                    var bin = UT.O2S(line[1]);
                    var pn = "";
                    if (wfproddict.ContainsKey(wf))
                    {
                        var prod = wfproddict[wf];
                        if (pndict.ContainsKey(prod + "-" + bin))
                        {
                            pn = pndict[prod + "-" + bin];
                        }
                    }

                    var tempvm = new WUXIWATBIN();
                    tempvm.Wafer = wf;
                    tempvm.Status = "PASS";
                    tempvm.Bin = bin;
                    tempvm.PN = pn;
                    tempvm.QTY = UT.O2S(line[2]);
                    ret.Add(tempvm);
                }
            }
            else
            {
                var srcdict = new Dictionary<string, int>();
                var sql = "select wafer,bincode,bincount from [WAT].[dbo].[WaferSrcData] where WAFER = @wafer and BinQuality = 'Pass'";
                var dict = new Dictionary<string, string>();
                dict.Add("@wafer", wafer);
                var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
                foreach (var line in dbret)
                {
                    var bin = UT.O2S(line[1]);
                    var cnt = UT.O2I(line[2]);
                    if (!srcdict.ContainsKey(bin))
                    {
                        srcdict.Add(bin, cnt);
                    }
                }

                var sampdict = new Dictionary<string, int>();
                sql = "select bin,count(bin) from [WAT].[dbo].[WaferSampleData] where WAFER = @wafer group by bin";
                dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
                foreach (var line in dbret)
                {
                    var bin = UT.O2S(line[0]);
                    var cnt = UT.O2I(line[1]);
                    if (!sampdict.ContainsKey(bin))
                    {
                        sampdict.Add(bin, cnt);
                    }
                }

                foreach (var skv in srcdict)
                {
                    var bin = skv.Key;
                    var cnt = skv.Value;
                    if (sampdict.ContainsKey(bin))
                    {
                        cnt = cnt - sampdict[bin];
                    }

                    var pn = "";
                    if (wfproddict.ContainsKey(wafer))
                    {
                        var prod = wfproddict[wafer];
                        if (pndict.ContainsKey(prod + "-" + bin))
                        {
                            pn = pndict[prod + "-" + bin];
                        }
                    }

                    var tempvm = new WUXIWATBIN();
                    tempvm.Wafer = wafer;
                    tempvm.Status = "PASS";
                    tempvm.Bin = bin;
                    tempvm.PN = pn;
                    tempvm.QTY = cnt.ToString();
                    ret.Add(tempvm);
                }
            }

            return ret;
        }

        private static bool CCT(string coupon, string type)
        {
            var keylist = new List<string>(new string[] { "E", "R", "T" });
            foreach (var k in keylist)
            {
                if (coupon.Contains(k + type))
                { return true; }
            }
            return false;
        }

        public static List<WUXIWATBIN> GetBinInfo(List<string> wflist, Controller ctrl)
        {
            var pndict = CfgUtility.GetProdfamPN(ctrl);
            var wfproddict = WXEvalPN.GetWaferProdfamDict();
            var ret = new List<WUXIWATBIN>();

            var nwflist = new List<string>();
            foreach (var wf in wflist)
            {
                var w = wf.ToUpper().Split(new string[] { "E", "R", "T" }, StringSplitOptions.RemoveEmptyEntries)[0];
                if (!nwflist.Contains(w))
                { nwflist.Add(w); }
            }

            foreach (var wf in nwflist)
            {
                var rest = GetWaferRes(wf);
                if (rest.Count == 0)
                {
                    var tempvm = new WUXIWATBIN();
                    tempvm.Wafer = wf;
                    tempvm.Status = "NO WAT";
                    ret.Add(tempvm);
                }
                else
                {
                    foreach (var r in rest)
                    {
                        var HTOL = "HTOL2";
                        if (CCT(r.Wafer, "09"))
                        { HTOL = "HTOL4"; }
                        if (CCT(r.Wafer, "08"))
                        { HTOL = "HTOL2"; }

                        if (r.PN.Contains(HTOL))
                        {
                            var tempvm = new WUXIWATBIN();
                            tempvm.Wafer = wf;
                            if (r.Status.Contains("PASS"))
                            { ret.AddRange(BinData(wf, pndict, wfproddict)); break; }
                            else if (r.Status.Contains("GREAT"))
                            {
                                tempvm.Status = "PENDING";
                                ret.Add(tempvm); break;
                            }
                            else
                            {
                                tempvm.Status = "FAIL";
                                ret.Add(tempvm); break;
                            }
                        }
                        else
                        {
                            var tempvm = new WUXIWATBIN();
                            tempvm.Wafer = wf;
                            if (r.Status.Contains("PASS")|| r.Status.Contains("GREAT"))
                            {
                                tempvm.Status = "PENDING";
                                ret.Add(tempvm); break;
                            }
                            else
                            {
                                tempvm.Status = "FAIL";
                                ret.Add(tempvm); break;
                            }
                        }
                    }//end foreach
                }//end else
            }//end foreach
            return ret;
        }

        public WUXIWATBIN()
        {
            Wafer = "";
            Status = "";
            Bin = "";
            PN = "";
            QTY = "";
        }

        public string Wafer { set; get; }
        public string Status { set; get; }
        public string Bin { set; get; }
        public string PN { set; get; }
        public string QTY { set; get; }
    }
}
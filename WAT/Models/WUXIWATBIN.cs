using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WAT.Models
{
    public class WUXIWATBIN
    {
        private static Dictionary<string, bool> GetDict(string sql)
        {
            var ret = new Dictionary<string, bool>();
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var k = UT.O2S(line[0]);
                if (!ret.ContainsKey(k))
                { ret.Add(k, true); }
            }
            return ret;
        }

        private static Dictionary<string, bool> GetWUXIWATWafer()
        {
            var sql = @"select distinct left(wafer,len(wafer)-1) as wf FROM [WAT].[dbo].[WATResult]";
            return GetDict(sql);
        }

        private static Dictionary<string, string> GetWUXIWATHTOLWafer(string htol)
        {
            var sql = @"select distinct left(wafer,len(wafer)-1) as wf,result,AppVal1 FROM [WAT].[dbo].[WATResult] where teststep like '%" + htol+ "%'  order by AppVal1 DESC";
            var ret = new Dictionary<string, string>();
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var k = UT.O2S(line[0]);
                var res = UT.O2S(line[1]).ToUpper();
                if (!ret.ContainsKey(k))
                { ret.Add(k, res); }
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

        public static List<WUXIWATBIN> GetBinInfo(List<string> wflist, Controller ctrl)
        {
            var pndict = CfgUtility.GetProdfamPN(ctrl);
            var wfproddict = WXEvalPN.GetWaferProdfamDict();

            var watwf = GetWUXIWATWafer();
            var htol2wf = GetWUXIWATHTOLWafer("HTOL2");
            var htol1wf = GetWUXIWATHTOLWafer("HTOL1");
            var ret = new List<WUXIWATBIN>();
            foreach (var wf in wflist)
            {
                if (watwf.ContainsKey(wf)){
                    if (htol2wf.ContainsKey(wf))
                    {
                        var stat = htol2wf[wf];
                        if (stat.Contains("PASS"))
                        {
                            ret.AddRange(BinData(wf,pndict,wfproddict));
                        }
                        else if (stat.Contains("GREAT")) {
                            var tempvm = new WUXIWATBIN();
                            tempvm.Wafer = wf;
                            tempvm.Status = "PENDING";
                            ret.Add(tempvm);
                        }
                        else {
                            var tempvm = new WUXIWATBIN();
                            tempvm.Wafer = wf;
                            tempvm.Status = "FAIL";
                            ret.Add(tempvm);
                        }
                    }
                    else
                    {
                        if (htol1wf.ContainsKey(wf))
                        {
                            var stat = htol1wf[wf];
                            if (stat.Contains("PASS") || stat.Contains("GREAT"))
                            {
                                var tempvm = new WUXIWATBIN();
                                tempvm.Wafer = wf;
                                tempvm.Status = "PENDING";
                                ret.Add(tempvm);
                            }
                            else
                            {
                                var tempvm = new WUXIWATBIN();
                                tempvm.Wafer = wf;
                                tempvm.Status = "FAIL";
                                ret.Add(tempvm);
                            }
                        }
                        else
                        {
                            var tempvm = new WUXIWATBIN();
                            tempvm.Wafer = wf;
                            tempvm.Status = "PENDING";
                            ret.Add(tempvm);
                        }
                    }
                }
                else {
                    var tempvm = new WUXIWATBIN();
                    tempvm.Wafer = wf;
                    tempvm.Status = "NO WAT";
                    ret.Add(tempvm);
                }
            }
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
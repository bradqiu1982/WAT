using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class SpecBinPassFail
    {
        public static List<SpecBinPassFail> GetAllSpec()
        {
            var ret = new List<SpecBinPassFail>();

            var sql = @"select [Eval_ProductName],[Bin_Product],[DCDefName],[ParameterName],[Wafer_LL],[Wafer_UL]
                        ,[DUT_LL],[DUT_UL],[min_DUT_Count] from [EngrData].[insite].[Eval_Specs_Bin_PassFail]";
            var dbret = DataBaseUT.ExeAllenSqlWithRes(sql);

            foreach (var line in dbret)
            {
                var tempvm = new SpecBinPassFail();
                tempvm.Eval_PN = UT.O2S(line[0]);
                tempvm.Bin_PN = UT.O2S(line[1]);
                tempvm.DCDName = UT.O2S(line[2]);
                tempvm.ParamName = UT.O2S(line[3]);
                tempvm.WLL = UT.O2S(line[4]);
                tempvm.WUL = UT.O2S(line[5]);
                tempvm.DTLL = UT.O2S(line[6]);
                tempvm.DTUL = UT.O2S(line[7]);
                tempvm.minDUT = UT.O2I(line[8]);
                ret.Add(tempvm);
            }
            return ret;
        }

        public static Dictionary<string, bool> RetrieveBinDict(string evalpn,List<SpecBinPassFail> alldata)
        {
            var ret = new Dictionary<string, bool>();
            foreach (var item in alldata)
            {
                if (string.Compare(item.Eval_PN, evalpn, true) == 0)
                {
                    if (!string.IsNullOrEmpty(item.Bin_PN) && !ret.ContainsKey(item.Bin_PN))
                    {
                        ret.Add(item.Bin_PN, true);
                    }
                }
            }
            return ret;
        }

        public static List<SpecBinPassFail> GetMinDUT(string evalpn, string dcdname, List<SpecBinPassFail> alldata)
        {
            var ret = new List<SpecBinPassFail>();
            foreach (var item in alldata)
            {
                if (string.Compare(item.Eval_PN, evalpn, true) == 0
                    && string.Compare(item.DCDName, dcdname, true) == 0)
                {
                    ret.Add(item);
                    return ret;
                }
            }
            return ret;
        }

        public static List<SpecBinPassFail> GetParam4FailMode(string evalpn, string rp, List<SpecBinPassFail> alldata)
        {
            var ret = new List<SpecBinPassFail>();
            var dpo = false;
            var dvf = false;
            foreach (var item in alldata)
            {
                if (string.Compare(item.Eval_PN, evalpn, true) == 0
                    && string.Compare(item.RP, rp, true) == 0)
                {
                    if (!dpo && item.ParamName.ToUpper().Contains("PO_LD_W_RD_REF"))
                    {
                        dpo = true;
                        ret.Add(item);
                    }

                    if (!dvf && item.ParamName.ToUpper().Contains("VF_LD_V_AD_REF"))
                    {
                        dvf = true;
                        ret.Add(item);
                    }

                    if (ret.Count == 2)
                    { return ret; }
                }
            }
            return ret;
        }

        public static List<SpecBinPassFail> GetSpecByPNDCDName(string evalpn, string dcdname, List<SpecBinPassFail> alldata)
        {
            var ret = new List<SpecBinPassFail>();
            foreach (var item in alldata)
            {
                if (string.Compare(item.Eval_PN, evalpn, true) == 0
                    && string.Compare(item.DCDName, dcdname, true) == 0
                    && !string.IsNullOrEmpty(item.Bin_PN))
                {
                    ret.Add(item);
                }
            }
            return ret;
        }

        public static List<SpecBinPassFail> GetCPKSpec(string evalpn, string dcdname, List<SpecBinPassFail> alldata)
        {
            var ret = new List<SpecBinPassFail>();
            foreach (var item in alldata)
            {
                if (string.Compare(item.Eval_PN, evalpn, true) == 0
                    && string.Compare(item.DCDName, dcdname, true) == 0
                    && !string.IsNullOrEmpty(item.Bin_PN)
                    && item.ParamName.ToUpper().Contains("CPK"))
                {
                    ret.Add(item);
                }
            }
            return ret;
        }

        public static List<SpecBinPassFail> GetFitSpec(string evalpn, string dcdname, List<SpecBinPassFail> alldata)
        {
            var ret = new List<SpecBinPassFail>();
            foreach (var item in alldata)
            {
                if (string.Compare(item.Eval_PN, evalpn, true) == 0
                    && string.Compare(item.DCDName, dcdname, true) == 0
                    && !string.IsNullOrEmpty(item.Bin_PN)
                    && item.ParamName.ToUpper().Contains("FIT"))
                {
                    ret.Add(item);
                }
            }
            return ret;
        }

        public string Eval_PN { set; get; }
        public string PN7 { get {
                if (Eval_PN.Length > 7)
                {
                    return Eval_PN.Substring(0, 7);
                }
                else
                { return Eval_PN; }
            } }

        public string Bin_PN { set; get; }

        public string DCDName { set; get; }
        public string RP {
            get {
                if (DCDName.ToUpper().Contains("_RP") || DCDName.ToUpper().Contains("_DP"))
                {
                    return UT.O2I(DCDName.ToUpper().Split(new string[] { "_RP", "_DP" }, StringSplitOptions.RemoveEmptyEntries)[1]).ToString();
                }
                else { return string.Empty; }
            }
        }

        public string ParamName { set; get; }

        public string WLL { set; get; }
        public string WUL { set; get; }

        public string DTLL { set; get; }
        public string DTUL { set; get; }

        public int minDUT { set; get; }

    }
}
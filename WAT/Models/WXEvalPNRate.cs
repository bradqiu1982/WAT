using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class WXEvalPNRate
    {
        public static void RefreshEVALPNRate()
        {
            var allpnrate = GetAllEvalPNRate();
            if (allpnrate.Count > 0)
            {
                foreach (var item in allpnrate)
                { item.UpdatePNRate(); }
            }
        }

        private void UpdatePNRate()
        {
            if (!HasData())
            {
                var dict = new Dictionary<string, string>();
                dict.Add("@EvalPN", EvalPN);
                dict.Add("@RealRate", RealRate);
                dict.Add("@TreatRate", TreatRate);

                var sql = "insert into WAT.dbo.WXEvalPNRate(EvalPN,RealRate,TreatRate) values(@EvalPN,@RealRate,@TreatRate)";
                DBUtility.ExeLocalSqlNoRes(sql, dict);
            }
        }

        private bool HasData()
        {
            var dict = new Dictionary<string, string>();
            dict.Add("@EvalPN", EvalPN);

            var sql = "select EvalPN from WXEvalPNRate where EvalPN=@EvalPN";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            if (dbret.Count > 0)
            { return true; }
            return false;
        }

        private static List<WXEvalPNRate> GetAllEvalPNRate()
        {
            var ret = new Dictionary<string,WXEvalPNRate>();

            var allevalpn = new List<string>();
            var sql = "select distinct left(Eval_ProductName,7) from [WAT].[dbo].[Eval_Specs_Chip_PassFail]";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            { allevalpn.Add(UT.O2S(line[0])); }

            var evalcond = "('" + string.Join("','", allevalpn) + "')";

            sql = @"select distinct pb.productname,p.Description from insite.insite.product p with(nolock) 
                    inner join insite.insite.productbase pb with(nolock) on pb.productbaseid=p.productbaseid
	                where pb.productname in <evalcond>";
            sql = sql.Replace("<evalcond>", evalcond);
            dbret = DBUtility.ExeAllenSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var pn = UT.O2S(line[0]);
                if (!ret.ContainsKey(pn))
                {
                    var desc = UT.O2S(line[1]).ToUpper();
                    var tempvm = GetRate(desc);
                    tempvm.EvalPN = pn;
                    ret.Add(pn, tempvm);
                }
            }

            sql = @"select distinct pb.productname,p.Description from [SHM-CSSQL].insite.insite.product p with(nolock) 
                    inner join [SHM-CSSQL].insite.insite.productbase pb with(nolock) on pb.productbaseid=p.productbaseid
	                where pb.productname in <evalcond>";
            sql = sql.Replace("<evalcond>", evalcond);
            dbret = DBUtility.ExeShermanSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var pn = UT.O2S(line[0]);
                if (!ret.ContainsKey(pn))
                {
                    var desc = UT.O2S(line[1]).ToUpper();
                    var tempvm = GetRate(desc);
                    tempvm.EvalPN = pn;
                    ret.Add(pn, tempvm);
                }
            }

            return ret.Values.ToList();
        }

        private static WXEvalPNRate GetRate(string desc)
        {
            var ret = new WXEvalPNRate();
            if (desc.Contains("50G"))
            { ret.RealRate = "50G"; ret.TreatRate = "25G"; return ret; }
            if (desc.Contains("28G"))
            { ret.RealRate = "28G"; ret.TreatRate = "25G"; return ret; }
            if (desc.Contains("25G"))
            { ret.RealRate = "25G"; ret.TreatRate = "25G"; return ret; }

            if (desc.Contains("14G"))
            { ret.RealRate = "14G"; ret.TreatRate = "10G"; return ret; }
            if (desc.Contains("10G"))
            { ret.RealRate = "10G"; ret.TreatRate = "10G"; return ret; }
            if (desc.Contains("8G"))
            { ret.RealRate = "8G"; ret.TreatRate = "10G"; return ret; }

            if (desc.Contains("5G"))
            { ret.RealRate = "5G"; ret.TreatRate = "5G"; return ret; }

            ret.RealRate = "25"; ret.TreatRate = "25G"; return ret;
        }

        public WXEvalPNRate()
        {
            EvalPN = "";
            RealRate = "";
            TreatRate = "";
        }

        public string EvalPN { set; get; }
        public string RealRate { set; get; }
        public string TreatRate { set; get; }
    }
}
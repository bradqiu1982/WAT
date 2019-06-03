using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class WXContainerInfo
    {

        public static WXContainerInfo GetInfo(string containername)
        {
            var ret = new WXContainerInfo();
            ret.containername = containername.ToUpper().Trim();
            ret.wafer = ret.containername.Split(new string[] { "E", "e" }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
            ret.containertype = "W";
            ret.lottype = "W";
            ret.ProductName = GetEvalPNFromWafer(ret.containername, ret.wafer);
            return ret;
        }

        private static string GetEvalPNFromWafer(string containername, string wafer)
        {
            var FilterStr = "";

            if (containername.Contains("E08")
                || containername.Contains("E01")
                || containername.Contains("E06"))
            { FilterStr = "50UP"; }
            else if (containername.Contains("E07")
                || containername.Contains("E10")
                || containername.Contains("E09")
                || containername.Contains("E03"))
            { FilterStr = "COB"; }

            var productfamilylist = new List<string>();
            var sql = @"select distinct Left(pf.ProductFamilyName,4) from  insite.insite.ProductBase pb  (nolock) 
                          inner join insite.insite.Product p with (nolock) on p.ProductID = pb.RevOfRcdID
                          inner join insite.insite.ProductFamily pf with (nolock) on p.ProductFamilyID = pf.ProductFamilyID
                          inner join insite.insite.ProductType pt with (nolock) on p.ProductTypeID = pt.ProductTypeID where pb.productname in(
                            SELECT distinct bp.BinJobPartNumber FROM [Insite].[insite].[Rpt_ReleasedJob] rj
                        left join [Insite].[insite].[BinJobPartnumbers] bp on bp.ChipPartNumber = rj.product
                        where Factory = 'CHIP' and left(ContainerName,9) = @wafernum and bp.BinJobPartNumber is not null)";
            var dict = new Dictionary<string, string>();
            dict.Add("@wafernum", wafer);
            var dbret = DBUtility.ExeAllenSqlWithRes(sql, dict);
            foreach (var line in dbret)
            { productfamilylist.Add(UT.O2S(line[0])); }

            if (productfamilylist.Count == 0)
            { return string.Empty; }

            var specdict = new Dictionary<string, string>();

            var familycond = "('" + string.Join("','", productfamilylist) + "')";
            sql = @"select distinct EvalPartNumber,spec from  [EngrData].[dbo].[Eval_Conditions_New] where LEFT(Product,4) in <familycond> and LEN(EvalPartNumber) = 7 and Spec is not null and spec <> 'NULL'";
            sql = sql.Replace("<familycond>", familycond);
            dbret = DBUtility.ExeAllenSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var spec = "1";
                var spname = UT.O2S(line[1]).ToUpper();
                if (spname.Contains("50UP"))
                { spec = "50UP"; }
                else if (spname.Contains("COB") || spname.Contains("BDH") || spname.Contains("HAST"))
                { spec = "COB"; }

                if (!specdict.ContainsKey(spec))
                { specdict.Add(spec, UT.O2S(line[0])); }
            }

            if (specdict.ContainsKey(FilterStr))
            { return specdict[FilterStr]; }
            else
            { return string.Empty; }

        }

        public WXContainerInfo()
        {
            containername = "";
            wafer = "";
            containertype = "";
            lottype = "";
        }

        public string containername { set; get; }
        public string ProductName { set; get; }
        public string wafer { set; get; }
        public string containertype { set; get; }
        public string lottype { set; get; }
    }
}
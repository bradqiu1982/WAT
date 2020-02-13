using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WXLogic
{
    public class WXContainerInfo
    {

        public static WXContainerInfo GetInfo(string CouponGroup)
        {
            var ret = new WXContainerInfo();
            ret.containername = CouponGroup.ToUpper().Trim();
            ret.wafer = ret.containername.Split(new string[] { "E", "R","T" }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
            ret.containertype = WXEvalPN.GetLotTypeByWaferNum(ret.wafer);
            ret.lottype = ret.containertype;
            ret.ProductName = WXEvalPN.GetEvalPNByWaferNum(ret.containername, ret.wafer);
            ret.ProdFam = WXEvalPN.GetProdFamByWaferNum(ret.wafer);
            return ret;
        }

        //private static string GetContainerType(string wafer)
        //{
        //    var sql = @"select ContainerType from insite.insite.container where containername = @wafernum";
        //    var dict = new Dictionary<string, string>();
        //    dict.Add("@wafernum", wafer);
        //    var dbret = DBUtility.ExeAllenSqlWithRes(sql, dict);
        //    foreach (var line in dbret)
        //    {
        //        return UT.O2S(line[0]);
        //    }

        //    return "W";
        //}

        //private static string GetEvalPNFromWafer(string containername, string wafer)
        //{
        //    var FilterStr = "";

        //    if (containername.Contains("E08")
        //        || containername.Contains("E01")
        //        || containername.Contains("E06"))
        //    { FilterStr = "50UP"; }
        //    else if (containername.Contains("E07")
        //        || containername.Contains("E10")
        //        || containername.Contains("E09")
        //        || containername.Contains("E03"))
        //    { FilterStr = "COB"; }

        //    var productfamilylist = new List<string>();
        //    var sql = @"select distinct Left(fj.EvalPartNumber,7),left(spec.DCDefName,len(spec.DCDefName)-5) from insite.insite.container c with (nolock) 
        //                inner join insite.insite.Product p with (nolock) on p.ProductID = c.ProductID
        //                inner join insite.insite.ProductFamily pf with (nolock) on p.ProductFamilyID = pf.ProductFamilyID
        //                left join [EngrData].[insite].[FinEvalJobStartInfo] fj with (nolock) on left(pf.productfamilyname,4) = fj.Device
        //                left join [EngrData].[insite].[Eval_Specs_Bin_PassFail] spec with (nolock) on spec.Eval_ProductName = fj.EvalPartNumber
        //                where containername = @wafernum and Len(fj.EvalPartNumber) > 6 and fj.EvalPartNumber is not null and spec.DCDefName is not null";
        //    var dict = new Dictionary<string, string>();
        //    dict.Add("@wafernum", wafer);
        //    var dbret = DBUtility.ExeAllenSqlWithRes(sql, dict);
        //    foreach (var line in dbret)
        //    {
        //        var evalpn = UT.O2S(line[0]);
        //        var dcd = UT.O2S(line[1]).ToUpper();
        //        if (dcd.Contains(FilterStr))
        //        {
        //            return evalpn;
        //        }
        //    }
        //    return string.Empty;
        //}

        public WXContainerInfo()
        {
            containername = "";
            wafer = "";
            containertype = "";
            lottype = "";
            ProdFam = "";
        }

        public string containername { set; get; }
        public string ProductName { set; get; }
        public string wafer { set; get; }
        public string containertype { set; get; }
        public string lottype { set; get; }
        public string ProdFam { set; get; }
    }
}
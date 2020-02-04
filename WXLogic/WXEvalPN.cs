using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WXLogic
{
    public class WXEvalPN
    {

        public static string GetProductFamilyFromAllen(string wafernum)
        {
            var sql = @"select distinct left(pf.productfamilyname,4) from insite.insite.container c with (nolock) 
                        inner join [Insite].[insite].[ProductBase] pb ON pb.[RevOfRcdId] = c.[ProductId]
                        inner join insite.insite.Product p with(nolock) on p.[ProductBaseId] = pb.[ProductBaseId] 
                        inner join insite.insite.ProductFamily pf with(nolock) on p.ProductFamilyID = pf.ProductFamilyID
                        where containername = @wafernum";

            var dict = new Dictionary<string, string>();
            dict.Add("@wafernum", wafernum);

            var dbret = DBUtility.ExeAllenSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {  return UT.O2S(line[0]); }

            return string.Empty;
        }

        public static string GetProductFamilyFromSherman(string wafernum)
        {
            var sql = @"SELECT distinct LEFT(pf.[ProductFamilyName],4) AS PRODUCT_FAMILY
                    FROM [SHM-CSSQL].[Insite].[insite].[Container] c with(nolock)
                    INNER JOIN [SHM-CSSQL].[Insite].[insite].[ProductBase] pb with(nolock) ON pb.[RevOfRcdId] = c.[ProductId] 
                    INNER JOIN [SHM-CSSQL].[Insite].[insite].[Product] p with(nolock) ON p.[ProductBaseId] = pb.[ProductBaseId] 
                    INNER JOIN [SHM-CSSQL].[Insite].[insite].[ProductFamily] pf with(nolock) ON pf.[ProductFamilyId] = p.[ProductFamilyId]
                    WHERE c.[ContainerName] = @wafernum";

            var dict = new Dictionary<string, string>();
            dict.Add("@wafernum", wafernum);

           var dbret = DBUtility.ExeShermanSqlWithRes(sql,dict);
            foreach (var line in dbret)
            { return UT.O2S(line[0]); }

            return string.Empty;
        }


        public static string GetEvalPNByWaferNum(string containername, string wafernum)
        {
            //var FilterStr = "";
            //if (containername.Contains("E08")
            //    || containername.Contains("E01")
            //    || containername.Contains("E06"))
            //{ FilterStr = "50UP"; }
            //else if (containername.Contains("E07")
            //    || containername.Contains("E10")
            //    || containername.Contains("E09")
            //    || containername.Contains("E03"))
            //{ FilterStr = "COB"; }

            var evalbin = "";
            if (containername.Contains("E"))
            {
                var idx = containername.IndexOf("E");
                evalbin = containername.Substring(idx, 3);
            }
            else if(containername.Contains("R"))
            {
                var idx = containername.IndexOf("R");
                evalbin = "E"+containername.Substring(idx+1, 2);
            }
            else if (containername.Contains("T"))
            {
                var idx = containername.IndexOf("T");
                evalbin = "E" + containername.Substring(idx + 1, 2);
            }

            var sql = "";
            if (!string.IsNullOrEmpty(evalbin))
            {
                sql = "select EvalPN from WAT.dbo.WXEvalPN where WaferNum = '<WaferNum>' and EvalBinName = '<evalbin>'";
                sql = sql.Replace("<WaferNum>", wafernum).Replace("<evalbin>", evalbin);
            }
            else
            {
                sql = "select EvalPN from WAT.dbo.WXEvalPN where WaferNum = '<WaferNum>' and EvalBinName = '<evalbin>'";
                sql = sql.Replace("<WaferNum>", wafernum).Replace("<evalbin>", "E01");
            }
            
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            { return UT.O2S(line[0]); }

            if (evalbin.Contains("E08")|| evalbin.Contains("R08")|| evalbin.Contains("T08"))
            {
                sql = "select EvalPN from WAT.dbo.WXEvalPN where WaferNum = '<WaferNum>' and EvalBinName = '<evalbin>'";
                sql = sql.Replace("<WaferNum>", wafernum).Replace("<evalbin>", "E01");
            }
            dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            { return UT.O2S(line[0]); }

            return string.Empty;
        }

        public static string GetLotTypeByWaferNum(string wafernum)
        {
            var sql = "select LotType from WAT.dbo.WXEvalPN where WaferNum = '<WaferNum>'";
            sql = sql.Replace("<WaferNum>", wafernum);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            { return UT.O2S(line[0]); }

            return "W";
        }



        public string WaferNum { set; get; }
        public string EvalPN { set; get; }
        public string DCDName { set; get; }
        public string LotType { set; get; }

    }
}
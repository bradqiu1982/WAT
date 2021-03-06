﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
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

        private static string GetProdFamByWaferNum(string wafernum)
        {
            var sql = "select Product from WAT.dbo.WXEvalPN where WaferNum = '<WaferNum>'";
            sql = sql.Replace("<WaferNum>", wafernum);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            { return UT.O2S(line[0]); }

            return "";
        }

        public static Dictionary<string, string> GetProdFamByWaferListAllen(List<string> wflist)
        {
            var dict = new Dictionary<string, string>();
            foreach (var wf in wflist)
            {
                var fam = GetProdFamByWaferNum(wf);
                if (!string.IsNullOrEmpty(fam) && !dict.ContainsKey(wf))
                {
                    dict.Add(wf, fam);
                }
                else if (!string.IsNullOrEmpty(fam))
                {
                    fam = GetProductFamilyFromAllen(wf);
                    if (!string.IsNullOrEmpty(fam) && !dict.ContainsKey(wf))
                    {
                        dict.Add(wf, fam);
                    }
                }
            }
            return dict;
        }

        private static void UpdateLotTypeFromAllen(string wafernum)
        {
            var sql = @"select containertype from insite.insite.container  where containername = @wafernum";

            var dict = new Dictionary<string, string>();
            dict.Add("@wafernum", wafernum);

            var dbret = DBUtility.ExeAllenSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                sql = "update WAT.dbo.WXEvalPN set LotType = @LotType where WaferNum = @WaferNum";
                dict = new Dictionary<string, string>();
                dict.Add("@WaferNum", wafernum);
                dict.Add("@LotType", UT.O2S(line[0]));
                DBUtility.ExeLocalSqlNoRes(sql, dict);
                return;
            }

        }

        public static string GetProductFamilyFromSherman(string wafernum)
        {
            var sql = @"SELECT distinct LEFT(pf.[ProductFamilyName],4) AS PRODUCT_FAMILY
                    FROM [SHM-CSSQL].[Insite].[insite].[Container] c with(nolock)
                    INNER JOIN [SHM-CSSQL].[Insite].[insite].[Product] p with(nolock) ON p.[ProductId] = c.[ProductId] 
                    INNER JOIN [SHM-CSSQL].[Insite].[insite].[ProductFamily] pf with(nolock) ON pf.[ProductFamilyId] = p.[ProductFamilyId]
                    WHERE c.[ContainerName] =  @wafernum";

            var dict = new Dictionary<string, string>();
            dict.Add("@wafernum", wafernum);

            var dbret = DBUtility.ExeShermanSqlWithRes(sql,dict);
            foreach (var line in dbret)
            { return UT.O2S(line[0]); }

            return string.Empty;
        }

        //private static void UpdateLotTypeFromSherman(string wafernum)
        //{
        //    var sql = @"SELECT ContainerType FROM [SHM-CSSQL].[Insite].[insite].[Container] WHERE [ContainerName] like '%<wafernum>%'";
        //    sql = sql.Replace("<wafernum>", wafernum);

        //    var dbret = DBUtility.ExeShermanSqlWithRes(sql);
        //    foreach (var line in dbret)
        //    {
        //        sql = "update WAT.dbo.WXEvalPN set LotType = @LotType where WaferNum = @WaferNum";
        //        var dict = new Dictionary<string, string>();
        //        dict.Add("@WaferNum", wafernum);
        //        dict.Add("@LotType", UT.O2S(line[0]));
        //        DBUtility.ExeLocalSqlNoRes(sql, dict);
        //        return;
        //    }

        //}


        private static bool PrepareAllenEvalPN(string wafernum)
        {
            var pdfm = GetProductFamilyFromAllen(wafernum);
            if (string.IsNullOrEmpty(pdfm))
            { return false; }

            var array = WXLogic.WATSampleXY.GetArrayFromAllenSherman(wafernum);

            var sql = @"SELECT pb.productname  FROM [Insite].[Insite].[Container] c with(nolock)
                          inner join insite.insite.product p with(nolock) on p.productid=c.productid
                          inner join insite.insite.productbase pb with(nolock) on pb.productbaseid=p.productbaseid
                          inner join insite.insite.productfamily pf with(nolock) on pf.productfamilyid=p.productfamilyid
                          inner join insite.insite.factory f with(nolock) on f.factoryid=p.factoryid
                          where f.factoryname = 'chip'
                          AND c.containername=@wafernum";

            var dict = new Dictionary<string, string>();
            dict.Add("@wafernum", wafernum);
            var dbret = DBUtility.ExeAllenSqlWithRes(sql, dict);
            if (dbret.Count > 0)
            {
                sql = "delete from WAT.dbo.WXEvalPN where WaferNum=@WaferNum";
                dict = new Dictionary<string, string>();
                dict.Add("@WaferNum", wafernum);
                DBUtility.ExeLocalSqlNoRes(sql, dict);
            }
            else
            { return false; }

            var templist = new List<WXEvalPN>();

            foreach (var line in dbret)
            {
                var pdname = UT.O2S(line[0]);

                var tempvm = new WXEvalPN();
                tempvm.EvalPN = pdname + "_B";
                tempvm.DCDName = "Eval_50up_rp";
                tempvm.LotType = "W";
                tempvm.EvalBin = "E08";
                templist.Add(tempvm);

                //tempvm = new WXEvalPN();
                //tempvm.EvalPN = pdname + "_C";
                //tempvm.DCDName = "Eval_50up_rp";
                //tempvm.LotType = "W";
                //tempvm.EvalBin = "E06";
                //templist.Add(tempvm);

                //tempvm = new WXEvalPN();
                //tempvm.EvalPN = pdname + "_T";
                //tempvm.DCDName = "Eval_COB_rp";
                //tempvm.LotType = "W";
                //tempvm.EvalBin = "E07";
                //templist.Add(tempvm);

                tempvm = new WXEvalPN();
                tempvm.EvalPN = pdname + "_HB";
                tempvm.DCDName = "Eval_COB_rp";
                tempvm.LotType = "W";
                tempvm.EvalBin = "E10";
                templist.Add(tempvm);

                tempvm = new WXEvalPN();
                tempvm.EvalPN = pdname + "_U";
                tempvm.DCDName = "Eval_50up_rp";
                tempvm.LotType = "W";
                tempvm.EvalBin = "E09";
                templist.Add(tempvm);

                //var dcdname = UT.O2S(line[1]);
                //var evalbin = UT.O2S(line[2]).ToUpper();
                //if (evalbin.Contains("E08")
                //    || evalbin.Contains("E06")
                //    || evalbin.Contains("E01"))
                //{ dcdname = "Eval_50up_rp"; }

                //sql = @"insert into WAT.dbo.WXEvalPN(WaferNum,EvalPN,DCDName,EvalBinName,Product) values(@WaferNum,@EvalPN,@DCDName,@EvalBinName,@Product)";
                //dict = new Dictionary<string, string>();
                //dict.Add("@WaferNum",wafernum);
                //dict.Add("@EvalPN", evalpn);
                //dict.Add("@DCDName", dcdname);
                //dict.Add("@EvalBinName", evalbin);
                //dict.Add("@Product", pdfm);
                //DBUtility.ExeLocalSqlWithRes(sql, dict);
            }

            foreach (var item in templist)
            {
                sql = @"insert into WAT.dbo.WXEvalPN(WaferNum,EvalPN,DCDName,LotType,EvalBinName,Product,AppVal1) values(@WaferNum,@EvalPN,@DCDName,@LotType,@EvalBinName,@Product,@array)";
                dict = new Dictionary<string, string>();
                dict.Add("@WaferNum", wafernum);
                dict.Add("@EvalPN", item.EvalPN);
                dict.Add("@DCDName", item.DCDName);
                dict.Add("@LotType", item.LotType);
                dict.Add("@EvalBinName", item.EvalBin);
                dict.Add("@Product", pdfm);
                dict.Add("@array", array);
                DBUtility.ExeLocalSqlWithRes(sql, dict);
            }

            return true;
        }

        private static bool PrepareShermanEvalPN(string wafernum)
        {
            var pdfm = GetProductFamilyFromSherman(wafernum);
            if (string.IsNullOrEmpty(pdfm))
            { return false; }

            var array = WXLogic.WATSampleXY.GetArrayFromAllenSherman(wafernum);

            var sql = "delete from WAT.dbo.WXEvalPN where WaferNum=@WaferNum";
            var dict = new Dictionary<string, string>();
            dict.Add("@WaferNum", wafernum);
            DBUtility.ExeLocalSqlNoRes(sql, dict);

            var templist = new List<WXEvalPN>();
            sql = @"SELECT distinct pb.productname FROM [SHM-CSSQL].[Insite].[insite].[Container] c with(nolock)
                    INNER JOIN [SHM-CSSQL].[Insite].[insite].[Product] p with(nolock) ON p.[ProductId] = c.[ProductId] 
                    INNER JOIN [SHM-CSSQL].[Insite].[insite].[ProductBase] pb with(nolock)  ON p.[ProductBaseId] = pb.[ProductBaseId]
		            INNER JOIN [SHM-CSSQL].[Insite].[insite].[factory] f with(nolock) on f.factoryid=p.factoryid
                    WHERE c.[ContainerName] = @wafernum and f.factoryname  = 'chip'";

            var dbret = DBUtility.ExeShermanSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                var pdname = UT.O2S(line[0]);

                var tempvm = new WXEvalPN();
                tempvm.EvalPN =pdname + "_B";
                tempvm.DCDName = "Eval_50up_rp";
                tempvm.LotType = "W";
                tempvm.EvalBin = "E08";
                templist.Add(tempvm);

                //tempvm = new WXEvalPN();
                //tempvm.EvalPN = pdname + "_C";
                //tempvm.DCDName = "Eval_50up_rp";
                //tempvm.LotType = "W";
                //tempvm.EvalBin = "E06";
                //templist.Add(tempvm);

                //tempvm = new WXEvalPN();
                //tempvm.EvalPN = pdname + "_T";
                //tempvm.DCDName = "Eval_COB_rp";
                //tempvm.LotType = "W";
                //tempvm.EvalBin = "E07";
                //templist.Add(tempvm);

                tempvm = new WXEvalPN();
                tempvm.EvalPN = pdname + "_HB";
                tempvm.DCDName = "Eval_COB_rp";
                tempvm.LotType = "W";
                tempvm.EvalBin = "E10";
                templist.Add(tempvm);

                tempvm = new WXEvalPN();
                tempvm.EvalPN = pdname + "_U";
                tempvm.DCDName = "Eval_50up_rp";
                tempvm.LotType = "W";
                tempvm.EvalBin = "E09";
                templist.Add(tempvm);
            }

            foreach (var item in templist)
            {
                sql = @"insert into WAT.dbo.WXEvalPN(WaferNum,EvalPN,DCDName,LotType,EvalBinName,Product,AppVal1) values(@WaferNum,@EvalPN,@DCDName,@LotType,@EvalBinName,@Product,@array)";
                dict = new Dictionary<string, string>();
                dict.Add("@WaferNum", wafernum);
                dict.Add("@EvalPN", item.EvalPN);
                dict.Add("@DCDName", item.DCDName);
                dict.Add("@LotType", item.LotType);
                dict.Add("@EvalBinName", item.EvalBin);
                dict.Add("@Product", pdfm);
                dict.Add("@array", array);
                DBUtility.ExeLocalSqlWithRes(sql, dict);
            }

            return true;
        }

        public static string GetLocalWaferArray(string wafer)
        {
            var dict = new Dictionary<string, string>();
            dict.Add("@WaferNum", wafer);

            var sql = "select top 1 AppVal1 from [WAT].[dbo].[WXEvalPN] where [WaferNum] = @WaferNum";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            var array = "";
            if (dbret.Count > 0)
            { array = UT.O2S(dbret[0][0]); }

            if (!string.IsNullOrEmpty(array))
            { return array; }
            else
            {
                array = WXLogic.WATSampleXY.GetArrayFromAllenSherman(wafer);
                if (!string.IsNullOrEmpty(array))
                {
                    dict.Add("@array", array);
                    sql = "update [WAT].[dbo].[WXEvalPN] set AppVal1 = @array where wafernum = @WaferNum";
                    DBUtility.ExeLocalSqlNoRes(sql, dict);
                }
                return array;
            }
        }

        //public static string GetLocalProductFam(string wafer)
        //{
        //    var dict = new Dictionary<string, string>();
        //    dict.Add("@WaferNum", wafer);

        //    var sql = "select top 1 Product from [WAT].[dbo].[WXEvalPN] where [WaferNum] = @WaferNum";
        //    var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
        //    var product = "";
        //    if (dbret.Count > 0)
        //    { product = UT.O2S(dbret[0][0]); }

        //    if (!string.IsNullOrEmpty(product))
        //    { return product; }
        //    else
        //    {
        //        PrepareEvalPN(wafer);
        //        dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
        //        if (dbret.Count > 0)
        //        { product = UT.O2S(dbret[0][0]); }
        //        return product;
        //    }
        //}

        public static bool PrepareEvalPN(string wafernum)
        {
            if (wafernum.Length == 13)
            {
                var shermanret = PrepareShermanEvalPN(wafernum);
                //UpdateLotTypeFromSherman(wafernum);
                if (!shermanret)
                { return false; }
            }
            else
            {
                var allenret = PrepareAllenEvalPN(wafernum);
                //UpdateLotTypeFromAllen(wafernum);
                if (!allenret)
                {
                    var shermanret = PrepareShermanEvalPN(wafernum);
                    //UpdateLotTypeFromSherman(wafernum);
                    if (!shermanret)
                    { return false; }
                }
            }
            return true;
        }

        public static Dictionary<string, string> GetWaferProdfamDict()
        {
            var ret = new Dictionary<string, string>();
            var sql = @"select distinct wafernum,product from [WAT].[dbo].[WXEvalPN] where product <> ''";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var wf = UT.O2S(line[0]);
                var prodfam = UT.O2S(line[1]);
                if (!ret.ContainsKey(wf))
                { ret.Add(wf, prodfam); }
            }
            return ret;
        }

        public WXEvalPN()
        {
            WaferNum = "";
            EvalPN = "";
            DCDName = "";
            LotType = "";
            EvalBin = "";
        }

        public string WaferNum { set; get; }
        public string EvalPN { set; get; }
        public string DCDName { set; get; }
        public string LotType { set; get; }
        public string EvalBin { set; get; }

    }
}
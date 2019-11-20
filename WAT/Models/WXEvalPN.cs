using System;
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
                    INNER JOIN [SHM-CSSQL].[Insite].[insite].[ProductBase] pb with(nolock) ON pb.[RevOfRcdId] = c.[ProductId] 
                    INNER JOIN [SHM-CSSQL].[Insite].[insite].[Product] p with(nolock) ON p.[ProductBaseId] = pb.[ProductBaseId] 
                    INNER JOIN [SHM-CSSQL].[Insite].[insite].[ProductFamily] pf with(nolock) ON pf.[ProductFamilyId] = p.[ProductFamilyId]
                    WHERE c.[ContainerName] like '%<wafernum>%'";

            sql = sql.Replace("<wafernum>", wafernum);

           var dbret = DBUtility.ExeShermanSqlWithRes(sql);
            foreach (var line in dbret)
            { return UT.O2S(line[0]); }

            return string.Empty;
        }

        private static void UpdateLotTypeFromSherman(string wafernum)
        {
            var sql = @"SELECT ContainerType FROM [SHM-CSSQL].[Insite].[insite].[Container] WHERE [ContainerName] like '%<wafernum>%'";
            sql = sql.Replace("<wafernum>", wafernum);

            var dbret = DBUtility.ExeShermanSqlWithRes(sql);
            foreach (var line in dbret)
            {
                sql = "update WAT.dbo.WXEvalPN set LotType = @LotType where WaferNum = @WaferNum";
                var dict = new Dictionary<string, string>();
                dict.Add("@WaferNum", wafernum);
                dict.Add("@LotType", UT.O2S(line[0]));
                DBUtility.ExeLocalSqlNoRes(sql, dict);
                return;
            }

        }


        private static bool PrepareAllenEvalPN(string wafernum)
        {
            var pdfm = GetProductFamilyFromAllen(wafernum);
            if (string.IsNullOrEmpty(pdfm))
            { return false; }

            var sql = @"select distinct fj.EvalPartNumber,left(spec.DCDefName,len(spec.DCDefName)-2),EvalBinName,fj.[Date] from [EngrData].[insite].[FinEvalJobStartInfo] fj with (nolock)  
                        left join [EngrData].[insite].[Eval_Specs_Bin_PassFail] spec with (nolock) on spec.Eval_ProductName = fj.EvalPartNumber
                        where fj.Device= @pdfm and fj.EvalPartNumber is not null and spec.DCDefName is not null and [Date] in 
						(select Max([Date]) from [EngrData].[insite].[FinEvalJobStartInfo] where device = @pdfm group by EvalBinName) order by fj.[Date] desc";

            var dict = new Dictionary<string, string>();
            dict.Add("@pdfm", pdfm);
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

            foreach (var line in dbret)
            {
                var evalpn = UT.O2S(line[0]);
                var dcdname = UT.O2S(line[1]);
                var evalbin = UT.O2S(line[2]);

                sql = @"insert into WAT.dbo.WXEvalPN(WaferNum,EvalPN,DCDName,EvalBinName,Product) values(@WaferNum,@EvalPN,@DCDName,@EvalBinName,@Product)";
                dict = new Dictionary<string, string>();
                dict.Add("@WaferNum",wafernum);
                dict.Add("@EvalPN", evalpn);
                dict.Add("@DCDName", dcdname);
                dict.Add("@EvalBinName", evalbin);
                dict.Add("@Product", pdfm);
                DBUtility.ExeLocalSqlWithRes(sql, dict);
            }

            return true;
        }

        private static bool PrepareShermanEvalPN(string wafernum)
        {
            var pdfm = GetProductFamilyFromSherman(wafernum);
            if (string.IsNullOrEmpty(pdfm))
            { return false; }


            var sql = "delete from WAT.dbo.WXEvalPN where WaferNum=@WaferNum";
            var dict = new Dictionary<string, string>();
            dict.Add("@WaferNum", wafernum);
            DBUtility.ExeLocalSqlNoRes(sql, dict);


            var templist = new List<WXEvalPN>();
            var tempvm = new WXEvalPN();
            tempvm.EvalPN = pdfm + "001";
            tempvm.DCDName = "Eval_50up_rp";
            templist.Add(tempvm);

            tempvm = new WXEvalPN();
            tempvm.EvalPN = pdfm + "002";
            tempvm.DCDName = "Eval_COB_rp";
            templist.Add(tempvm);

            foreach (var item in templist)
            {
                sql = @"insert into WAT.dbo.WXEvalPN(WaferNum,EvalPN,DCDName,EvalBinName,Product) values(@WaferNum,@EvalPN,@DCDName,@EvalBinName,@Product)";
                dict = new Dictionary<string, string>();
                dict.Add("@WaferNum", wafernum);
                dict.Add("@EvalPN", tempvm.EvalPN);
                dict.Add("@DCDName", tempvm.DCDName);
                dict.Add("@EvalBinName", "E08");
                dict.Add("@Product", pdfm);
                DBUtility.ExeLocalSqlWithRes(sql, dict);
            }

            return true;
        }

        public static bool PrepareEvalPN(string wafernum)
        {
            var allenret = PrepareAllenEvalPN(wafernum);
            UpdateLotTypeFromAllen(wafernum);
            if (!allenret)
            {
                var shermanret = PrepareShermanEvalPN(wafernum);
                UpdateLotTypeFromSherman(wafernum);
                if (!shermanret)
                { return false; }
            }
            return true;
        }
        
        public string WaferNum { set; get; }
        public string EvalPN { set; get; }
        public string DCDName { set; get; }
        public string LotType { set; get; }

    }
}
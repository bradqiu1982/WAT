using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WAT.Models
{
    public class AllenVcselPNs
    {
        public static AllenVcselPNs GetPNsFromAllen(string wafer,string bin,Dictionary<string, AllenVcselPNs> localdata)
        {
            var ret = new AllenVcselPNs();

            var dict = new Dictionary<string, string>();
            dict.Add("@wafer", wafer);
            var sql = "SELECT distinct [Product] FROM [EngrData].[dbo].[WXEvalPN] where WaferNum = @wafer";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            if (dbret.Count > 0)
            {
                var product = UT.O2S(dbret[0][0]);
                sql = @"SELECT [ChipPartNumber] unsorted_pn
                          ,[UnInspectedPart]
                          ,[InspectedPart] bom_pn
                          ,[BinProdfam]
                      FROM [Insite].[insite].[BinJobPartnumbers] where device=@product and binnumber = @bin";
                dict = new Dictionary<string, string>();
                dict.Add("@product", product);
                dict.Add("@bin", bin);

                dbret = DBUtility.ExeAllenSqlWithRes(sql, dict);
                foreach (var line in dbret)
                {
                    ret.UnsortedPN = UT.O2S(line[0]);
                    ret.UninspectPN = UT.O2S(line[1]);
                    ret.BomPN = UT.O2S(line[2]);
                    ret.PNBIN = UT.O2S(line[3]);
                    return ret;
                }

                if (string.IsNullOrEmpty(ret.UnsortedPN))
                {
                    return GetDataFromLocal(product, bin, localdata);
                }
            }

            return ret;
        }

        public static Dictionary<string, AllenVcselPNs> GetMapFromLocal(Controller ctrl)
        {
            var data = ExcelReader.RetrieveDataFromExcel(ctrl.Server.MapPath("~/Scripts") + "\\" + "VCSELPN.xlsx", null, 12);
            var ret = new Dictionary<string, AllenVcselPNs>();
            foreach (var line in data)
            {
                var tempvm = new AllenVcselPNs();
                tempvm.PNBIN = UT.O2S(line[0]);
                tempvm.UnsortedPN = UT.O2S(line[5]);
                tempvm.UninspectPN = UT.O2S(line[4]);
                tempvm.BomPN = UT.O2S(line[9]);

                if (!ret.ContainsKey(tempvm.PNBIN))
                { ret.Add(tempvm.PNBIN, tempvm); }
            }
            return ret;
        }

        private static AllenVcselPNs GetDataFromLocal(string product, string bin, Dictionary<string, AllenVcselPNs> localdata)
        {
            var sql = @"SELECT left(binprodfam,4),count(left(binprodfam,4)) cnt
                      FROM [Insite].[insite].[BinJobPartnumbers] where device = @product group by left(binprodfam,4) order by cnt desc";
            var dict = new Dictionary<string, string>();
            dict.Add("@product", product);
            var dbret = DBUtility.ExeAllenSqlWithRes(sql, dict);
            if (dbret.Count > 0)
            {
                var key = UT.O2S(dbret[0][0]) + "-0" + bin;
                if (localdata.ContainsKey(key))
                { return localdata[key]; }
            }

            return new AllenVcselPNs();
        }

        public AllenVcselPNs()
        {
            UnsortedPN = "";
            UninspectPN = "";
            BomPN = "";
            PNBIN = "";
        } 


        public string UnsortedPN { set; get; }
        public string UninspectPN { set; get; }
        public string BomPN { set; get; }
        public string PNBIN { set; get;}
    }
}
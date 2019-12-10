using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Xml;

namespace WAT.Models
{
    public class WATSampleXY
    {
        private static string GetArrayFromDieSort(string wafer)
        {
            var dict = new Dictionary<string, string>();
            dict.Add("@wafer", wafer);
            var sql = "SELECT top 1 [PArray] FROM [WAT].[dbo].[WaferSampleData] where WAFER = @wafer";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                return Convert.ToString(line[0]).ToUpper().Replace("1X", "");
            }
            return string.Empty;
        }

        public static int Get_First_Singlet_From_Array_Coord(int DIE_ONE_X, int DIE_ONE_FIELD_MIN_X, int arrayx, int Array_Count)
        {
            var new_x = ((arrayx - DIE_ONE_X
                + Math.Floor((double)(DIE_ONE_X - DIE_ONE_FIELD_MIN_X) / Array_Count)) * Array_Count)
                + DIE_ONE_FIELD_MIN_X;
            if (Array_Count != 1)
            { return (int)Math.Round(new_x, 0) - 1; }
            else
            { return (int)Math.Round(new_x, 0); }
        }


        public static string GetWATSampleSingletCoordination(string wafer)
        {
            var array =  WXLogic.WATSampleXY.GetArrayFromAllenSherman(wafer);
            if (string.IsNullOrEmpty(array))
            { return string.Empty; }
            var arraysize = UT.O2I(array);

            var sql = "select distinct X,Y from [WAT].[dbo].[WaferSampleData] where wafer like '<wafer>%'";
            sql = sql.Replace("<wafer>", wafer);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            if (dbret.Count > 0)
            {
                var dieonex = WXLogic.AdminFileOperations.GetDieOneByWafer(wafer);
                if (dieonex.Count > 0)
                {
                    var ret = new List<object>();
                    foreach (var line in dbret)
                    {
                        var ax = UT.O2S(line[0]);
                        var y = UT.O2S(line[1]);
                        var x = Get_First_Singlet_From_Array_Coord(dieonex[0], dieonex[1], UT.O2I(ax), arraysize).ToString();
                        ret.Add(new
                        {
                            x = x,
                            y = y
                        });

                        for (var idx = 1; idx < arraysize; idx++)
                        {
                            ret.Add(new
                            {
                                x = (UT.O2I(x)+idx).ToString(),
                                y = y
                            });
                        }
                    }
                    return Newtonsoft.Json.JsonConvert.SerializeObject(ret);
                }
                else
                {
                    return string.Empty;
                }
            }
            else
            {
                var productfm = "";
                if (wafer.Length == 13)
                {
                    productfm = WXEvalPN.GetProductFamilyFromSherman(wafer);
                }
                else
                {
                    productfm = WXEvalPN.GetProductFamilyFromAllen(wafer);
                    if (string.IsNullOrEmpty(productfm))
                    {
                        productfm = WXEvalPN.GetProductFamilyFromSherman(wafer);
                    }
                }

                var syscfgdict = CfgUtility.GetSysConfig(null);
                var reviewfolder = syscfgdict["DIESORTREVIEW"];
                var allfiles = ExternalDataCollector.DirectoryEnumerateAllFiles(null, reviewfolder);
                var fs = "";
                foreach (var f in allfiles)
                {
                    if (f.Contains(wafer))
                    { fs = f; break; }
                }

                if (string.IsNullOrEmpty(fs))
                {
                    allfiles = ExternalDataCollector.DirectoryEnumerateAllFiles(null, syscfgdict["DIESORTFOLDER"]+"\\"+productfm);
                    foreach (var f in allfiles)
                    {
                        if (f.Contains(wafer))
                        { fs = f; break; }
                    }
                }
                if (string.IsNullOrEmpty(fs))
                {
                    allfiles = ExternalDataCollector.DirectoryEnumerateAllFiles(null, syscfgdict["DIESORTFOLDER"]);
                    foreach (var f in allfiles)
                    {
                        if (f.Contains(wafer))
                        { fs = f; break; }
                    }
                }

                if (string.IsNullOrEmpty(fs))
                { return string.Empty; }

                var dieonex = WXLogic.AdminFileOperations.GetDieOneByFile(fs);
                if (dieonex.Count == 0)
                { return string.Empty; }

                var ret = new List<object>();
                var folderuser = syscfgdict["SHAREFOLDERUSER"];
                var folderdomin = syscfgdict["SHAREFOLDERDOMIN"];
                var folderpwd = syscfgdict["SHAREFOLDERPWD"];
                using (NativeMethods cv = new NativeMethods(folderuser, folderdomin, folderpwd))
                {
                    var doc = new XmlDocument();
                    doc.Load(fs);
                    var namesp = doc.DocumentElement.GetAttribute("xmlns");
                    doc = DieSortVM.StripNamespace(doc);
                    XmlElement root = doc.DocumentElement;

                    var passbinnodes = root.SelectNodes("//BinDefinition[@BinQuality='Pass' and @BinCode and @Pick='true']");
                    if (passbinnodes.Count == 0)
                    { return string.Empty; }

                    var passbinlist = new List<string>();
                    foreach (XmlElement nd in passbinnodes)
                    { passbinlist.Add(nd.GetAttribute("BinCode")); }

                    var bincodelist = root.SelectNodes("//BinCode[@X and @Y]");
                    foreach (XmlElement nd in bincodelist)
                    {
                        try
                        {
                            if (passbinlist.Contains(nd.InnerText))
                            {
                                var ax = nd.GetAttribute("X");
                                var y = nd.GetAttribute("Y");
                                var x = Get_First_Singlet_From_Array_Coord(dieonex[0], dieonex[1], UT.O2I(ax), arraysize).ToString();
                                ret.Add(new
                                {
                                    x = x,
                                    y = y
                                });

                                for (var idx = 1; idx < arraysize; idx++)
                                {
                                    ret.Add(new
                                    {
                                        x = (UT.O2I(x) + idx).ToString(),
                                        y = y
                                    });
                                }
                            }
                        }
                        catch (Exception ex) { }
                    }
                }

                if (ret.Count > 0)
                { return Newtonsoft.Json.JsonConvert.SerializeObject(ret); }
                else
                { return string.Empty; }
            }
        }


        WATSampleXY()
        {
            CouponID = "";
            ChannelInfo = "";
            X = "";
            Y = "";
        }

        public string CouponID { set; get; }
        public string ChannelInfo { set; get; }
        public string X { set; get; }
        public string Y { set; get; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class WaferQUALVM
    {
        private static Dictionary<string, DateTime> GetWaferDict()
        {
            var ret = new Dictionary<string, DateTime>();
            var sql = "select WaferNum,ComingDate from WaferQUALVM";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var w = Convert.ToString(line[0]);
                var d = Convert.ToDateTime(line[1]);
                if (!ret.ContainsKey(w))
                {
                    ret.Add(w, d);
                }
            }
            return ret;
        }

        private static bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        public static void LoadNewWaferFromMES()
        {
            var wdict = GetWaferDict();
            var LatestDate = "";
            if (wdict.Count > 0)
            {
                var ds = wdict.Values.ToList();
                ds.Sort();
                LatestDate = ds[ds.Count - 1].ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            { LatestDate = "2018-01-01 00:00:00"; }

            var vdict = WaferPN.GetVcselDict();
            var vlist = vdict.Keys.ToList();
            var pncond = "('" + string.Join("','", vlist) + "')";
            var tempdict = new Dictionary<string, bool>();
            var waferlist = new List<WaferQUALVM>();


            var sql = @"SELECT distinct  Left(aoc.ParamValueString,9) as Wafer,MIN(hml.MfgDate) as MINTIME, pb.ProductName
                        FROM [InsiteDB].[insite].[dc_IQC_InspectionResult] (nolock) aoc 
                     left join [InsiteDB].[insite].HistoryMainline (nolock) hml on aoc.[HistoryMainlineId] = hml.HistoryMainlineId
                     left join [InsiteDB].[insite].Product (nolock) pd on pd.ProductId = hml.ProductId
                     left join [InsiteDB].[insite].ProductBase (nolock) pb on pb.ProductBaseId = pd.ProductBaseId
                     where hml.MfgDate > '<LatestDate>' and  hml.MfgDate < '<NOW>'  and pb.ProductName in <pncond> and Len(ParamValueString) >= 9 
                      and Left(ParamValueString,9) <> '1900-01-0' and ParamValueString like'%-%' group by Left(aoc.ParamValueString,9),pb.ProductName";
            sql = sql.Replace("<LatestDate>", LatestDate).Replace("<NOW>", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Replace("<pncond>", pncond);
            var dbret = DBUtility.ExeMESSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var wafer = Convert.ToString(line[0]);
                var comingdate = Convert.ToDateTime(line[1]).ToString("yyyy-MM-dd HH:mm:ss");
                var pn = Convert.ToString(line[2]);
                if (!wdict.ContainsKey(wafer) && !tempdict.ContainsKey(wafer))
                {
                    tempdict.Add(wafer, true);
                    if ((wafer.Length - wafer.Replace("-", "").Length) > 1 || !IsDigitsOnly(wafer.Replace("-", "")))
                    { continue; }

                    var tempvm = new WaferQUALVM();
                    tempvm.WaferNum = wafer;
                    tempvm.ComingDate = comingdate;
                    tempvm.PN = pn;
                    waferlist.Add(tempvm);
                }
            }

            sql = @"SELECT distinct  Left(aoc.ParamValueString,9) as Wafer,MIN(hml.MfgDate) as MINTIME, pb.ProductName
                        FROM [InsiteDB].[insite].[dc_AOC_ManualInspection] (nolock) aoc 
                     left join [InsiteDB].[insite].HistoryMainline (nolock) hml on aoc.[HistoryMainlineId] = hml.HistoryMainlineId
                     left join [InsiteDB].[insite].Product (nolock) pd on pd.ProductId = hml.ProductId
                     left join [InsiteDB].[insite].ProductBase (nolock) pb on pb.ProductBaseId = pd.ProductBaseId
                     where hml.MfgDate > '<LatestDate>' and  hml.MfgDate < '<NOW>'  and pb.ProductName in <pncond> and Len(ParamValueString) >= 9 
                      and Left(ParamValueString,9) <> '1900-01-0' and ParamValueString like'%-%' group by Left(aoc.ParamValueString,9),pb.ProductName";
            sql = sql.Replace("<LatestDate>", LatestDate).Replace("<NOW>", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Replace("<pncond>", pncond);
            dbret = DBUtility.ExeMESSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var wafer = Convert.ToString(line[0]);
                var comingdate = Convert.ToDateTime(line[1]).ToString("yyyy-MM-dd HH:mm:ss");
                var pn = Convert.ToString(line[2]);
                if (!wdict.ContainsKey(wafer) && !tempdict.ContainsKey(wafer))
                {
                    tempdict.Add(wafer, true);
                    if ((wafer.Length - wafer.Replace("-", "").Length) > 1 || !IsDigitsOnly(wafer.Replace("-", "")))
                    { continue; }

                    var tempvm = new WaferQUALVM();
                    tempvm.WaferNum = wafer;
                    tempvm.ComingDate = comingdate;
                    tempvm.PN = pn;
                    waferlist.Add(tempvm);
                }
            }

            foreach (var vm in waferlist)
            {
                var csql = "insert into WaferQUALVM(WaferNum,ComingDate,PN) values(@WaferNum,@ComingDate,@PN)";
                var dict = new Dictionary<string, string>();
                dict.Add("@WaferNum",vm.WaferNum);
                dict.Add("@ComingDate",vm.ComingDate);
                dict.Add("@PN",vm.PN);
                DBUtility.ExeLocalSqlNoRes(csql, dict);
            }
            
        }

        private static void LoadWUXIYield(List<string> wlist)
        {
            var wydict = new Dictionary<string, WaferQUALVM>();

            var wcond = "('" + string.Join("','", wlist) + "')";
            var sql = "select WaferNo,Failure,Num from HTOLWaferTestSum where WhichTest = 'Post HTOL Burn In' and WaferNo in <WAFERCOND> order by WaferNo";
            sql = sql.Replace("<WAFERCOND>", wcond);
            var dbret = DBUtility.ExeNPITraceSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var wf = Convert.ToString(line[0]);
                var failure = Convert.ToString(line[1]);
                var num = Convert.ToInt32(line[2]);
                if (wydict.ContainsKey(wf))
                {
                    wydict[wf].WXQUALTotal += num;
                    if (failure.ToUpper().Contains("PASS"))
                    { wydict[wf].WXQUALPass += num; }
                }
                else
                {
                    var tempvm = new WaferQUALVM();
                    tempvm.WXQUALTotal = num;
                    if (failure.ToUpper().Contains("PASS"))
                    { tempvm.WXQUALPass = num; }
                    wydict.Add(wf, tempvm);
                }
            }

            foreach (var kv in wydict)
            {
                var csql = "update WaferQUALVM set WXQUALPass = @WXQUALPass,WXQUALTotal = @WXQUALTotal where WaferNum = @WaferNum";
                var dict = new Dictionary<string, string>();
                dict.Add("@WaferNum",kv.Key);
                dict.Add("@WXQUALPass",kv.Value.WXQUALPass.ToString());
                dict.Add("@WXQUALTotal",kv.Value.WXQUALTotal.ToString());
                DBUtility.ExeLocalSqlNoRes(csql, dict);
            }
        }

        public static void LoadWUXIWaferQUAL()
        {
            var vdict = WaferPN.GetVcselDict();
            var wlist = new List<string>();
            var sql = "select WaferNum,PN from WaferQUALVM where WXQUALTotal = '0'";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var wf = Convert.ToString(line[0]);
                var pn = Convert.ToString(line[1]);
                if (vdict.ContainsKey(pn))
                {
                    if (!vdict[pn].Desc.Contains("10G"))
                    { wlist.Add(wf); }
                }//end if
            }//end foreach

            LoadWUXIYield(wlist);
        }


        public static List<WaferQUALVM> RetrieveWaferData(string startdate, string enddate)
        {
            var vdict = WaferPN.GetVcselDict();
            var ret = new List<WaferQUALVM>();

            var sql = "select WaferNum,ComingDate,PN,WXQUALPass,WXQUALTotal from WaferQUALVM where ComingDate > '<startdate>' and ComingDate < '<enddate>' order by ComingDate desc";
            sql = sql.Replace("<startdate>", startdate).Replace("<enddate>", enddate);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var tempvm = new WaferQUALVM();
                tempvm.WaferNum = Convert.ToString(line[0]);
                tempvm.ComingDate = Convert.ToDateTime(line[1]).ToString("yyyy-MM-dd");
                tempvm.PN = Convert.ToString(line[2]);
                tempvm.WXQUALPass = Convert.ToInt32(line[3]);
                tempvm.WXQUALTotal = Convert.ToInt32(line[4]);
                ret.Add(tempvm);
            }

            foreach (var vm in ret)
            {
                if (vdict.ContainsKey(vm.PN))
                {
                    vm.VArray = vdict[vm.PN].PArray;
                    vm.VRate = vdict[vm.PN].Desc;
                    vm.VTech = vdict[vm.PN].Tech;
                }
            }
            return ret;
        }

        public WaferQUALVM()
        {
            WaferNum = "";
            ComingDate = "1982-05-06 10:00:00";
            PN = "";
            WXQUALPass = 0;
            WXQUALTotal = 0;
            VArray = "";
            VRate = "";
            VTech = "";
        }


        public string WaferNum { set; get; }
        public string ComingDate { set; get; }
        public string PN { set; get; }
        public int WXQUALPass { set; get; }
        public int WXQUALTotal { set; get; }
        public string WXQUALYield { get {
                if (WXQUALTotal == 0)
                { return string.Empty; }

                return Math.Round((double)WXQUALPass/(double)WXQUALTotal*100,2).ToString();
            } }

        public string VArray { set; get; }
        public string VRate { set; get; }
        public string VTech { set; get; }
    }
}
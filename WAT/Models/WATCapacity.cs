using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace WAT.Models
{
    public class WATCapacity
    {

        public static List<WATCapacity> GetWATCapacity(string starttime)
        {
            var ret = new List<WATCapacity>();

            var dict = new Dictionary<string, string>();
            dict.Add("@starttime", starttime);

            var sql = @"select left(c.containername,10) as wafer,min(c.TestTimeStamp) as mintime,REPLACE(REPLACE('1x'+ep.AppVal1+ ' ' +r.RealRate,'14G','10G'),'28G','25G') as vtype FROM [Insite].[dbo].[ProductionResult] c with(nolock) 
                      left join wat.dbo.WXEvalPN ep with (nolock) on ep.WaferNum = left(c.Containername,9)
                      left join wat.dbo.WXEvalPNRate r on left(ep.EvalPN,7) = r.EvalPN
                      where len(c.Containername) = 20 and c.TestStep = 'PRLL_VCSEL_Post_Burn_in_Test' and c.TestTimeStamp > @starttime  and  r.RealRate is not null  and (c.Containername like '%E08%' or c.Containername like '%R08%' or c.Containername like '%T08%')
                      group by left(c.containername,10),r.RealRate,ep.AppVal1  order by mintime asc";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                var tempvm = new WATCapacity();
                tempvm.Wafer = UT.O2S(line[0]).ToUpper();
                tempvm.WFDate = UT.O2T(line[1]).AddHours(-23);
                tempvm.VType = UT.O2S(line[2]);
                ret.Add(tempvm);
            }

            sql = @"select left(c.containername,14) as wafer,min(c.TestTimeStamp) as mintime,REPLACE(REPLACE('1x'+ep.AppVal1+ ' ' +r.RealRate,'14G','10G'),'28G','25G') as vtype FROM [Insite].[dbo].[ProductionResult] c with(nolock) 
                  left join wat.dbo.WXEvalPN ep with (nolock) on ep.WaferNum = left(c.Containername,13)
                  left join wat.dbo.WXEvalPNRate r on left(ep.EvalPN,7) = r.EvalPN
                  where len(c.Containername) = 24 and c.TestStep = 'PRLL_VCSEL_Post_Burn_in_Test' and c.TestTimeStamp > @starttime   and  r.RealRate is not null  and (c.Containername like '%E08%' or c.Containername like '%R08%' or c.Containername like '%T08%')
                  group by left(c.containername,14),r.RealRate,ep.AppVal1  order by mintime asc";
            dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                var tempvm = new WATCapacity();
                tempvm.Wafer = UT.O2S(line[0]).ToUpper();
                tempvm.WFDate = UT.O2T(line[1]).AddHours(-23);
                tempvm.VType = UT.O2S(line[2]);
                ret.Add(tempvm);
            }

            ret.Sort(delegate(WATCapacity obj1,WATCapacity obj2)
            {
               return obj1.WFDate.CompareTo(obj2.WFDate);
            });

            return ret;
        }

        public static List<WATCapacity> GetWATHTOL2Wafer(string starttime)
        {
            var ret = new List<WATCapacity>();

            var wfdict = new Dictionary<string, string>();

            var dict = new Dictionary<string, string>();
            dict.Add("@starttime", starttime);

            var sql = @"select left(c.containername,10) as wafer,max(c.TestTimeStamp) as mintime,REPLACE(REPLACE('1x'+ep.AppVal1+ ' ' +r.RealRate,'14G','10G'),'28G','25G') as vtype FROM [Insite].[dbo].[ProductionResult] c with(nolock) 
                      left join wat.dbo.WXEvalPN ep with (nolock) on ep.WaferNum = left(c.Containername,9)
                      left join wat.dbo.WXEvalPNRate r on left(ep.EvalPN,7) = r.EvalPN
                      where len(c.Containername) = 20 and c.TestStep = 'PRLL_Post_HTOL2_Test' and c.TestTimeStamp > @starttime  and  r.RealRate is not null and (c.Containername like '%E08%' or c.Containername like '%R08%' or c.Containername like '%T08%')
                      group by left(c.containername,10),r.RealRate,ep.AppVal1  order by mintime asc";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                var tempvm = new WATCapacity();
                tempvm.Wafer = UT.O2S(line[0]).ToUpper();
                tempvm.WFDate = UT.O2T(line[1]).AddHours(-23);
                tempvm.VType = UT.O2S(line[2]);

                if (wfdict.ContainsKey(tempvm.Wafer))
                { continue; }
                wfdict.Add(tempvm.Wafer, tempvm.Wafer);

                ret.Add(tempvm);
            }

            sql = @"select left(c.containername,14) as wafer,max(c.TestTimeStamp) as mintime,REPLACE(REPLACE('1x'+ep.AppVal1+ ' ' +r.RealRate,'14G','10G'),'28G','25G') as vtype FROM [Insite].[dbo].[ProductionResult] c with(nolock) 
                  left join wat.dbo.WXEvalPN ep with (nolock) on ep.WaferNum = left(c.Containername,13)
                  left join wat.dbo.WXEvalPNRate r on left(ep.EvalPN,7) = r.EvalPN
                  where len(c.Containername) = 24 and c.TestStep = 'PRLL_Post_HTOL2_Test' and c.TestTimeStamp > @starttime   and  r.RealRate is not null  and (c.Containername like '%E08%' or c.Containername like '%R08%' or c.Containername like '%T08%')
                  group by left(c.containername,14),r.RealRate,ep.AppVal1  order by mintime asc";
            dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                var tempvm = new WATCapacity();
                tempvm.Wafer = UT.O2S(line[0]).ToUpper();
                tempvm.WFDate = UT.O2T(line[1]);
                tempvm.VType = UT.O2S(line[2]);

                if (wfdict.ContainsKey(tempvm.Wafer))
                { continue; }
                wfdict.Add(tempvm.Wafer, tempvm.Wafer);

                ret.Add(tempvm);
            }

            sql = @"select left(c.containername,10) as wafer,max(c.TestTimeStamp) as mintime,REPLACE(REPLACE('1x'+ep.AppVal1+ ' ' +r.RealRate,'14G','10G'),'28G','25G') as vtype FROM [Insite].[dbo].[ProductionResult] c with(nolock) 
                      left join wat.dbo.WXEvalPN ep with (nolock) on ep.WaferNum = left(c.Containername,9)
                      left join wat.dbo.WXEvalPNRate r on left(ep.EvalPN,7) = r.EvalPN
                      where len(c.Containername) = 20 and c.TestStep = 'PRLL_Post_HTOL1_Test' and c.TestTimeStamp > @starttime  and  r.RealRate is not null and (c.Containername like '%E08%' or c.Containername like '%R08%' or c.Containername like '%T08%')
                      group by left(c.containername,10),r.RealRate,ep.AppVal1  order by mintime asc";
            dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                var tempvm = new WATCapacity();
                tempvm.Wafer = UT.O2S(line[0]).ToUpper();
                tempvm.WFDate = UT.O2T(line[1]).AddHours(-23);
                tempvm.VType = UT.O2S(line[2]);

                if (wfdict.ContainsKey(tempvm.Wafer))
                { continue; }
                wfdict.Add(tempvm.Wafer, tempvm.Wafer);

                ret.Add(tempvm);
            }

            sql = @"select left(c.containername,14) as wafer,max(c.TestTimeStamp) as mintime,REPLACE(REPLACE('1x'+ep.AppVal1+ ' ' +r.RealRate,'14G','10G'),'28G','25G') as vtype FROM [Insite].[dbo].[ProductionResult] c with(nolock) 
                  left join wat.dbo.WXEvalPN ep with (nolock) on ep.WaferNum = left(c.Containername,13)
                  left join wat.dbo.WXEvalPNRate r on left(ep.EvalPN,7) = r.EvalPN
                  where len(c.Containername) = 24 and c.TestStep = 'PRLL_Post_HTOL1_Test' and c.TestTimeStamp > @starttime   and  r.RealRate is not null  and (c.Containername like '%E08%' or c.Containername like '%R08%' or c.Containername like '%T08%')
                  group by left(c.containername,14),r.RealRate,ep.AppVal1  order by mintime asc";
            dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                var tempvm = new WATCapacity();
                tempvm.Wafer = UT.O2S(line[0]).ToUpper();
                tempvm.WFDate = UT.O2T(line[1]);
                tempvm.VType = UT.O2S(line[2]);

                if (wfdict.ContainsKey(tempvm.Wafer))
                { continue; }
                wfdict.Add(tempvm.Wafer, tempvm.Wafer);

                ret.Add(tempvm);
            }

            ret.Sort(delegate (WATCapacity obj1, WATCapacity obj2)
            {
                return obj1.WFDate.CompareTo(obj2.WFDate);
            });

            return ret;
        }

        public static List<WATCapacity> GetWATWIP(List<string> waferlist)
        {
            var ret = new List<WATCapacity>();

            var sb = new StringBuilder();
            foreach (var wf in waferlist)
            {
                sb.Append("or Containername like '" + wf + "08%' ");
            }

            var wfcond = sb.ToString().Substring(3);

            var sql = @"select distinct left(containername,10) as wafer,TestStep FROM [Insite].[dbo].[ProductionResult]
                      where len(Containername) = 20 and ("+wfcond+")";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var tempvm = new WATCapacity();
                tempvm.Wafer = UT.O2S(line[0]).ToUpper();
                tempvm.Step = UT.O2S(line[1]).ToUpper();
                ret.Add(tempvm);
            }

            sql = @"select distinct left(containername,14) as wafer,TestStep FROM [Insite].[dbo].[ProductionResult]
                  where len(Containername) = 24 and (" + wfcond + ")";
            dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var tempvm = new WATCapacity();
                tempvm.Wafer = UT.O2S(line[0]).ToUpper();
                tempvm.Step = UT.O2S(line[1]).ToUpper();
                ret.Add(tempvm);
            }

            return ret;
        }

        public static List<DateTime> GetWeekList(DateTime sdate)
        {
            var datelist = new List<DateTime>();
            for (var idx = 0; idx < 10; idx++)
            {
                var tempdate = sdate.AddDays(idx);
                if (tempdate.DayOfWeek == DayOfWeek.Thursday)
                {
                    sdate = tempdate;
                    break;
                }
            }

            var dateidx = sdate;
            var currentday = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00");
            while (true)
            {
                datelist.Add(dateidx);
                dateidx = dateidx.AddDays(7);
                if (dateidx >= currentday)
                { break; }
            }

            return datelist;
        }

        public WATCapacity()
        {
            Wafer = "";
            WFDate = DateTime.Parse("1982-05-06 10:00:00");
            VType = "";
            Pass = "PENDING";
            WKStr = "";
            Step = "";
            OvenSlot = 10;
            PN = "";
        }
        public string Wafer { set; get; }
        public DateTime WFDate { set; get; }

        public string VType { set; get; }
        public string Pass { set; get; }

        public string WKStr { set; get; }
        public string Step { set; get; }
        public int OvenSlot { set; get; }
        public string PN { set; get; }
    }
}
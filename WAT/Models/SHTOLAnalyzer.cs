using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WAT.Models
{
    public class SHTOLAnalyzer
    {
        public static void AnalyzeData(Controller ctrl,string startdate_,string enddate_)
        {
            var startdate = startdate_;
            var enddate = enddate_;
            if (string.IsNullOrEmpty(startdate_))
            {
                startdate = GetLatestFinishedSNDate();
                enddate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd") + " 00:00:00";
            }

            var registdate = UT.O2T(enddate).AddDays(-1).ToString("yyyy-MM-dd") + " 00:00:00";

            var analyzedsnlist = GetFinishedSNs(startdate);
            var testissuesnlist = GetTestIssueSNs();

            var snlist = GetToBeAnalyzeSNs(startdate, enddate);
            foreach (var sn in snlist)
            {
                if (analyzedsnlist.ContainsKey(sn))
                { continue; }
                if (testissuesnlist.ContainsKey(sn))
                { continue; }

                AddFinishedSN(sn, registdate);

                var fieldlist = GetSNRxFields(sn);
                foreach (var field in fieldlist)
                {
                    var mxmn = GetMaxMinDataVal(sn, field);
                    var mx = mxmn[0];
                    var mn = mxmn[1];
                    var ag = mxmn[2];

                    if (mx == -40) { continue; }

                    var reason = "";
                    if (Math.Abs(mx - mn) > 0.5)
                    { reason = "DELTA,AVG "+Math.Round(ag,3).ToString(); }
                    else if (mx > 3 || mn > 3 || mx < -3 || mn < -3)
                    { reason = "ABS"; }

                    if (!string.IsNullOrEmpty(reason))
                    {
                        var info = GetSNDetailInfo(sn, startdate, enddate);
                        info.SN = sn;
                        info.DataField = field;
                        info.MXVal = mx;
                        info.MNVal = mn;
                        info.Reason = reason;
                        info.Store();
                    }//end reason
                }//end field
            }//end sn
        }

        private static List<string> GetToBeAnalyzeSNs(string startdate, string enddate)
        {
            var ret = new List<string>();
            var sql = @"select distinct SN FROM [Insite].[dbo].[SHTOLvm] where load_time > @startdate and load_time < @enddate  and (Stat = 'Done' or Stat = 'PAUSE')";
            var dict = new Dictionary<string, string>();
            dict.Add("@startdate", startdate);
            dict.Add("@enddate", enddate);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,dict);
            foreach (var line in dbret)
            {
                var SN = UT.O2S(line[0]).Trim().ToUpper();
                ret.Add(SN);
            }

            sql = @"SELECT distinct SN FROM  (SELECT SN, ROUND(CONVERT(float, Val,1),5) As v FROM   [Insite].[dbo].[SHTOLvm]  WHERE  (Load_Time > @startdate) AND (Load_Time < @enddate) AND (ValName LIKE '%RX%') AND (ValName LIKE '%POWER%') AND (ValName NOT LIKE '%DELTA%') AND (ISNUMERIC(Val) = 1)) AS subq
                        WHERE  v < -3 and v > -40";
            dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                var SN = UT.O2S(line[0]).Trim().ToUpper();
                if (!ret.Contains(SN))
                {
                    ret.Add(SN);
                }
            }

            return ret;
        }

        private static List<string> GetSNRxFields(string sn)
        {
            var ret = new List<string>();
            var sql = @"select distinct valname FROM [Insite].[dbo].[SHTOLvm] where sn = @sn  and valname like '%RX%' and ValName like '%POWER%' and (Stat = 'Done' or Stat = 'PAUSE')";
            var dict = new Dictionary<string, string>();
            dict.Add("@sn", sn);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                var fieldname = UT.O2S(line[0]).Trim().ToUpper();
                if (!fieldname.Contains("DELTA"))
                { ret.Add(fieldname); }
            }
            return ret;
        }

        public static List<double> GetMaxMinDataVal(string sn, string valname)
        {
            var ret = new List<double>();
            var sql = @"select max(va) mx,min(va) mn,avg(va) ag  from ( select CONVERT(float,val) as va,row_number() Over(order by Load_Time asc) as rw 
                 from [Insite].[dbo].[SHTOLvm] where sn = @sn and ValName = @valname and Val is not null and Stat <> 'PAUSE' ) as subq where rw > 10";
            var dict = new Dictionary<string, string>();
            dict.Add("@sn", sn);
            dict.Add("@valname", valname);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                ret.Add(UT.O2D(line[0]));
                ret.Add(UT.O2D(line[1]));
                ret.Add(UT.O2D(line[2]));
            }
            return ret;
        }

        public static SHTOLAnalyzer GetSNDetailInfo(string sn,string startdate, string enddate)
        {
            var ret = new SHTOLAnalyzer();
            var dict = new Dictionary<string, string>();
            dict.Add("@sn", sn);
            var sql = "select top 1 product FROM [Insite].[dbo].[SHTOLvm] where sn = @sn and product <> ''";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            { ret.Product = UT.O2S(line[0]); }

            sql = "select top 1 Create_Time FROM [Insite].[dbo].[SHTOLvm] where sn = @sn and Create_Time <> ''";
            dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            { ret.CreateTime = UT.T2S(line[0]); }

            sql = "select top 1 Load_Time,Tester FROM [Insite].[dbo].[SHTOLvm] where sn = @sn and Load_Time >=  @startdate and load_time < @enddate order by Load_Time desc";
            dict.Add("@startdate", startdate);
            dict.Add("@enddate", enddate);
            dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                ret.FinishTime = UT.T2S(line[0]);
                ret.Tester = UT.O2S(line[1]);
            }

            return ret;
        }

        private void Store()
        {
            var dict = new Dictionary<string, string>();
            dict.Add("@SN", SN);
            dict.Add("@Product", Product);
            dict.Add("@DataField", DataField);
            dict.Add("@MXVal", MXVal.ToString());
            dict.Add("@MNVal", MNVal.ToString());
            dict.Add("@Reason", Reason);
            dict.Add("@CreateTime", CreateTime);
            dict.Add("@FinishTime", FinishTime);
            dict.Add("@Tester", Tester);

            var sql = @"delete from [Insite].[dbo].[SHTOLAnalyzer] where SN=@SN and DataField=@DataField and AppVal1=''";
            DBUtility.ExeLocalSqlNoRes(sql, dict);

            sql = @"insert into [Insite].[dbo].[SHTOLAnalyzer](SN,Product,DataField,MXVal,MNVal,Reason,CreateTime,FinishTime,AppVal2) 
                            values(@SN,@Product,@DataField,@MXVal,@MNVal,@Reason,@CreateTime,@FinishTime,@Tester)";
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        private static void AddFinishedSN(string sn, string startdate)
        {
            var sql = @"insert into [Insite].[dbo].[SHTOLAnalyzed](SN,FinishTime)  values(@SN,@FinishTime)";
            var dict = new Dictionary<string, string>();
            dict.Add("@SN", sn);
            dict.Add("@FinishTime", startdate);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        private static Dictionary<string,bool> GetFinishedSNs(string startdate)
        {
            var ret = new Dictionary<string, bool>();
            var dict = new Dictionary<string, string>();
            dict.Add("@startdate", startdate);
            var sql = "select distinct SN from [Insite].[dbo].[SHTOLAnalyzed] where FinishTime >= @startdate";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                var sn = UT.O2S(line[0]);
                if (!ret.ContainsKey(sn))
                { ret.Add(sn, true); }
            }
            return ret;
        }

        private static string GetLatestFinishedSNDate()
        {
            var ret = "";
            var sql = "select top 1 [FinishTime] from [Insite].[dbo].[SHTOLAnalyzed] order by [FinishTime] desc";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var ftime = UT.T2S(line[0]);
                if (string.Compare(ftime, DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00") == 0)
                { ret = ftime; }
                else
                { ret = UT.T2S(UT.O2T(ftime).AddDays(1)); }
            }

            if (string.IsNullOrEmpty(ret))
            {
                ret = DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00";
            }
            return ret;
        }

        private static Dictionary<string, bool> GetTestIssueSNs()
        {
            var ret = new Dictionary<string, bool>();
            var sql = "select distinct SN  FROM [Insite].[dbo].[SHTOLAnalyzer] where AppVal1 = 'TESTISSUE'";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var sn = UT.O2S(line[0]);
                if (!ret.ContainsKey(sn))
                { ret.Add(sn, true); }
            }
            return ret;
        }

        public static List<SHTOLAnalyzer> LoadAllSHTOLStat(string all)
        {
            var ret = new List<SHTOLAnalyzer>();
            var sql = "SELECT top 2000 [SN],[Product],[DataField],[MXVal],[MNVal],[Reason],[FinishTime],[AppVal1],AppVal2 FROM [Insite].[dbo].[SHTOLAnalyzer] where AppVal1 <> 'TESTISSUE'  order by FinishTime desc,SN";
            if (!string.IsNullOrEmpty(all))
            {  sql = "SELECT [SN],[Product],[DataField],[MXVal],[MNVal],[Reason],[FinishTime],[AppVal1],AppVal2 FROM [Insite].[dbo].[SHTOLAnalyzer] order by FinishTime desc,SN"; }

            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var tempvm = new SHTOLAnalyzer();
                tempvm.SN = UT.O2S(line[0]);
                tempvm.Product = UT.O2S(line[1]);
                tempvm.DataField = UT.O2S(line[2]);
                tempvm.MXVal = UT.O2D(line[3]);
                tempvm.MNVal = UT.O2D(line[4]);
                tempvm.Reason = UT.O2S(line[5]);
                tempvm.FinishTime = UT.T2S(line[6]);
                tempvm.CFM = UT.O2S(line[7]);
                tempvm.Tester = UT.O2S(line[8]);
                ret.Add(tempvm);
            }
            return ret;
        }

        public static void UpdateSHTOLJudgement(string sn, string stat)
        {
            var dict = new Dictionary<string, string>();
            dict.Add("@SN", sn);
            dict.Add("@STAT", stat);
            var sql = "update [Insite].[dbo].[SHTOLAnalyzer] set AppVal1 = @STAT where SN = @SN";
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static Dictionary<string, List<double>> GetSNPWRData(string sn)
        {
            var ret = new Dictionary<string, List<double>>();
            var fieldlist = GetSNRxFields(sn);
            foreach (var field in fieldlist)
            {
                var dict = new Dictionary<string, string>();
                dict.Add("@sn", sn);
                dict.Add("@field", field);
                var sql = "SELECT [Val] FROM [Insite].[dbo].[SHTOLvm] where sn = @sn and ValName = @field and ISNUMERIC(val) =1  order by Load_Time asc";
                var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
                var datalist = new List<double>();
                var idx = 0;
                foreach (var line in dbret)
                {
                    if (idx < 10) { idx++; continue; }
                    datalist.Add(UT.O2D(line[0]));
                }
                ret.Add(field, datalist);
            }
            return ret;
        }

        public static List<double> GetSNTemp(string sn)
        {
            var ret = new List<double>();
            var dict = new Dictionary<string, string>();
            dict.Add("@sn", sn);
            dict.Add("@field", "TEMPERATURE");
            var sql = "SELECT [Val] FROM [Insite].[dbo].[SHTOLvm] where sn = @sn and ValName = @field and ISNUMERIC(val) =1  order by Load_Time asc";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            var datalist = new List<double>();
            var idx = 0;
            foreach (var line in dbret)
            {
                if (idx < 10) { idx++; continue; }
                ret.Add(UT.O2D(line[0]));
            }
            return ret;
        }


        public SHTOLAnalyzer()
        {
            SN = "";
            Product = "";
            DataField = "";
            MXVal = -9999.0;
            MNVal = -9999.0;
            Reason = "";
            CreateTime = "1982-05-06 10:30:00";
            FinishTime = "1982-05-06 10:30:00";
            CFM = "";
            Tester = "";
        }


        public string SN { set; get; }
        public string Product { set; get; }
        public string DataField { set; get; }
        public double MXVal { set; get; }
        public double MNVal { set; get; }
        public string Reason { set; get; }
        public string CreateTime { set; get; }
        public string FinishTime { set; get; }
        public string CFM { set; get; }
        public string Tester { set; get; }
    }
}
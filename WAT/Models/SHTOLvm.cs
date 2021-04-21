using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WAT.Models
{
    public class SHTOLvm
    {
        public static void RefreshDailySHTOLData(Controller ctrl)
        {
            var syscfg = CfgUtility.GetSysConfig(ctrl);
            var shtoltesters = syscfg["SHTOLMAC"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (var tester in shtoltesters)
            {
                var startlatetime = GetSHTOLLatestTime(tester);
                if (string.IsNullOrEmpty(startlatetime))
                { startlatetime = DateTime.Now.AddDays(-14).ToString("yyyy-MM-dd HH:mm:ss"); }
                LoadSHTOLData(startlatetime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), tester);
            }
        }

        private static string GetSHTOLLatestTime(string tester)
        {
            var sql = "select top 1 Load_Time from Insite.dbo.SHTOLvm where Tester = @tester order by Load_Time desc";             

            var dict = new Dictionary<string, string>();
            dict.Add("@tester", tester);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                var latetime = UT.O2T(line[0]).AddSeconds(1).ToString("yyyy-MM-dd HH:mm:ss");
                if (latetime.Contains("1982-"))
                { return string.Empty; }
                else
                { return latetime; }
            }//end foreach
            return string.Empty;
        }

        private static void LoadSHTOLData(string starttime, string endtime, string tester)
        {
            var retlist = new List<SHTOLvm>();
            var sql = @"select bu.SN,bu.[Type] as Product,bu.[State],bu.Result,bd.[Name],bd.[Value],bu.Create_Time,bd.Load_Time from [DB_BMS].[dbo].[BI_Unit] bu (nolock)
                        left join [DB_BMS].[dbo].[BI_Data] bd on bu.Data_Set_ID = bd.Data_Set_ID
                        where bd.Load_Time > '" + starttime + "' and bd.Load_Time < '" + endtime + "' ";
            var dbret = DBUtility.ExeOvenSqlWithRes(tester, sql);
            foreach (var line in dbret)
            {
                var tempvm = new SHTOLvm();
                tempvm.SN = UT.O2S(line[0]);
                tempvm.Product = UT.O2S(line[1]);
                tempvm.Stat = UT.O2S(line[2]);
                tempvm.Result = UT.O2S(line[3]);
                tempvm.ValName = UT.O2S(line[4]);
                tempvm.Val = UT.O2S(line[5]);
                tempvm.Tester = tester;
                tempvm.Create_Time = UT.O2T(line[6]).ToString("yyyy-MM-dd HH:mm:ss");
                tempvm.Load_Time = UT.O2T(line[7]).ToString("yyyy-MM-dd HH:mm:ss");
                retlist.Add(tempvm);
            }

            foreach (var d in retlist)
            { d.StoreData(); }
        }

        private void StoreData()
        {
            var sql = @"insert into Insite.dbo.SHTOLvm(SN,Product,Stat,Result,ValName,Val,Tester,Create_Time,Load_Time)  
                        values(@SN,@Product,@Stat,@Result,@ValName,@Val,@Tester,@Create_Time,@Load_Time)";
            var dict = new Dictionary<string, string>();
            dict.Add("@SN", SN);
            dict.Add("@Product", Product);
            dict.Add("@Stat", Stat);
            dict.Add("@Result", Result);
            dict.Add("@ValName", ValName);
            dict.Add("@Val", Val);
            dict.Add("@Tester", Tester);
            dict.Add("@Create_Time", Create_Time);
            dict.Add("@Load_Time", Load_Time);

            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static Dictionary<string,Dictionary<string, int>> GetWeeklyDoneDistribution(string startdate, string enddate)
        {
            var ret = new Dictionary<string, Dictionary<string, int>>();

            var ldate = WKSpan.RetrieveDateSpanByWeek(startdate, enddate);
            var startidx = 0;
            for (int idx = startidx; idx < ldate.Count - 1; idx++)
            {
                var dict = GetDoneDistribution(ldate[idx].ToString(), ldate[idx + 1].ToString());
                if (dict.Count > 0)
                {
                    ret.Add(ldate[idx].ToString(), dict);
                }
            }
            return ret;
        }

        private static Dictionary<string, int> GetDoneDistribution(string starttime,string endtime)
        {
            var ret = new Dictionary<string, int>();
            var sql = "select product,Count(sn) from ( select distinct product,sn from [Insite].[dbo].[SHTOLvm] "
                       +" where load_time > '"+ starttime + "' and load_time < '"+ endtime + "' and (Stat = 'Done' or Stat = 'PAUSE')) as a group by product ";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                ret.Add(UT.O2S(line[0]), UT.O2I(line[1]));
            }
            return ret;
        }

        public static Dictionary<string, int> GetSHTOLWIP()
        {
            var ret = new Dictionary<string, int>();
            var sdate = DateTime.Now.AddDays(-9).ToString("yyyy-MM-dd");
            var sql = @"select product, Count(sn)from(select distinct product, sn from[Insite].[dbo].[SHTOLvm] 
                        where load_time > '"+ sdate + "' and sn not in (select distinct sn from[Insite].[dbo].[SHTOLvm] where load_time > '"+sdate+"' and(Stat = 'Done' or Stat = 'PAUSE')) ) as a group by product";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                ret.Add(UT.O2S(line[0]), UT.O2I(line[1]));
            }
            return ret;
        }

        public SHTOLvm()
        {
            SN = "";
            Product = "";
            Stat = "";
            Result = "";
            ValName = "";
            Val = "";
            Tester = "";
            Create_Time = "";
            Load_Time = "";
        }

        public string SN { set; get; }
        public string Product { set; get; }
        public string Stat { set; get; }
        public string Result { set; get; }
        public string ValName { set; get; }
        public string Val { set; get; }
        public string Tester { set; get; }
        public string Create_Time { set; get; }
        public string Load_Time { set; get; }

    }

}
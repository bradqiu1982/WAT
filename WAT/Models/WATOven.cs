using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WAT.Models
{
    public class WATOven
    {
        public static List<object> GetOvenData(string teststation, string startdate, string enddate, int freq, string coupongroupid = null)
        {
            var ret = new List<object>();

            var tempdict = new Dictionary<string, List<double>>();
            var currentdict = new Dictionary<string, List<double>>();

            var sql = @"select d.OVENTEMPERATURE,d.ImA,d.CreateTime from WAT.dbo.OvenData (nolock) d 
                        left join WAT.dbo.OvenStart (nolock) s on d.DataSet_ID = s.DataSet_ID
                        where (s.[Plan] like '%12mA_172H%' or s.[Plan] like '%12mA_120H%') and s.SN like '%E08%' and d.PCName = @teststation
                        and d.CreateTime >= @startdate and d.CreateTime <= @enddate and d.rid%" + freq + @"=0 order by d.CreateTime asc";

            if (!string.IsNullOrEmpty(coupongroupid))
            {
                if (coupongroupid.Length < 9)
                { return ret; }

                sql = @"select d.OVENTEMPERATURE,d.ImA,d.CreateTime from WAT.dbo.OvenData (nolock) d 
                        left join WAT.dbo.OvenStart (nolock) s on d.DataSet_ID = s.DataSet_ID
                        where s.SN like '%" + coupongroupid + "%' and len(s.SN) > 9 order by d.CreateTime asc";
            }

            var dict = new Dictionary<string, string>();
            dict.Add("@startdate", startdate);
            dict.Add("@enddate", enddate);
            dict.Add("@teststation", teststation);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);

            foreach (var line in dbret)
            {
                var date = UT.O2T(line[2]).ToString("yyyy-MM-dd");

                if (line[0] != System.DBNull.Value)
                {
                    var val = UT.O2S(line[0]);
                    if (val.Length > 0 && !val.Contains("NAN"))
                    {
                        var v = UT.O2D(val);
                        if (v > 0 && v <= 150)
                        {
                            if (!tempdict.ContainsKey(date))
                            {
                                var templist = new List<double>();
                                templist.Add(v);
                                tempdict.Add(date, templist);
                            }
                            else
                            { tempdict[date].Add(v); }
                        }
                    }
                }

                if (line[1] != System.DBNull.Value)
                {
                    var val = UT.O2S(line[1]);
                    if (val.Length > 0 && !val.Contains("NAN"))
                    {
                        var v = UT.O2D(val);
                        if (v > 0 && v <= 30)
                        {
                            if (!currentdict.ContainsKey(date))
                            {
                                var templist = new List<double>();
                                templist.Add(v);
                                currentdict.Add(date, templist);
                            }
                            else
                            { currentdict[date].Add(v); }
                        }
                    }
                }
            }

            if (tempdict.Count > 0)
            {
                ret.Add(tempdict);
                ret.Add(currentdict);
            }

            return ret;
        }

        public static List<List<string>> GetOvenDataByWafer(string coupongroupid)
        {
            var ret = new List<List<string>>();
            if (coupongroupid.Length < 9)
            { return ret; }

            var sql = @"select d.rid,s.SN,s.[Plan],s.Board,s.Seat,d.[LEVEL],d.SLOT,d.TARGETC,d.WATER_SETC
                ,d.TARGET_IC,d.OVENTEMPERATURE,d.ImA,d.CreateTime from WAT.dbo.OvenData (nolock) d 
                left join WAT.dbo.OvenStart (nolock) s on d.DataSet_ID = s.DataSet_ID
                where  Len(s.SN) > 9 and s.SN like '%"+ coupongroupid + "%' order by CreateTime asc";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var tmpline = new List<string>();
                for (var idx = 0; idx <= 11; idx++)
                { tmpline.Add(UT.O2S(line[idx])); }
                tmpline.Add(UT.O2T(line[12]).ToString("yyyy-MM-dd HH:mm:ss"));
                ret.Add(tmpline);
            }
            return ret;
        }

        //public static List<object> GetOvenData1(string teststation, string startdate, string enddate,int freq,string coupongroupid = null)
        //{
        //    var ret = new List<object>();

        //    var tempdict = new Dictionary<string, List<double>>();
        //    var currentdict = new Dictionary<string, List<double>>();

        //    var sql = @"SELECT value,Load_Time FROM [BMS41].[dbo].[BI_Data] where  Data_Set_ID in 
        //        ( select Data_Set_ID FROM [BMS41].[dbo].[BI_Data]  where Tag = 'START'  and Load_Time > @startdate and Load_Time < @enddate and Value like '%E08%' and Value like '%13mA%') 
        //        and Tag = 'DATA'  and Load_Time > @startdate and Load_Time < @enddate and ID%" + freq+@" = 0 order by Load_Time asc";

        //    if (!string.IsNullOrEmpty(coupongroupid))
        //    {
        //        sql = @"SELECT value,Load_Time FROM [BMS41].[dbo].[BI_Data] where  Data_Set_ID in 
        //        ( select Data_Set_ID FROM [BMS41].[dbo].[BI_Data]  where Tag = 'START' and Value like '%"+coupongroupid+@"%') 
        //        and Tag = 'DATA' order by Load_Time asc";
        //    }

        //    var dict = new Dictionary<string, string>();
        //    dict.Add("@startdate", startdate);
        //    dict.Add("@enddate", enddate);

        //    var dbret = DBUtility.ExeOvenSqlWithRes(teststation,sql, dict);

        //    foreach (var line in dbret)
        //    {
        //        if (line[0] != System.DBNull.Value)
        //        {
        //            var val = UT.O2S(line[0]);
        //            var date = UT.O2T(line[1]).ToString("yyyy-MM-dd");

        //            if (val.ToUpper().Contains("OVENTEMPERATURE"))
        //            {
        //                var valstr = val.ToUpper().Split(new string[] { "OVENTEMPERATURE" }, StringSplitOptions.None)[1]
        //                    .Split(new string[] { ":", "," }, StringSplitOptions.None)[1].Replace("\"", "").Replace("}", "").Replace(" ", "");
                        
        //                if (valstr.Length > 0 && !valstr.Contains("NAN"))
        //                {
        //                    var v = UT.O2D(valstr);
        //                    if (v > 0 && v <= 150)
        //                    {
        //                        if (!tempdict.ContainsKey(date))
        //                        {
        //                            var templist = new List<double>();
        //                            templist.Add(v);
        //                            tempdict.Add(date, templist);
        //                        }
        //                        else
        //                        { tempdict[date].Add(v); }
        //                    }
        //                }

        //            }

        //            if (val.ToUpper().Contains("I[MA]"))
        //            {
        //                var valstr = val.ToUpper().Split(new string[] { "I[MA]" }, StringSplitOptions.None)[1]
        //                    .Split(new string[] { ":", "," }, StringSplitOptions.None)[1].Replace("\"", "").Replace("}", "").Replace(" ", "");

        //                if (valstr.Length > 0 && !valstr.Contains("NAN"))
        //                {
        //                    var v = UT.O2D(valstr);
        //                    if (v > 0 && v <= 30)
        //                    {
        //                        if (!currentdict.ContainsKey(date))
        //                        {
        //                            var templist = new List<double>();
        //                            templist.Add(v);
        //                            currentdict.Add(date, templist);
        //                        }
        //                        else
        //                        { currentdict[date].Add(v); }
        //                    }
        //                }
        //            }
        //        }

        //    }

        //    if (tempdict.Count > 0)
        //    {
        //        ret.Add(tempdict);
        //        ret.Add(currentdict);
        //    }

        //    return ret;
        //}

        private static void LoadOvenStartData(string startdate, string enddate, string tester)
        {
            var ovenstartlist = new List<OvenStart>();
            var sql = @"select Data_Set_ID,Value,[Station],Load_Time from [BMS41].[dbo].[BI_Data] where 
                          Load_Time > @startdate and Load_Time <= @enddate and Tag = 'START'";
            var dict = new Dictionary<string, string>();
            dict.Add("@startdate", startdate);
            dict.Add("@enddate", enddate);

            var dbret = DBUtility.ExeOvenSqlWithRes(tester, sql, dict);
            foreach (var line in dbret)
            {
                try
                {
                    var osb = (OvenStartBase)Newtonsoft.Json.JsonConvert.DeserializeObject(UT.O2S(line[1]), (new OvenStartBase()).GetType());
                    var ovenstart = new OvenStart(osb);
                    ovenstart.DataSet_ID = UT.O2S(line[0]) + "_" + tester;
                    if (string.IsNullOrEmpty(ovenstart.Station))
                    { ovenstart.Station = UT.O2S(line[2]); }
                    try
                    {
                        DateTime.Parse(ovenstart.CreateTime);
                    }
                    catch (Exception e)
                    {
                        ovenstart.CreateTime = UT.O2T(line[3]).ToString("yyyy-MM-dd HH:mm:ss");
                    }

                    ovenstartlist.Add(ovenstart);
                }
                catch (Exception ex) { }
            }

            foreach (var data in ovenstartlist)
            {
                data.StoreData();
            }
        }

        private static void LoadOvenDataData(string startdate, string enddate, string tester)
        {
            var ovendatalist = new List<OvenData>();
            var sql = @"select Data_Set_ID,Value,[Station],Load_Time from [BMS41].[dbo].[BI_Data] where 
                          Load_Time > @startdate and Load_Time <= @enddate and Tag = 'DATA'";
            var dict = new Dictionary<string, string>();
            dict.Add("@startdate", startdate);
            dict.Add("@enddate", enddate);
            var dbret = DBUtility.ExeOvenSqlWithRes(tester, sql, dict);
            foreach (var line in dbret)
            {
                try
                {
                    var odb = (OvenDataBase)Newtonsoft.Json.JsonConvert.DeserializeObject(UT.O2S(line[1]).Replace("[","").Replace("]",""), (new OvenDataBase()).GetType());
                    var ovendata = new OvenData(odb);
                    ovendata.DataSet_ID = UT.O2S(line[0]) + "_" + tester;
                    if (string.IsNullOrEmpty(ovendata.PCName))
                    { ovendata.PCName = UT.O2S(line[2]); }
                    ovendata.CreateTime = UT.O2T(line[3]).ToString("yyyy-MM-dd HH:mm:ss");
                    ovendatalist.Add(ovendata);
                }
                catch (Exception ex) { }
            }

            foreach (var data in ovendatalist)
            {
                data.StoreData();
            }
        }

        private static void LoadOvenData(string startdate, string enddate, string tester)
        {
            LoadOvenStartData( startdate,  enddate,  tester);
            LoadOvenDataData( startdate,  enddate,  tester);
        }

        private static string GetOvenLatestTime(string tester, bool ovenstart)
        {
            var sql = "";
            if (ovenstart)
            { sql = "select top 1 CreateTime from WAT.dbo.OvenStart where Station = @tester order by CreateTime desc"; }
            else
            { sql = "select top 1 CreateTime from WAT.dbo.OvenData where PCName = @tester order by CreateTime desc"; }

            var dict = new Dictionary<string, string>();
            dict.Add("@tester", tester);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                var latetime = UT.O2T(line[0]).ToString("yyyy-MM-dd HH:mm:ss");
                if (latetime.Contains("1982-"))
                { return string.Empty; }
                else
                { return latetime; }
            }//end foreach
            return string.Empty;
        }

        public static void RefreshDailyOvenData(Controller ctrl)
        {
            var syscfg = CfgUtility.GetSysConfig(ctrl);
            var oventesters = syscfg["WATOVEN"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (var tester in oventesters)
            {
                var startlatetime = GetOvenLatestTime(tester,true);
                if (string.IsNullOrEmpty(startlatetime))
                { startlatetime = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss"); }
                LoadOvenStartData(startlatetime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), tester);

                var datalatetime = GetOvenLatestTime(tester, false);
                if (string.IsNullOrEmpty(datalatetime))
                { datalatetime = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss"); }
                LoadOvenDataData(datalatetime, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), tester);
            }
        }

    }

    public class OvenStart : OvenStartBase
    {
        public void StoreData()
        {
            var sql = @"insert into WAT.dbo.OvenStart(DataSet_ID,SN,[Plan],[Type],Station,Board,Seat,CreateTime) 
                        values(@DataSet_ID,@SN,@Plan,@Type,@Station,@Board,@Seat,@CreateTime)";
            var dict = new Dictionary<string, string>();
            dict.Add("@DataSet_ID", DataSet_ID);
            dict.Add("@SN", SN);
            dict.Add("@Plan", Plan);
            dict.Add("@Type", Type);
            dict.Add("@Station", Station);
            dict.Add("@Board", Board);
            dict.Add("@Seat", Seat);
            dict.Add("@CreateTime", CreateTime);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public OvenStart(OvenStartBase b)
        {
            DataSet_ID = "";
            SN = b.SN;
            ProductDetail = b.ProductDetail;
            Plan = b.Plan;
            Type = b.Type;
            Span = b.Span;
            ConditionT = b.ConditionT;
            Station = b.Station;
            Board = b.Board;
            Seat = b.Seat;
            CreateTime = b.CreateTime;
        }

        public string DataSet_ID { set; get; }
    }


    public class OvenData : OvenDataBase
    {
        public void StoreData()
        {
            var sql = @"insert into WAT.dbo.OvenData(DataSet_ID,OVENTEMPERATURE,PCName,[LEVEL],[SLOT],TARGETC,WATER_SETC,ImA,TARGET_IC,CreateTime) 
                        values(@DataSet_ID,@OVENTEMPERATURE,@PCName,@LEVEL,@SLOT,@TARGETC,@WATER_SETC,@ImA,@TARGET_IC,@CreateTime)";
            var dict = new Dictionary<string, string>();
            dict.Add("@DataSet_ID", DataSet_ID);
            dict.Add("@OVENTEMPERATURE", OVENTEMPERATURE);
            dict.Add("@PCName", PCName);
            dict.Add("@LEVEL", LEVEL);
            dict.Add("@SLOT", SLOT);
            dict.Add("@TARGETC", TARGETC);
            dict.Add("@WATER_SETC", WATER_SETC);
            dict.Add("@ImA", ImA);
            dict.Add("@TARGET_IC", TARGET_IC);
            dict.Add("@CreateTime", CreateTime);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public OvenData(OvenDataBase b)
        {
            DataSet_ID = "";
            CreateTime = "";
            OVENTEMPERATURE = b.OVENTEMPERATURE;
            PCName = b.PCName;
            LEVEL = b.LEVEL;
            SLOT = b.SLOT;
            TARGETC = b.TARGETC;
            WATER_SETC = b.WATER_SETC;
            ImA = b.ImA;
            TARGET_IC = b.TARGET_IC;
        }

        public string DataSet_ID { set; get; }
        public string CreateTime { set; get; }
    }


    public class OvenStartBase {
        
        public OvenStartBase() {
            SN ="";
            ProductDetail="";
            Plan="";
            Type="";
            Span="";
            ConditionT="";
            Station="";
            Board="";
            Seat="";
            CreateTime="";
        }

        public string SN { set; get; }
        public string ProductDetail { set; get; }
        public string Plan { set; get; }
        public string Type { set; get; }
        public string Span { set; get; }
        public string ConditionT { set; get; }
        public string Station { set; get; }
        public string Board { set; get; }
        public string Seat { set; get; }
        public string CreateTime { set; get; }
    }

    public class OvenDataBase {
        public OvenDataBase() {
            OVENTEMPERATURE = "";
            PCName = "";
            LEVEL = "";
            SLOT = "";
            TARGETC = "";
            WATER_SETC = "";
            ImA = "";
            TARGET_IC = "";
        }

        public string OVENTEMPERATURE { set; get; }
        public string PCName { set; get; }
        public string LEVEL { set; get; }
        public string SLOT { set; get; }
        public string TARGETC { set; get; }
        public string WATER_SETC { set; get; }
        public string ImA { set; get; }
        public string TARGET_IC { set; get; }
    }

}
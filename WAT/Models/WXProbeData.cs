﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class WXProbeData
    {

        //NEED TO BE UPDATE
        private static List<WXProbeData> GetAllenData(string WaferNum)
        {
            var ret = new List<WXProbeData>();
            
            var sql = @"select [Xcoord],[Ycoord],[Ith],[SeriesR],[SlopEff] from [EngrData].[dbo].[Wuxi_WAT_VR_Report] 
                        where [WaferID] = @WaferID and [Ith] is not null and [SeriesR] is not null and [SlopEff] is not null";
            var dict = new Dictionary<string, string>();
            dict.Add("@WaferID", WaferNum);
            var dbret = DBUtility.ExeAllenSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                var tempvm = new WXProbeData();
                tempvm.X = UT.O2S(line[0]);
                tempvm.Y = UT.O2S(line[1]);
                tempvm.Ith = UT.O2S(line[2]);
                tempvm.SeriesR = UT.O2S(line[3]);
                tempvm.SlopEff = UT.O2S(line[4]);
                ret.Add(tempvm);
            }
            return ret;
        }

        public static bool AllenHasData(string WaferNum)
        {
            var sql = @"select top 1 [Xcoord],[Ycoord],[Ith],[SeriesR],[SlopEff] from [EngrData].[dbo].[Wuxi_WAT_VR_Report] 
                        where [WaferID] = @WaferID and [Ith] is not null and [SeriesR] is not null and [SlopEff] is not null";
            var dict = new Dictionary<string, string>();
            dict.Add("@WaferID", WaferNum);
            var dbret = DBUtility.ExeAllenSqlWithRes(sql, dict);
            if (dbret.Count > 0)
            { return true; }
            else
            { return false; }
        }

        public static bool AddProbeTrigge2Allen(string WaferNum)
        {
            var sql = @"insert into [EngrData].[dbo].[NeoMAP_Triggers] ([Wafer_ID],[Trigger_Type],[Source]) 
                    values(@WaferNum,'wuxiwat','Neo-Expert:david.mathes')";
            var dict = new Dictionary<string, string>();
            dict.Add("@WaferNum", WaferNum);
            return DBUtility.ExeAllenSqlNoRes(sql, dict);
        }

        private static List<WXProbeData> GetShermanData(string WaferNum)
        {
            var ret = new List<WXProbeData>();
            var dict = new Dictionary<string, string>();
            dict.Add("@WAFER_NUMBER", WaferNum);
            dict.Add("@DATA_SET_TYPE_NAME", "Final Probe");
            dict.Add("@Include_Units_IN_Col_Headers", "1");
            dict.Add("@Passing_Dies_Only", "0");
            var dbret = DBUtility.ExeShermanStoreProcedureWithRes("Get_Latest_PROBE_DATA_For_Wafer", dict);
            foreach (var line in dbret)
            {
                var tempvm = new WXProbeData();
                tempvm.X = UT.O2S(line[12]);
                tempvm.Y = UT.O2S(line[13]);
                tempvm.Ith = UT.O2S(line[61]);
                tempvm.SeriesR = UT.O2S(line[122]);
                tempvm.SlopEff = UT.O2S(line[245]);
                ret.Add(tempvm);
            }
            return ret;
        }

        public static bool PrepareProbeData(string WaferNum)
        {
            var srclist = GetAllenData(WaferNum);
            if (srclist.Count == 0)
            {
                srclist = GetShermanData(WaferNum);
            }

            if (srclist.Count > 0)
            {
                var sql = "delete from [EngrData].[dbo].[VR_Eval_Pts_Data_Basic] where [WaferID] = @WaferID";
                var dict = new Dictionary<string, string>();
                dict.Add("@WaferID", WaferNum);
                DBUtility.ExeLocalSqlNoRes(sql, dict);

                sql = @"insert into [EngrData].[dbo].[VR_Eval_Pts_Data_Basic](EntryTime,WaferID,Xcoord,Ycoord,Ith,Wafer,SeriesR,SlopEff) 
                        values(@EntryTime,@WaferID,@Xcoord,@Ycoord,@Ith,@Wafer,@SeriesR,@SlopEff)";
                var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                foreach (var item in srclist)
                {
                    dict = new Dictionary<string, string>();
                    dict.Add("@EntryTime", now);
                    dict.Add("@WaferID", WaferNum);
                    dict.Add("@Xcoord", item.X);
                    dict.Add("@Ycoord", item.Y);
                    dict.Add("@Ith", item.Ith);
                    dict.Add("@Wafer", WaferNum);
                    dict.Add("@SeriesR", item.SeriesR);
                    dict.Add("@SlopEff", item.SlopEff);

                    DBUtility.ExeLocalSqlNoRes(sql, dict);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public static List<WXProbeData> GetData(string WaferNum)
        {
            var ret = new List<WXProbeData>();

            var sql = @"SELECT [WaferID]
					  ,[Xcoord]
					  ,[Ycoord]
					  ,[Ith]
					  ,[Wafer]
					  ,[SeriesR]
					  ,[SlopEff]
				   FROM [EngrData].[dbo].[VR_Eval_Pts_Data_Basic] pvr with(nolock)
				   WHERE [WaferID] = @WaferNum
				   AND pvr.RID = (SELECT max(RID) FROM [EngrData].[dbo].[VR_Eval_Pts_Data_Basic] pvrmax
									WHERE pvrmax.[WaferID] = @WaferNum
									AND pvrmax.[Xcoord]=pvr.[Xcoord]
									AND pvrmax.[Ycoord]=pvr.[Ycoord]
									AND pvrmax.[WaferID]=pvr.[WaferID]
									)";

            var dict = new Dictionary<string, string>();
            dict.Add("@WaferNum", WaferNum);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                var tempvm = new WXProbeData();
                tempvm.WaferID = UT.O2S(line[0]);
                tempvm.X = UT.O2S(line[1]);
                tempvm.Y = UT.O2S(line[2]);
                tempvm.Ith = UT.O2S(line[3]);
                tempvm.WaferNum = UT.O2S(line[4]);
                tempvm.SeriesR = UT.O2S(line[5]);
                tempvm.SlopEff = UT.O2S(line[6]);

                ret.Add(tempvm);
            }

            return ret;
        }

        public WXProbeData()
        {
            X = "";
            Y = "";
            UpdateTime = "1982-05-06 10:00:00";
            Ith = "";
            SeriesR = "";
            SlopEff = "";
            WaferNum = "";
            WaferID = "";
        }

        public string X { set; get; }
        public string Y { set; get; }
        public string UpdateTime { set; get; }
        public string Ith { set; get; }
        public string SeriesR { set; get; }
        public string SlopEff { set; get; }
        public string WaferNum { set; get; }
        public string WaferID { set; get; }
    }
}
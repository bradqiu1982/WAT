﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WXLogic
{
    public class WXProbeData
    {

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
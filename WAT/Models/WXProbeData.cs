using System;
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
            
            var sql = @"select distinct [Xcoord],[Ycoord],[Ith],[SeriesR],[SlopEff] from [EngrData].[dbo].[Wuxi_WAT_VR_Report] 
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
            dict.Add("@Sample_Quantity", "-1");
            dict.Add("@Comma_Delimited_TEST_NAMES_To_Return", "RT.Ith,RT.SeriesR,RT.SlopEff");
            var dbret = DBUtility.ExeShermanStoreProcedureWithRes("Get_Latest_PROBE_DATA_For_Wafer", dict);

            var xidx = 14;
            var yidx = 15;
            var iidx = 22;
            var sridx = 21;
            var slidx = 23;

            var ridx = 0;
            foreach (var line in dbret)
            {
                if (ridx == 0)
                {
                    ridx++;
                    var colname1 = UT.O2S(line[0]).ToUpper();
                    if (colname1.Contains("DATA_SET_ID"))
                    {
                        var colidx = 0;
                        foreach (var item in line)
                        {
                            var cname = UT.O2S(item).ToUpper();
                            if (string.Compare(cname, "X") == 0)
                            { xidx = colidx;}
                            if (string.Compare(cname, "Y") == 0)
                            { yidx = colidx;}
                            if (cname.Contains("RT.ITH"))
                            {
                                iidx = colidx;
                            }
                            if (cname.Contains("RT.SER"))
                            { sridx = colidx; }
                            if (cname.Contains("RT.SLOP"))
                            { slidx = colidx; }

                            colidx++;
                        }
                        continue;
                    }
                }

                if (line[xidx] == DBNull.Value || line[yidx] == DBNull.Value
                    || line[iidx] == DBNull.Value || line[sridx] == DBNull.Value
                    || line[slidx] == DBNull.Value)
                { continue; }

                var tempvm = new WXProbeData();
                tempvm.X = UT.O2S(line[xidx]);
                tempvm.Y = UT.O2S(line[yidx]);
                tempvm.Ith = UT.O2S(line[iidx]);
                tempvm.SeriesR = UT.O2S(line[sridx]);
                tempvm.SlopEff = UT.O2S(line[slidx]);

                //tempvm.Ith = UT.O2S(line[90]);
                //tempvm.SeriesR = UT.O2S(line[132]);
                //tempvm.SlopEff = UT.O2S(line[133]);

                ret.Add(tempvm);
            }
            return ret;
        }

        public static bool PrepareProbeData(string WaferNum)
        {
            var srclist = new List<WXProbeData>();
            if (WaferNum.Length == 13)
            {
                srclist = GetShermanData(WaferNum);
            }
            else
            {
                srclist = GetAllenData(WaferNum);
                if (srclist.Count == 0)
                {
                    srclist = GetShermanData(WaferNum);
                }
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
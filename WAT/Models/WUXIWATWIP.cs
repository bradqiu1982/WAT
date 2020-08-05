using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WAT.Models
{
    public class WUXIWATWIP
    {

        //public static List<WUXIWATWIP> GetWATWIP(Controller ctrl)
        //{
        //    var ret = new List<WUXIWATWIP>();
        //    var wdict = new Dictionary<string, WUXIWATWIP>();
        //    var sql = @"SELECT distinct [WAFER] ,[STEP] ,[TestTimeStamp] FROM [WAT].[dbo].[WUXIWATTESTSTATUS] 
        //            where [WAFER] <> '' and wafer not like '%R-%' order by TestTimeStamp desc";

        //    var dbret = DBUtility.ExeLocalSqlWithRes(sql);
        //    foreach (var line in dbret)
        //    {
        //        var wafer = UT.O2S(line[0]).ToUpper();
        //        if (!wdict.ContainsKey(wafer))
        //        {
        //            var tempvm = new WUXIWATWIP();
        //            tempvm.TESTTIMESTAMP = UT.O2T(line[2]).ToString("yyyy-MM-dd HH:mm:ss");
        //            wdict.Add(wafer, tempvm);

        //            tempvm = new WUXIWATWIP();
        //            tempvm.WAFER = wafer;
        //            tempvm.STEP = UT.O2S(line[1]);
        //            tempvm.TESTTIMESTAMP = UT.O2T(line[2]).ToString("yyyy-MM-dd HH:mm:ss");
        //            ret.Add(tempvm);
        //        }
        //        else
        //        {
        //            wdict[wafer].During = Math.Round((UT.O2T(wdict[wafer].TESTTIMESTAMP) - UT.O2T(line[2])).TotalHours,0);
        //        }
        //    }

        //    if (wdict.Count > 0)
        //    {
        //        var koyodict = WuxiWATData4MG.GetKOYODict(wdict.Keys.ToList(), ctrl);
        //        foreach (var item in ret)
        //        {
        //            item.VArray = WXLogic.WATSampleXY.GetArrayFromAllenSherman(item.WAFER.Replace("E","").Replace("R", "").Replace("T", ""));
        //            if (koyodict.ContainsKey(item.WAFER.Replace("E", "").Replace("R", "").Replace("T", "")))
        //            { item.VType = "KOYO"; }

        //            if (wdict.ContainsKey(item.WAFER))
        //            { item.During = wdict[item.WAFER].During; }
        //        }

        //        var OGPSNList = WATOGPVM.GetMEOGPXYWafer(ctrl);
        //        var LocalOGP = WATOGPVM.GetLocalOGPXYWafer();

        //        foreach (var item in ret)
        //        {
        //            if (OGPSNList.Contains(item.WAFER))
        //            { item.HasOGP = "YES"; }

        //            if (LocalOGP.Contains(item.WAFER))
        //            { item.LocalXY = "YES"; }
        //        }
        //    }

        //    return ret;
        //}

        public WUXIWATWIP()
        {
            WAFER = "";
            STEP = "";
            TESTTIMESTAMP = "";
            VType = "";
            VArray = "";
            HasOGP = "NO";
            During = 0;
            LocalXY = "NO";
        }

        public string WAFER { set; get; }
        public string STEP { set; get; }
        public string TESTTIMESTAMP { set; get; }
        public string VType { set; get; }
        public string VArray { set; get; }
        public string HasOGP { set; get; }
        public double During { set; get; }
        public string LocalXY { set; get; }
    }
}
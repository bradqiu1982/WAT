using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WAT.Models
{
    public class WUXIWATWIP
    {

        public static List<WUXIWATWIP> GetWATWIP(Controller ctrl)
        {
            var ret = new List<WUXIWATWIP>();
            var wdict = new Dictionary<string, bool>();
            var sql = "SELECT distinct [WAFER] ,[STEP] ,[TestTimeStamp] FROM [WAT].[dbo].[WUXIWATTESTSTATUS] where [WAFER] <> '' order by TestTimeStamp desc";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var wafer = UT.O2S(line[0]);
                if (!wdict.ContainsKey(wafer))
                {
                    wdict.Add(wafer, true);

                    var tempvm = new WUXIWATWIP();
                    tempvm.WAFER = wafer;
                    tempvm.STEP = UT.O2S(line[1]);
                    tempvm.TESTTIMESTAMP = UT.O2T(line[2]).ToString("yyyy-MM-dd HH:mm:ss");
                    ret.Add(tempvm);
                }
            }

            if (wdict.Count > 0)
            {
                var koyodict = WuxiWATData4MG.GetKOYODict(wdict.Keys.ToList(), ctrl);
                foreach (var item in ret)
                {
                    item.VArray = WXLogic.WATSampleXY.GetArrayFromAllenSherman(item.WAFER);
                    if (koyodict.ContainsKey(item.WAFER))
                    { item.VType = "KOYO"; }
                }

                var OGPSNList = WATOGPVM.GetAllOGPWafers(ctrl);
                foreach (var item in ret)
                {
                    if (OGPSNList.Contains(item.WAFER))
                    { item.HasOGP = "YES"; }
                }
            }

            return ret;
        }

        public WUXIWATWIP()
        {
            WAFER = "";
            STEP = "";
            TESTTIMESTAMP = "";
            VType = "";
            VArray = "";
            HasOGP = "NO";
        }

        public string WAFER { set; get; }
        public string STEP { set; get; }
        public string TESTTIMESTAMP { set; get; }
        public string VType { set; get; }
        public string VArray { set; get; }
        public string HasOGP { set; get; }
    }
}
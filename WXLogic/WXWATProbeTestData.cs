using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WXLogic
{
    public class WXWATProbeTestData
    {
        public static List<WXWATProbeTestData> GetData(string coupongroup,string wafernum,bool sharedatatoallen)
        {
            var ret = new List<WXWATProbeTestData>();

            var oringaldata = WXOriginalWATData.GetData(coupongroup);
            var probedata = WXProbeData.GetData(wafernum);

            var probedict = new Dictionary<string, WXProbeData>();
            foreach (var item in probedata)
            {
                var key = item.X + ":" + item.Y;
                if (!probedict.ContainsKey(key))
                {
                    probedict.Add(key, item);
                }
            }

            var ignoredict = WXWATIgnoreDie.RetrieveIgnoreDieDict(wafernum);

            var coupondict = new Dictionary<string, bool>();
            var pbcnt0 = 0;
            var pbcnt1 = 0;
            var pbcnt2 = 0;
            

            foreach (var item in oringaldata)
            {
                var key = item.X + ":" + item.Y;
                if (ignoredict.ContainsKey(key))
                { continue; }

                if (!coupondict.ContainsKey(item.Containername))
                { coupondict.Add(item.Containername, true); }

                var tempvm = new WXWATProbeTestData(item.TestTimeStamp, coupongroup, item.TestStation
                    ,item.RP,item.UnitNum,item.X,item.Y,item.BINNum,item.BINName);
                tempvm.CommonTestName = "BVR_LD_A";
                tempvm.TestValue = UT.O2D(item.BVR_LD_A);
                ret.Add(tempvm);

                tempvm = new WXWATProbeTestData(item.TestTimeStamp, coupongroup, item.TestStation
                    , item.RP, item.UnitNum, item.X, item.Y, item.BINNum, item.BINName);
                tempvm.CommonTestName = "PO_LD_W";
                tempvm.TestValue = UT.O2D(item.PO_LD_W);
                ret.Add(tempvm);

                tempvm = new WXWATProbeTestData(item.TestTimeStamp, coupongroup, item.TestStation
                    , item.RP, item.UnitNum, item.X, item.Y, item.BINNum, item.BINName);
                tempvm.CommonTestName = "VF_LD_V";
                tempvm.TestValue = UT.O2D(item.VF_LD_V);
                ret.Add(tempvm);

                tempvm = new WXWATProbeTestData(item.TestTimeStamp, coupongroup, item.TestStation
                   , item.RP, item.UnitNum, item.X, item.Y, item.BINNum, item.BINName);
                tempvm.CommonTestName = "SLOPE_WperA";
                tempvm.TestValue = UT.O2D(item.SLOPE_WperA);
                if (probedict.ContainsKey(key))
                { tempvm.ProbeValue = probedict[key].SlopEff; pbcnt0++;}
                ret.Add(tempvm);

                tempvm = new WXWATProbeTestData(item.TestTimeStamp, coupongroup, item.TestStation
                   , item.RP, item.UnitNum, item.X, item.Y, item.BINNum, item.BINName);
                tempvm.CommonTestName = "THOLD_A";
                tempvm.TestValue = UT.O2D(item.THOLD_A);
                if (probedict.ContainsKey(key))
                { tempvm.ProbeValue = probedict[key].Ith; pbcnt1++; }
                ret.Add(tempvm);

                tempvm = new WXWATProbeTestData(item.TestTimeStamp, coupongroup, item.TestStation
                   , item.RP, item.UnitNum, item.X, item.Y, item.BINNum, item.BINName);
                tempvm.CommonTestName = "R_LD_ohm";
                tempvm.TestValue = UT.O2D(item.R_LD_ohm);
                if (probedict.ContainsKey(key))
                { tempvm.ProbeValue = probedict[key].SeriesR; pbcnt2++; }
                ret.Add(tempvm);

                tempvm = new WXWATProbeTestData(item.TestTimeStamp, coupongroup, item.TestStation
                   , item.RP, item.UnitNum, item.X, item.Y, item.BINNum, item.BINName);
                tempvm.CommonTestName = "IMAX_A";
                tempvm.TestValue = UT.O2D(item.IMAX_A);
                ret.Add(tempvm);
            }

            var couponcount = coupondict.Count();
            var probecount = Math.Max(pbcnt0, Math.Max(pbcnt1, pbcnt2));
            foreach (var item in ret)
            {
                item.ProbeCount = probecount;
                item.CouponCount = couponcount;
            }

            if (sharedatatoallen)
            {
                ShareDataToAllen(coupongroup, ret);
            }

            return ret;
        }

        private static bool CheckSharedData(string coupongroup, string rp)
        {
            var sql = "select top 1 * from [EngrData].[dbo].[WUX_WATShareData] where ContainerNum  = @ContainerNum and RP = @RP";
            var dict = new Dictionary<string, string>();
            dict.Add("@ContainerNum", coupongroup);
            dict.Add("@RP", rp);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            if (dbret.Count > 0)
            {
                sql = "delete from [EngrData].[dbo].[WUX_WATShareData] where ContainerNum  = @ContainerNum and RP = @RP";
                DBUtility.ExeLocalSqlNoRes(sql, dict);
                return true;
            }

            return true;
        }

        private static void ShareDataToAllen(string coupongroup,List<WXWATProbeTestData> sharedata)
        {
            var rpdict = new Dictionary<string, List<WXWATProbeTestData>>();
            foreach (var item in sharedata)
            {
                if (rpdict.ContainsKey(item.RP))
                {
                    rpdict[item.RP].Add(item);
                }
                else
                {
                    var templist = new List<WXWATProbeTestData>();
                    templist.Add(item);
                    rpdict.Add(item.RP, templist);
                }
            }

            foreach (var rkv in rpdict)
            {
                if (CheckSharedData(coupongroup,rkv.Key))
                {
                    var sharelist = rkv.Value;
                    foreach (var data in sharelist)
                    {
                        var sql = @"insert into [EngrData].[dbo].[WUX_WATShareData](TimeStamp,ContainerNum,ToolName,RP,UnitNum,X,Y,CommonTestName,TestValue,ProbeValue,BinNum,BinName) 
                                      values(@TimeStamp,@ContainerNum,@ToolName,@RP,@UnitNum,@X,@Y,@CommonTestName,@TestValue,@ProbeValue,@BinNum,@BinName)";
                        var dict = new Dictionary<string, string>();
                        dict.Add("@TimeStamp",data.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"));
                        dict.Add("@ContainerNum",data.ContainerNum);
                        dict.Add("@ToolName",data.ToolName);
                        dict.Add("@RP",data.RP);
                        dict.Add("@UnitNum",data.UnitNum);
                        dict.Add("@X",data.X);
                        dict.Add("@Y",data.Y);
                        dict.Add("@CommonTestName",data.CommonTestName);
                        dict.Add("@TestValue",data.TestValue.ToString());
                        dict.Add("@ProbeValue",data.ProbeValue);
                        dict.Add("@BinNum",data.BinNum);
                        dict.Add("@BinName",data.BinName);
                        DBUtility.ExeLocalSqlNoRes(sql,dict);
                    }
                }
            }//end foreach
        }

        public static int GetReadCount(List<WXWATProbeTestData> srcdata, string rp)
        {
            var dict1 = new Dictionary<string, WXWATProbeTestData>();
            foreach (var item in srcdata)
            {
                if (string.Compare(item.RP, rp, true) == 0)
                {
                    var key = item.RP + ":" + item.TimeStamp.ToString() + ":" + item.UnitNum + ":" + item.CommonTestName;
                    if (!dict1.ContainsKey(key))
                    {
                        var tempvm = new WXWATProbeTestData();
                        tempvm.RP = item.RP;
                        tempvm.TimeStamp = item.TimeStamp;
                        tempvm.UnitNum = item.UnitNum;
                        tempvm.CommonTestName = item.CommonTestName;
                        dict1.Add(key, tempvm);
                    }//end if
                }//end if
            }//end foreach

            var vallist = dict1.Values.ToList();
            var dict2 = new Dictionary<string, int>();
            foreach (var item in vallist)
            {
                var key = item.UnitNum + ":" + item.CommonTestName;
                if (dict2.ContainsKey(key))
                { dict2[key] += 1; }
                else
                { dict2.Add(key, 1); }
            }//end foreach

            var clist = dict2.Values.ToList();
            if (clist.Count > 0)
            { return clist.Max(); }
            else
            { return 0; }

        }

        private WXWATProbeTestData(DateTime dt,string container,string toolnm
            ,string rp,string unit,string x,string y,string binnum,string binnm)
        {
            TimeStamp = dt;
            ContainerNum = container;
            ToolName = toolnm;
            RP = rp;
            UnitNum = unit;
            X = x;
            Y = y;
            BinNum = binnum;
            BinName = binnm;

            CommonTestName = "";
            ProbeValue = "";

            ProbeCount = 0;
            CouponCount = 0;
        }

        public WXWATProbeTestData()
        {
            TimeStamp = DateTime.Parse("1982-05-06 10:00:00");
            ContainerNum = "";
            ToolName = "";
            RP = "";
            UnitNum = "";
            X = "";
            Y = "";
            CommonTestName = "";
            TestValue = 0.0;
            ProbeValue = "";
            BinNum = "";
            BinName = "";
            ProbeCount = 0;
            CouponCount = 0;
        }

        public DateTime TimeStamp { set; get; }
        public string ContainerNum { set; get; }
        public string ToolName { set; get; }
        public string RP { set; get; }
        public string UnitNum { set; get; }
        public string X { set; get; }
        public string Y { set; get; }
        public string CommonTestName { set; get; }
        public double TestValue { set; get; }
        public string ProbeValue { set; get; }
        public string BinNum { set; get; }
        public string BinName { set; get; }

        public int ProbeCount { set; get; }
        public int CouponCount { set; get; }
    }
}
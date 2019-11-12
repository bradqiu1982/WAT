using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class WXWATTestData
    {
        //public static List<WXWATTestData> GetData(string coupongroup)
        //{
        //    var samplexy = WATSampleXY.GetSampleXYByCouponGroup(coupongroup);
        //    var samplexydict = new Dictionary<string, WATSampleXY>();
        //    foreach (var sitem in samplexy)
        //    {
        //        var key = sitem.CouponID + "-" + sitem.ChannelInfo;
        //        if (!samplexydict.ContainsKey(key))
        //        { samplexydict.Add(key, sitem); }
        //    }


        //    var ret = new List<WXWATTestData>();
        //    var sql = "select TestTimeStamp,Containername,Product,TestStation,TestStep,BVR_LD_A,PO_LD_W,VF_LD_V,SLOPE_WperA,THOLD_A,R_LD_ohm,IMAX_A,Notes,ChannelInfo,KINK2BETTER from Insite.dbo.ProductionResult where Containername like '<coupongroup>%' order by TestTimeStamp desc";
        //    sql = sql.Replace("<coupongroup>", coupongroup);

        //    var dbret = DBUtility.ExeLocalSqlWithRes(sql);
        //    var uniqdict = new Dictionary<string, bool>();

        //    foreach (var line in dbret)
        //    {
        //        if (uniqdict.ContainsKey(UT.O2S(line[1]) + "-" + UT.O2S(line[13]) + "-" + UT.O2T(line[0]).ToString("yyyy-MM-dd HH:mm:ss")))
        //        { continue; }

        //        var tempvm = new WXWATTestData();
        //        tempvm.TestTimeStamp = UT.O2T(line[0]);
        //        tempvm.Containername = UT.O2S(line[1]).Substring(0, 14);
        //        tempvm.Product = UT.O2S(line[2]);
        //        tempvm.TestStation = UT.O2S(line[3]);
        //        tempvm.TestStep = UT.O2S(line[4]);
        //        tempvm.BVR_LD_A = UT.O2S(line[5]);
        //        tempvm.PO_LD_W = UT.O2S(line[6]);
        //        tempvm.VF_LD_V = UT.O2S(line[7]);
        //        tempvm.SLOPE_WperA = UT.O2S(line[8]);
        //        tempvm.THOLD_A = UT.O2S(line[9]);
        //        tempvm.R_LD_ohm = UT.O2S(line[10]);
        //        tempvm.IMAX_A = UT.O2S(line[11]);

        //        tempvm.Notes = UT.O2S(line[12]);
        //        tempvm.ChannelInfo = UT.O2S(line[13]);

        //        tempvm.KINK2BETTER = UT.O2S(line[14]);

        //        tempvm.RP = TestName2RP(tempvm.TestStep);

        //        //var bin = tempvm.Notes.Split(new string[] { ":" }, StringSplitOptions.None).ToList();
        //        //tempvm.BINNum = bin[0];
        //        //tempvm.BINName = bin[1];

        //        var xykey = tempvm.Containername + "-" + tempvm.ChannelInfo;
        //        if (samplexydict.ContainsKey(xykey))
        //        {
        //            tempvm.X = samplexydict[xykey].X;
        //            tempvm.Y = samplexydict[xykey].Y;
        //            tempvm.UnitNum = tempvm.X + "-" + tempvm.Y;
        //            uniqdict.Add(tempvm.Containername + "-" + tempvm.ChannelInfo + "-" + tempvm.TestTimeStamp.ToString("yyyy-MM-dd HH:mm:ss"), true);
        //            ret.Add(tempvm);
        //        }
        //    }

        //    return ret;
        //}

        public static List<string> GetAllCouponID()
        {
            var retdict = new Dictionary<string,bool>();
            var sql = "select distinct Containername from Insite.dbo.ProductionResult where len(Containername) > 14 and (Containername like '%E%' or Containername like '%R%')";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            foreach (var line in dbret)
            {
                var couponid = UT.O2S(line[0]).Split(new string[] { "_" },StringSplitOptions.RemoveEmptyEntries)[0];
                if (couponid.Length >= 14)
                {
                    couponid = couponid.Substring(0, couponid.Length - 2);
                    if (!retdict.ContainsKey(couponid))
                    { retdict.Add(couponid, true); }
                }
            }

            return retdict.Keys.ToList();
        }

        public static string RP2TestName(int rp)
        {
            if (rp == 0)
            { return "PRLL_VCSEL_Pre_Burn_in_Test"; }
            else if (rp == 1)
            { return "PRLL_VCSEL_Post_Burn_in_Test"; }
            else if (rp == 2)
            { return "PRLL_Post_HTOL1_Test"; }
            else if (rp == 3)
            { return "PRLL_Post_HTOL2_Test"; }
            else
            { return ""; }

        }

        public static string TestName2RP(string testname)
        {
            var stepnametrim = testname.Replace(" ", "").ToUpper();

            var cfg = WXCfg.GetSysCfg();
            if (cfg.Count > 0)
            {
                if (cfg.ContainsKey(stepnametrim))
                {
                    return cfg[stepnametrim];
                }
                else
                {
                    return "";
                }
            }
            else
            {
                if (string.Compare(stepnametrim, "PRLL_VCSEL_Pre_Burn_in_Test", true) == 0)
                { return "0"; }
                else if (string.Compare(stepnametrim, "PRLL_VCSEL_Post_Burn_in_Test", true) == 0)
                { return "1"; }
                else if (string.Compare(stepnametrim, "PRLL_Post_HTOL1_Test", true) == 0)
                { return "2"; }
                else if (string.Compare(stepnametrim, "PRLL_Post_HTOL2_Test", true) == 0)
                { return "3"; }
                else
                { return ""; }
            }
        }


        public WXWATTestData()
        {
            ChannelInfo = "";
            Containername = "";
            TestTimeStamp = DateTime.Now;
            TestDuration_s = "";
            TestStation = "";
            BVR_LD_A = "0.0";
            PO_LD_W = "0.0";
            VF_LD_V = "0.0";
            SLOPE_WperA = "0.0";
            THOLD_A = "0.0";
            R_LD_ohm = "0.0";
            IMAX_A = "0.0";
            KINK2BETTER = "0.0";
            VI_MASK = "0.0";
            VI_KINK = "0.0";
            Notes = "";
            TestStep = "";

            UpdateCode = "";
            UploadCode = "";

            SN = "";
            Product = "";
            SoftVer = "";
            SpecVer = "";

            X = "";
            Y = "";
            ReadPoint = -1;
            UnitNum = "";

            BINNum = "0";
            BINName = "PASS";
        }

        public string ChannelInfo { set; get; }
        public string Containername { set; get; }
        public DateTime TestTimeStamp { set; get; }
        public string TestDuration_s { set; get; }
        public string TestStation { set; get; }
        public string BVR_LD_A { set; get; }
        public string PO_LD_W { set; get; }
        public string VF_LD_V { set; get; }
        public string SLOPE_WperA { set; get; }
        public string THOLD_A { set; get; }
        public string R_LD_ohm { set; get; }
        public string IMAX_A { set; get; }
        public string KINK2BETTER { set; get; }
        public string VI_MASK { set; get; }
        public string VI_KINK { set; get; }
        public string Notes { set; get; }
        public string TestStep { set; get; }
        public string UpdateCode { set; get; }
        public string UploadCode { set; get; }
        public string SN { set; get; }
        public string Product { set; get; }
        public string SoftVer { set; get; }
        public string SpecVer { set; get; }



        public string X { set; get; }
        public string Y { set; get; }
        public string RP { set; get; }

        public int ReadPoint { set; get; }
        public string UnitNum { set; get; }
        public string BINNum { set; get; }
        public string BINName { set; get; }

    }
}
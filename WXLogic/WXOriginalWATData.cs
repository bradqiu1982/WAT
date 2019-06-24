using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WXLogic
{
    public class WXOriginalWATData
    {
        //   public static bool PrepareFakeDataFromAllen(string ContainerName)
     //   {
     //       var sql = @"SELECT 
					//e.[TestTimeStamp] as TIME_STAMP
					//,e.[Containername] as CONTAINER_NUMBER
					//,pb.productname as EVAL_PRODUCT_ID
					//,e.[TestStation] as TOOL_NAME
					//,convert(varchar,e.[Rel_Lot]) as READ_POINT
					//,e.[Pos] as UNIT_NUMBER
					//,xy.xcoord as X
					//,xy.ycoord as Y
					//,e.[BVR_LD_A],e.[PO_LD_W],e.[VF_LD_V],e.[SLOPE_WperA],e.[KINK],e.[THOLD_A],e.[ROLL],e.[R_LD_ohm],e.[IMAX_A]
     //               ,e.[Result] as BIN_NUMBER
					//,e.[ErrAbbr] as BIN_NAME
     //               FROM [EngrData].[insite].[dc_TO_Vcsel_50up] e with(nolock)
				 //   LEFT JOIN [EngrData].[dbo].[Eval_XY_Coordinates] xy with(nolock) on xy.container=e.containername AND xy.devicenumber=e.pos
     //               INNER JOIN insite.insite.container c with(nolock) on c.containername = e.containername
		   //         INNER JOIN insite.insite.product p with(nolock) on p.productid=c.productid
		   //         INNER JOIN insite.insite.productbase pb with(nolock) on pb.productbaseid=p.productbaseid
     //               where e.containername = @containername  AND ([OsaPartNum] = pb.productname+'_TO'  OR [OsaPartNum] = pb.productname OR [OsaPartNum]+'_B' = replace(pb.productname,'_B','_TO_B')
					//																						OR [OsaPartNum] = replace(pb.productname,'_12mA','_TO') )";

     //       var ret = new List<WXOriginalWATData>();
     //       var dict = new Dictionary<string, string>();
     //       dict.Add("@containername", ContainerName);
     //       var dbret = DBUtility.ExeAllenSqlWithRes(sql, dict);


     //       foreach (var line in dbret)
     //       {
     //           var tempvm = new WXOriginalWATData();
     //           tempvm.ChannelInfo = UT.O2S(line[6]) + ":" + UT.O2S(line[7]);

     //           tempvm.TestTimeStamp = UT.O2T(line[0]);
     //           tempvm.Containername = UT.O2S(line[1]);
     //           tempvm.Product = UT.O2S(line[2]);
     //           tempvm.TestDuration_s = "18";
     //           tempvm.TestStation = UT.O2S(line[3]);
     //           tempvm.ReadPoint = UT.O2I(line[4]);

     //           tempvm.TestStep = RP2TestName(tempvm.ReadPoint);

     //           tempvm.BVR_LD_A = UT.O2D(line[8]).ToString();
     //           tempvm.PO_LD_W = UT.O2D(line[9]).ToString();
     //           tempvm.VF_LD_V = UT.O2D(line[10]).ToString();
     //           tempvm.SLOPE_WperA = UT.O2D(line[11]).ToString();
     //           //tempvm.KINK2BETTER = UT.O2D(line[12]).ToString();
     //           tempvm.THOLD_A = UT.O2D(line[13]).ToString();
     //           //tempvm.ROLL = UT.O2D(line[14]).ToString();
     //           tempvm.R_LD_ohm = UT.O2D(line[15]).ToString();
     //           tempvm.IMAX_A = UT.O2D(line[16]).ToString();

     //           tempvm.Notes = UT.O2D(line[17]) + ":" + UT.O2D(line[18]);

     //           ret.Add(tempvm);
     //       }

     //       if (ret.Count > 0)
     //       {
     //           dict = new Dictionary<string, string>();
     //           dict.Add("@containername", ContainerName);
     //           sql = "delete from EngrData.dbo.ProductionResult where Containername = @Containername";
     //           DBUtility.ExeLocalSqlNoRes(sql, dict);
     //       }

     //       sql = @"insert into EngrData.dbo.ProductionResult(TestId,ChannelInfo,Containername,TestTimeStamp,TestDuration_s,TestStation,
     //               BVR_LD_A,PO_LD_W,VF_LD_V,SLOPE_WperA,THOLD_A,R_LD_ohm,IMAX_A,KINK2BETTER,VI_MASK,VI_KINK,Notes,TestStep,UpdateCode,
     //               SN,Product,SoftVer,SpecVer) values(@TestId,@ChannelInfo,@Containername,@TestTimeStamp,@TestDuration_s,@TestStation,
     //               @BVR_LD_A,@PO_LD_W,@VF_LD_V,@SLOPE_WperA,@THOLD_A,@R_LD_ohm,@IMAX_A,@KINK2BETTER,@VI_MASK,@VI_KINK,@Notes,@TestStep,@UpdateCode,
     //               @SN,@Product,@SoftVer,@SpecVer)";


     //       foreach (var item in ret)
     //       {
     //           dict = new Dictionary<string, string>();
     //           dict.Add("@TestId", GetUniqKey());
     //           dict.Add("@ChannelInfo", item.ChannelInfo);
     //           dict.Add("@Containername", item.Containername);
     //           dict.Add("@TestTimeStamp", item.TestTimeStamp.ToString("yyyy-MM-dd HH:mm:ss"));
     //           dict.Add("@TestDuration_s", item.TestDuration_s);
     //           dict.Add("@TestStation", item.TestStation);
     //           dict.Add("@BVR_LD_A", item.BVR_LD_A);
     //           dict.Add("@PO_LD_W", item.PO_LD_W);
     //           dict.Add("@VF_LD_V", item.VF_LD_V);
     //           dict.Add("@SLOPE_WperA", item.SLOPE_WperA);
     //           dict.Add("@THOLD_A", item.THOLD_A);
     //           dict.Add("@R_LD_ohm", item.R_LD_ohm);
     //           dict.Add("@IMAX_A", item.IMAX_A);
     //           dict.Add("@KINK2BETTER", item.KINK2BETTER);
     //           dict.Add("@VI_MASK", item.VI_MASK);
     //           dict.Add("@VI_KINK", item.VI_KINK);
     //           dict.Add("@Notes", item.Notes);
     //           dict.Add("@TestStep", item.TestStep);
     //           dict.Add("@UpdateCode", item.UpdateCode);
     //           dict.Add("@UploadCode", item.UploadCode);
     //           dict.Add("@SN", item.SN);
     //           dict.Add("@Product", item.Product);
     //           dict.Add("@SoftVer", item.SoftVer);
     //           dict.Add("@SpecVer", item.SpecVer);
     //           DBUtility.ExeLocalSqlNoRes(sql, dict);
     //       }

     //       if (ret.Count > 0)
     //       { return true; }
     //       else
     //       { return false; }
     //   }


        private static string GetUniqKey()
        {
            return Guid.NewGuid().ToString("N");
        }

        //public static List<WXOriginalWATData> GetFakeData(string containername)
        //{
        //    var ret = new List<WXOriginalWATData>();

        //    var sql = "select TestTimeStamp,Containername,Product,TestStation,TestStep,BVR_LD_A,PO_LD_W,VF_LD_V,SLOPE_WperA,THOLD_A,R_LD_ohm,IMAX_A,Notes,ChannelInfo from EngrData.dbo.ProductionResult where Containername = @Containername order by TestTimeStamp desc";
        //    var dict = new Dictionary<string, string>();
        //    dict.Add("@Containername", containername);

        //    var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
        //    var channeldict = new Dictionary<string, bool>();

        //    foreach (var line in dbret)
        //    {
        //        if (channeldict.ContainsKey(UT.O2S(line[13])+"-"+ UT.O2T(line[0]).ToString("yyyy-MM-dd HH:mm:ss")))
        //        { continue; }

        //        var tempvm = new WXOriginalWATData();
        //        tempvm.TestTimeStamp = UT.O2T(line[0]);
        //        tempvm.Containername = UT.O2S(line[1]);
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

        //        channeldict.Add(tempvm.ChannelInfo+"-"+tempvm.TestTimeStamp.ToString("yyyy-MM-dd HH:mm:ss"), true);


        //        tempvm.RP = TestName2RP(tempvm.TestStep);

        //        //tempvm.Product = GetPN(tempvm.Containername, tempvm.Product);
        //        var xy = GetFakeXY(tempvm.Containername, tempvm.ChannelInfo);
        //        tempvm.X = xy[0];
        //        tempvm.Y = xy[1];
        //        tempvm.UnitNum = tempvm.X+"-"+tempvm.Y;

        //        var bin = tempvm.Notes.Split(new string[] { ":" }, StringSplitOptions.None).ToList();
        //        tempvm.BINNum = bin[0];
        //        tempvm.BINName = bin[1];

        //        ret.Add(tempvm);
        //    }

        //    return ret;

        //}


        public static List<WXOriginalWATData> GetData(string coupongroup)
        {
            var samplexy = WATSampleXY.GetSampleXYByCouponGroup(coupongroup);
            var samplexydict = new Dictionary<string, WATSampleXY>();
            foreach (var sitem in samplexy)
            {
                var key = sitem.CouponID + "-" + sitem.ChannelInfo;
                if (!samplexydict.ContainsKey(key))
                { samplexydict.Add(key,sitem); }
            }


            var ret = new List<WXOriginalWATData>();
            var sql = "select TestTimeStamp,Containername,Product,TestStation,TestStep,BVR_LD_A,PO_LD_W,VF_LD_V,SLOPE_WperA,THOLD_A,R_LD_ohm,IMAX_A,Notes,ChannelInfo,KINK2BETTER from Insite.dbo.ProductionResult where Containername like '<coupongroup>%' order by TestTimeStamp desc";
            sql = sql.Replace("<coupongroup>", coupongroup);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql);
            var uniqdict = new Dictionary<string, bool>();

            foreach (var line in dbret)
            {
                if (uniqdict.ContainsKey(UT.O2S(line[1])+"-"+UT.O2S(line[13]) + "-" + UT.O2T(line[0]).ToString("yyyy-MM-dd HH:mm:ss")))
                { continue; }

                var tempvm = new WXOriginalWATData();
                tempvm.TestTimeStamp = UT.O2T(line[0]);
                tempvm.Containername = UT.O2S(line[1]).Substring(0,14);
                tempvm.Product = UT.O2S(line[2]);
                tempvm.TestStation = UT.O2S(line[3]);
                tempvm.TestStep = UT.O2S(line[4]);
                tempvm.BVR_LD_A = UT.O2S(line[5]);
                tempvm.PO_LD_W = UT.O2S(line[6]);
                tempvm.VF_LD_V = UT.O2S(line[7]);
                tempvm.SLOPE_WperA = UT.O2S(line[8]);
                tempvm.THOLD_A = UT.O2S(line[9]);
                tempvm.R_LD_ohm = UT.O2S(line[10]);
                tempvm.IMAX_A = UT.O2S(line[11]);

                tempvm.Notes = UT.O2S(line[12]);
                tempvm.ChannelInfo = UT.O2S(line[13]);

                tempvm.KINK2BETTER = UT.O2S(line[14]);
 
                tempvm.RP = TestName2RP(tempvm.TestStep);
                
                //var bin = tempvm.Notes.Split(new string[] { ":" }, StringSplitOptions.None).ToList();
                //tempvm.BINNum = bin[0];
                //tempvm.BINName = bin[1];

                var xykey = tempvm.Containername + "-" + tempvm.ChannelInfo;
                if (samplexydict.ContainsKey(xykey))
                {
                    tempvm.X = samplexydict[xykey].X;
                    tempvm.Y = samplexydict[xykey].Y;
                    tempvm.UnitNum = (UT.O2I(tempvm.Containername.Substring(12,2))*10000+UT.O2I(tempvm.ChannelInfo)).ToString();
                    uniqdict.Add(tempvm.Containername +"-"+tempvm.ChannelInfo + "-" + tempvm.TestTimeStamp.ToString("yyyy-MM-dd HH:mm:ss"), true);
                    ret.Add(tempvm);
                }
            }

            return ret;
        }



        ////THIS FUNCTION NEED TO BE UPDATE
        //private static List<string> GetFakeXY(string containername, string channelinfo)
        //{
        //    return channelinfo.Split(new string[] { ":" }, StringSplitOptions.None).ToList();
        //}



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


        public WXOriginalWATData()
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
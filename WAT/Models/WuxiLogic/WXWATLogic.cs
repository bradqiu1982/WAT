using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WAT.Models
{
    public class WXWATLogic
    {
        public static WXWATLogic WATPassFail(string CouponID_,string CurrentStepName)
        {
            var ret = new WXWATLogic();

            var bitemp = 100;
            var shippable = 1;

            var CouponID = "";
            if (CouponID_.Length < 12)
            {
                ret.AppErrorMsg = "Get an illege couponid: " + CouponID_;
                return ret;
            }
            else
            {
                CouponID = CouponID_.Substring(0, 12);
            }

            var testname = GetTestNameFromCurrentStep(CurrentStepName);
            if (string.IsNullOrEmpty(testname))
            {
                ret.AppErrorMsg = "Fail to get test name for current step: " + CurrentStepName;
                return ret;
            }

            var RP = WXOriginalWATData.TestName2RP(testname);
            var DCDName = GetDCDName(CouponID, RP);
            if (string.IsNullOrEmpty(DCDName))
            {
                ret.AppErrorMsg = "Fail to get dcdname from coupon: " + CouponID;
                return ret;
            }

            var containerinfo = WXContainerInfo.GetInfo(CouponID);
            if (string.IsNullOrEmpty(containerinfo.ProductName))
            {
                ret.AppErrorMsg = "Fail to get eval productname from : " + CouponID;
                return ret;
            }

            var watprobeval = WXWATProbeTestData.GetData(CouponID, containerinfo.wafer, testname);
            if (watprobeval.Count == 0)
            {
                ret.AppErrorMsg = "Fail to get WAT test data by : " + CouponID;
                return ret;
            }
            var probecount = watprobeval[0].ProbeCount;
            var readcount = WXWATProbeTestData.GetReadCount(watprobeval, RP);


            return new WXWATLogic();
        }


        private static string GetTestNameFromCurrentStep(string stepname)
        {
            var stepnametrim = stepname.Replace(" ", "").ToUpper();

            var cfg = WXCfg.GetSysCfg();
            if (cfg.Count > 0)
            {
                if (cfg.ContainsKey(stepnametrim))
                {
                    return cfg[stepnametrim];
                }
                else
                {
                    return string.Empty;
                }
            }
            else
            {
                if (string.Compare(stepnametrim, "POSTBIJUDGEMENT") == 0)
                {
                    return "PRLL_VCSEL_Post_Burn_in_Test";
                }
                else if (string.Compare(stepnametrim, "POSTHTOL1JUDGEMENT") == 0)
                {
                    return "PRLL_Post_HTOL1_Test";
                }
                else if (string.Compare(stepnametrim, "POSTHTOL2JUDGEMENT") == 0)
                {
                    return "PRLL_Post_HTOL2_Test";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        private static string GetDCDName(string couponid, string rp)
        {
            var cp = couponid.ToUpper();
            var rpstr = "rp"+(100 + UT.O2I(rp)).ToString().Substring(1);

            if (cp.Contains("E08")
                || cp.Contains("E01")
                || cp.Contains("E06"))
            {
                return "Eval_50up_" + rpstr;
            }
            else if (cp.Contains("E07")
                || cp.Contains("E10")
                || cp.Contains("E09")
                || cp.Contains("E03"))
            {
                return "Eval_COB_" + rpstr;
            }
            else
            {
                return "";
            }
        }


        public static void Usage()
        {
            var wuxlogic = new WXWATLogic();
            if (!string.IsNullOrEmpty(wuxlogic.AppErrorMsg))
            {
                //hold
                //reason: wuxlogic.AppErrorMsg
            }
            else
            {
                if (wuxlogic.TestPass)
                {
                    //move next
                }
                else
                {
                    if (wuxlogic.ScrapIt)
                    {
                        //hold
                        //reason: "Scrap:" + wuxlogic.ResultReason
                    }
                    else
                    {
                        if (wuxlogic.NeedRetest)
                        {
                            //hold
                            //reason: wuxlogic.ResultReason
                        }
                        else
                        {
                            //hold
                            //reason: wuxlogic.ResultReason,wait for PE to judgement
                        }
                    }//end else
                }//end else
            }//end else
        }

        public WXWATLogic()
        {
            TestPass = false;
            NeedRetest = false;
            ScrapIt = false;
            ResultReason = "";
            AppErrorMsg = "";
        }

        public bool TestPass { set; get; } //test pass
        public bool NeedRetest { set; get; } //whether need to retest
        public bool ScrapIt { set; get; } //whether scrap
        public string ResultReason { set; get; } //retest/scrap reason
        public string AppErrorMsg { set; get; } //for app logic error
    }
}
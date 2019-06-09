using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WAT.Models
{
    public class WXWATLogic
    {
        public static WXWATLogic WATPassFail(string coupongroup1,string CurrentStepName)
        {
            var ret = new WXWATLogic();

            var cfg = WXCfg.GetSysCfg();
            var sharedatatoallen = true;
            if (cfg.ContainsKey("SHARETOALLEN") && cfg["SHARETOALLEN"].Contains("FALSE"))
            { sharedatatoallen = false; }

            var bitemp = 100;
            var shippable = 1;

            var CouponGroup = "";
            if (coupongroup1.Length < 12)
            {
                ret.AppErrorMsg = "Get an illege couponid: " + coupongroup1;
                return ret;
            }
            else
            {
                CouponGroup = coupongroup1.Substring(0, 12);
            }

            
            var testname = GetTestNameFromCurrentStep(CurrentStepName);
            if (string.IsNullOrEmpty(testname))
            {
                ret.AppErrorMsg = "Fail to get test name for current step: " + CurrentStepName;
                return ret;
            }

            var RP = WXOriginalWATData.TestName2RP(testname);
            if (string.IsNullOrEmpty(RP))
            {
                ret.AppErrorMsg = "Fail to get read point for test: " + testname;
                return ret;
            }


            var DCDName = GetDCDName(CouponGroup, RP);
            if (string.IsNullOrEmpty(DCDName))
            {
                ret.AppErrorMsg = "Fail to get dcdname from coupon group: " + CouponGroup;
                return ret;
            }
            
            //Container Info
            var containerinfo = WXContainerInfo.GetInfo(CouponGroup);
            if (string.IsNullOrEmpty(containerinfo.ProductName))
            {
                ret.AppErrorMsg = "Fail to get eval productname from : " + CouponGroup;
                return ret;
            }

            //SPEC
            var allspec = WXSpecBinPassFail.GetAllSpec();
            var dutminitem = WXSpecBinPassFail.GetMinDUT(containerinfo.ProductName, DCDName, allspec);
            if (dutminitem.Count == 0)
            {
                //System.Windows.MessageBox.Show("Fail to get min DUT count.....");
                ret.AppErrorMsg = "Fail to get min DUT count.....";
                return ret;
            }

            //WAT PROB
            var watprobeval = WXWATProbeTestData.GetData(CouponGroup, containerinfo.wafer, sharedatatoallen);
            if (watprobeval.Count == 0)
            {
                ret.AppErrorMsg = "Fail to get WAT test data by : " + CouponGroup;
                return ret;
            }
            var probecount = watprobeval[0].ProbeCount;
            var readcount = WXWATProbeTestData.GetReadCount(watprobeval, RP);

            //WAT PROB FILTER
            var watprobevalfiltered = WXWATProbeTestDataFiltered.GetFilteredData(watprobeval, RP);
            if (watprobevalfiltered.Count == 0)
            {
                //System.Windows.MessageBox.Show("Fail to get wat prob filtered data.....");
                ret.AppErrorMsg = "Fail to get wat prob filtered data.....";
                return ret;
            }

            //FAIL MODE
            var spec4fmode = WXSpecBinPassFail.GetParam4FailMode(containerinfo.ProductName, RP, allspec);
            var failmodes = WXWATProbeTestDataFiltered.GetWATFailureModes(watprobevalfiltered, spec4fmode, bitemp);


            //Coupon Stat Data
            var binpndict = WXSpecBinPassFail.RetrieveBinDict(containerinfo.ProductName, allspec);
            var couponstatdata = WXWATCouponStats.GetCouponData(watprobevalfiltered, binpndict);

            //CPK
            var cpkspec = WXSpecBinPassFail.GetCPKSpec(containerinfo.ProductName, DCDName, allspec);
            var cpktab = WXWATCPK.GetCPK(RP, couponstatdata, watprobevalfiltered, cpkspec);

            //TTF
            var fitspec = WXSpecBinPassFail.GetFitSpec(containerinfo.ProductName, DCDName, allspec);
            var ttfdata = WXWATTTF.GetTTFData(containerinfo.ProductName,UT.O2I(RP), fitspec, watprobevalfiltered, failmodes);

            //TTFSorted
            var ttfdatasorted = WXWATTTFSorted.GetSortedTTFData(ttfdata);

            //TTFTerms
            var ttftermdata = WXWATTTFTerms.GetTTRTermsData(ttfdatasorted);

            //TTFfit
            var ttffit = WXWATTTFfit.GetFitData(ttftermdata);

            //TTFuse
            var ttpspec = WXSpecBinPassFail.GetFitTTPSpec(containerinfo.ProductName, allspec);
            var ttfuse = WXWATTTFuse.GetTTFuseData(ttpspec, ttffit);

            //TTFUnit
            var ttfunitspec = WXSpecBinPassFail.GetTTFUnitSpec(containerinfo.ProductName, allspec);
            var ttfunitdata = WXWATTTFUnit.GetUnitData(watprobevalfiltered,RP, ttfuse, ttfdatasorted, ttfunitspec);

            var ttfmu = WXWATTTFmu.GetmuData(fitspec, RP, watprobevalfiltered, ttfuse, ttffit);


            //Pass Fail Unit
            var passfailunitspec = WXSpecBinPassFail.GetPassFailUnitSpec(containerinfo.ProductName, DCDName, allspec);
            var passfailunitdata = WXWATPassFailUnit.GetPFUnitData(RP, DCDName, passfailunitspec
                , watprobevalfiltered, couponstatdata, cpktab, ttfmu, ttfunitdata);


            var failcount = WXWATPassFailUnit.GetFailCount(passfailunitdata);
            var failunit = WXWATPassFailUnit.GetFailUnit(passfailunitdata);

            //Pass Fail Coupon
            var watpassfailcoupondata = WXWATPassFailCoupon.GetPFCouponData(passfailunitdata, dutminitem[0]);

            var failstring = WXWATPassFailCoupon.GetFailString(watpassfailcoupondata);

            var couponDutCount = WXWATPassFailCoupon.GetDutCount(watpassfailcoupondata);
            var couponSumFails = WXWATPassFailCoupon.GetSumFails(watpassfailcoupondata);

            var logicresult = RetestLogic(containerinfo, DCDName, UT.O2I(RP), shippable, probecount, readcount
               , dutminitem[0].minDUT, failcount, failstring, watpassfailcoupondata.Count(), couponDutCount, couponSumFails);

            var scrapspec = WXSpecBinPassFail.GetScrapSpec(containerinfo.ProductName, DCDName, allspec);
            logicresult.ScrapIt = ScrapLogic(containerinfo, scrapspec, UT.O2I(RP), readcount, failcount, bitemp, failmodes);

            return logicresult;
        }

        private static WXWATLogic RetestLogic(WXContainerInfo container, string DCDName, int rp, int shippable, int probeCount
    , int readCount, int dutMinQty, int failcount, string failstring, int couponCount, int couponDutCount, int couponSumFails)
        {
            var ret = new WXWATLogic();
            if (couponCount > 0)
            {
                if (((string.Compare(container.containertype, "T", true) != 0
                    && string.Compare(container.containertype, "E", true) != 0
                    && string.Compare(container.containertype, "D", true) != 0) && shippable == 1)
                    || rp == 0)
                {
                    if (couponDutCount >= dutMinQty)
                    {
                        if (couponSumFails == 0)
                        {
                            ret.TestPass = true;
                            ret.NeedRetest = false;
                            ret.ResultReason = "Proceed";
                        }
                        else
                        {
                            if (DCDName.ToUpper().Contains("50UP"))
                            {
                                if (readCount < 4 && failcount <= 10 && failstring.ToUpper().Contains("C-P"))
                                {
                                    ret.TestPass = false;
                                    ret.NeedRetest = true;
                                    ret.ResultReason = "Inspect, Check XY coord and fix if necessary, Check orientation and Reseat failing units and retest.";
                                }
                                else if (readCount == 1 && failcount <= 10 && failstring.ToUpper().Contains("VF_LD_V_AD_REF"))
                                {
                                    ret.TestPass = false;
                                    ret.NeedRetest = true;
                                    ret.ResultReason = "Reseat failing units and retest";
                                }
                                else if (readCount == 1 && failcount <= 10 && failstring.ToUpper().Contains("RD_REF"))
                                {
                                    ret.TestPass = false;
                                    ret.NeedRetest = true;
                                    ret.ResultReason = "Inspect, ReInsert board and retest";
                                }
                                else if (readCount == 1 && failcount <= 10 && failstring.ToUpper().Contains("AD_REF"))
                                {
                                    ret.TestPass = false;
                                    ret.NeedRetest = true;
                                    ret.ResultReason = "Inspect, ReInsert board and retest";
                                }
                                else if (readCount == 1 && failcount <= 10 && failstring.ToUpper().Contains("DB_REF"))
                                {
                                    ret.TestPass = false;
                                    ret.NeedRetest = true;
                                    ret.ResultReason = "Inspect, ReInsert board and retest";
                                }
                                else
                                {
                                    ret.TestPass = false;
                                    ret.NeedRetest = false;
                                    ret.ResultReason = "Fails. Submit and check that the container goes on hold";
                                }
                            }
                            else if (DCDName.ToUpper().Contains("COB"))
                            {
                                if (readCount == 1 && failcount <= 10 && failstring.ToUpper().Contains("C-P"))
                                {
                                    ret.TestPass = false;
                                    ret.NeedRetest = true;
                                    ret.ResultReason = "Inspect and retest";
                                }
                                else if (readCount == 1 && failstring.ToUpper().Contains("VF_RP"))
                                {
                                    ret.TestPass = false;
                                    ret.NeedRetest = true;
                                    ret.ResultReason = "Inspect, ReInsert board and retest";
                                }
                                else if (readCount == 1 && failcount <= 10 && failstring.ToUpper().Contains("RD_REF"))
                                {
                                    ret.TestPass = false;
                                    ret.NeedRetest = true;
                                    ret.ResultReason = "Inspect, ReInsert board and retest";
                                }
                                else if (readCount == 1 && failcount <= 10 && failstring.ToUpper().Contains("AD_REF"))
                                {
                                    ret.TestPass = false;
                                    ret.NeedRetest = true;
                                    ret.ResultReason = "Inspect, ReInsert board and retest";
                                }
                                else if (readCount == 1 && failcount <= 10 && failstring.ToUpper().Contains("DB_REF"))
                                {
                                    ret.TestPass = false;
                                    ret.NeedRetest = true;
                                    ret.ResultReason = "Inspect, ReInsert board and retest";
                                }
                                else
                                {
                                    ret.TestPass = false;
                                    ret.NeedRetest = false;
                                    ret.ResultReason = "Fails. Submit and check that the container goes on hold";
                                }
                            }
                            else
                            {
                                ret.TestPass = false;
                                ret.NeedRetest = false;
                                ret.ResultReason = "Fails. Submit and check that the container goes on hold";

                            }
                        }
                    }
                    else
                    {
                        if (readCount == 1 && probeCount < dutMinQty)
                        {
                            ret.TestPass = false;
                            ret.NeedRetest = true;
                            ret.ResultReason = "Not enough DUTs have xy coordinates recorded or not enough DUTs have probe data recorded.  Make sure container goes on hold and contact engineering.";
                        }
                        else if (readCount == 1)
                        {
                            ret.TestPass = false;
                            ret.NeedRetest = true;
                            ret.ResultReason = "Not enough DUTs measured. Retest container ensuring container is fully inserted into connector. Redo Check Test Data.";
                        }
                        else if (readCount > 1
                            && string.Compare(DCDName, "Eval_50up_rp00", true) == 0
                            && container.containername.ToUpper().Contains("R"))
                        {
                            ret.TestPass = false;
                            ret.NeedRetest = true;
                            ret.ResultReason = "Scrap and repick container. Notify Test Engineering that container had too many missing DUTs.";
                        }
                        else
                        {
                            ret.TestPass = false;
                            ret.NeedRetest = false;
                            ret.ResultReason = "Fails. Submit and check that the container goes on hold 88.";
                        }
                    }
                }
                else
                {
                    ret.TestPass = false;
                    ret.NeedRetest = false;
                    ret.ResultReason = "Not Production WAT.  Proceed to next step (note that in some cases the next step may be an ENG HOLD step)";
                }
            }
            //else if (allunitexclusion)
            //{
            //    ret.TestPass = false;
            //    ret.NeedRetest = false;
            //    ret.ResultReason = "All units now excluded. Hold for engineering.";
            //}
            else
            {
                if (string.Compare(container.containertype, "T", true) == 0
                    || string.Compare(container.containertype, "E", true) == 0
                    || string.Compare(container.containertype, "D", true) == 0)
                {
                    ret.TestPass = false;
                    ret.NeedRetest = true;
                    ret.ResultReason = "Data missing or product or specs not set up or WAT Engineering mode used.  Proceed per Engineering instruction.";
                }
                else
                {
                    ret.TestPass = false;
                    ret.NeedRetest = true;
                    ret.ResultReason = "Retest Required! There is no data associated with this read point. Check that the correct containername, test program and read point were used.";
                }
            }

            return ret;
        }

        private static bool ScrapLogic(WXContainerInfo containerinfo, List<WXSpecBinPassFail> scrapspec
    , int rp, int readcount, int failcount, Double bitemp, List<WXWATFailureMode> failmodes)
        {

            var bfailmode = false;
            foreach (var fm in failmodes)
            {
                if (string.Compare(fm.Failure, "WEAROUT", true) == 0
                || string.Compare(fm.Failure, "DELAM", true) == 0
                || string.Compare(fm.Failure, "DVF", true) == 0
                || string.Compare(fm.Failure, "LOWPOWERLOWLEAKAGE", true) == 0)
                {
                    bfailmode = true;
                    break;
                }
            }


            if (rp > 0 && readcount > 1 && failcount > 0 && bitemp >= 0
                && (string.Compare(containerinfo.lottype, "n", true) == 0
                || string.Compare(containerinfo.lottype, "q", true) == 0
                || string.Compare(containerinfo.lottype, "w", true) == 0
                || string.Compare(containerinfo.lottype, "r", true) == 0)
                && bfailmode  && scrapspec.Count > 0
                && (containerinfo.containername.ToUpper().Contains("E01")
                || containerinfo.containername.ToUpper().Contains("E06"))
                    )
            {
                return true;
            }

            return false;
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
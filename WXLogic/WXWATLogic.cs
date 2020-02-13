using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;


namespace WXLogic
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.
    public class WXWATLogic : IWXWATLogic
    {

        private string GetCouponGroup(string coupongroup1)
        {
            var CouponGroup = "";
            try
            {
                if (coupongroup1.Length < 12 || (!coupongroup1.Contains("E") &&!coupongroup1.Contains("R") && !coupongroup1.Contains("T")))
                { return string.Empty; }
                else
                {
                    var len = 0;
                    if (coupongroup1.Contains("E"))
                    { len = coupongroup1.IndexOf("E") + 3; }
                    else if (coupongroup1.Contains("R"))
                    { len = coupongroup1.IndexOf("R") + 3; }
                    else if (coupongroup1.Contains("T"))
                    { len = coupongroup1.IndexOf("T") + 3; }

                    if (coupongroup1.Length < len)
                    { return string.Empty; }
                    else
                    { CouponGroup = coupongroup1.Substring(0, len); }
                }
            }
            catch (Exception ex) { return string.Empty; }
            return CouponGroup;
        }



        public  WXWATLogic WATPassFail(string coupongroup1, string CurrentStepName)
        {
            var ret = new WXWATLogic();

            var cfg = WXCfg.GetSysCfg();
            var sharedatatoallen = true;
            if (cfg.ContainsKey("SHARETOALLEN") && cfg["SHARETOALLEN"].Contains("FALSE"))
            { sharedatatoallen = false; }

            if (!string.IsNullOrEmpty(AnalyzeParam))
            { sharedatatoallen = false; }

            var CouponGroup = GetCouponGroup(coupongroup1.ToUpper());
            if (string.IsNullOrEmpty(CouponGroup))
            {
                ret.AppErrorMsg = "Get an illege couponid: " + coupongroup1;
                return ret;
            }

            var bitemp = 100;
            if (CouponGroup.Contains("E06") 
                || CouponGroup.Contains("R06") 
                || CouponGroup.Contains("T06"))
            { bitemp = 25; }

            var shippable = 1;

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

            
            var waferarray = WATSampleXY.GetArrayFromAllenSherman(containerinfo.wafer);

            if (!string.IsNullOrEmpty(waferarray)
                && (CouponGroup.Contains("E08") || CouponGroup.Contains("R08")|| CouponGroup.Contains("T08"))
                && string.IsNullOrEmpty(AnalyzeParam))
            {
                var couponcount = WXOriginalWATData.GetCurrentRPTestedCoupon(CouponGroup, UT.O2I(RP));

                var necessarynum = 0;
                var arraynum = UT.O2I(waferarray);
                if (arraynum == 1)
                { necessarynum = 4; }
                else if (arraynum == 4)
                { necessarynum = 10; }
                else if (arraynum == 12)
                { necessarynum = 30; }

                if (couponcount < necessarynum)
                {
                    ret.AppErrorMsg = "For 1x"+waferarray+" wafer, the necessary coupon count of E08 WAT logic check should be great than " + necessarynum.ToString();
                    return ret;
                }
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
            var failmodestr = WXWATFailureMode.GetFailModeString(failmodes);

            if ((AnalyzeParam.Contains("_AD_")
                || AnalyzeParam.Contains("_DB_")
                || AnalyzeParam.Contains("_RD_"))
                && !AnalyzeParam.Contains("_CPK_"))
            {
                CollectAnalyzeDeltaValue(watprobevalfiltered);
                return ret;
            }

            //Coupon Stat Data
            var binpndict = WXSpecBinPassFail.RetrieveBinDict(containerinfo.ProductName, allspec);
            var couponstatdata = WXWATCouponStats.GetCouponData(watprobevalfiltered, binpndict);

            if ((AnalyzeParam.Contains("_MDD_")
                || AnalyzeParam.Contains("_MXDP_")))
            {
                CollectAnalyzePOLDSummary(couponstatdata);
                return ret;
            }

            //CPK
            var cpkspec = WXSpecBinPassFail.GetCPKSpec(containerinfo.ProductName, DCDName, allspec);
            var cpktab = WXWATCPK.GetCPK(RP, couponstatdata, watprobevalfiltered, cpkspec);

            if (AnalyzeParam.Contains("_CPK_"))
            {
                CollectAnalyzeCPKValue(cpktab);
                return ret;
            }

            //TTF
            var fitspec = WXSpecBinPassFail.GetFitSpec(containerinfo.ProductName, DCDName, allspec);
            var ttfdata = WXWATTTF.GetTTFData(containerinfo.ProductName, UT.O2I(RP), fitspec, watprobevalfiltered, failmodes);

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
            var ttfunitdata = WXWATTTFUnit.GetUnitData(watprobevalfiltered, RP, ttfuse, ttfdatasorted, ttfunitspec);

            var ttfmu = WXWATTTFmu.GetmuData(fitspec, RP, watprobevalfiltered, ttfuse, ttffit);


            //Pass Fail Unit
            var passfailunitspec = WXSpecBinPassFail.GetPassFailUnitSpec(containerinfo.ProductName, DCDName, allspec);
            var passfailunitdata = WXWATPassFailUnit.GetPFUnitData(RP, DCDName, passfailunitspec
                , watprobevalfiltered, couponstatdata, cpktab, ttfmu, ttfunitdata);

            if (AnalyzeParam.Contains("_C-P"))
            {
                CollectAnalyzeC_PValue(passfailunitdata);
                return ret;
            }

            var failcount = WXWATPassFailUnit.GetFailCount(passfailunitdata);
            var failunitinfo = WXWATPassFailUnit.GetFailUnitWithInfo(passfailunitdata);

            WXFinEvalDispositionLog.CleanFinEvalDispositionLog(CouponGroup, sharedatatoallen,RP);
            //Pass Fail Coupon
            var watpassfailcoupondata = WXWATPassFailCoupon.GetPFCouponData(passfailunitdata, dutminitem[0]
                ,sharedatatoallen,CouponGroup,RP,containerinfo);

            var failstring = WXWATPassFailCoupon.GetFailString(watpassfailcoupondata);
            var couponDutCount = WXWATPassFailCoupon.GetDutCount(watpassfailcoupondata);
            var couponSumFails = WXWATPassFailCoupon.GetSumFails(watpassfailcoupondata);

            var BIYield = 100.0;
            if (string.Compare(RP, "1") == 0)
            { BIYield = WXWATPassFailUnit.GetDutBIYield(passfailunitdata); }
            var BIYieldSpec = UT.O2D(cfg["PRODBIYIELDRP01"]);

            var logicresult = RetestLogic(containerinfo, DCDName, UT.O2I(RP), shippable, probecount, readcount
               , dutminitem[0].minDUT, failcount, failstring, watpassfailcoupondata.Count(), couponDutCount, couponSumFails,BIYield,BIYieldSpec);

            if (!string.IsNullOrEmpty(AnalyzeParam))
            { logicresult.AnalyzeParamData.AddRange(AnalyzeParamData); }

            var scrapspec = WXSpecBinPassFail.GetScrapSpec(containerinfo.ProductName, DCDName, allspec);
            logicresult.ScrapIt = ScrapLogic(containerinfo, scrapspec, UT.O2I(RP), readcount, failcount, bitemp, failmodes);
            if (logicresult.ScrapIt)
            {
                logicresult.ResultReason = failmodestr;
                logicresult.ValueCollect.Add("Scrap ?", "YES");
            }
            else
            {
                logicresult.ValueCollect.Add("Scrap ?", "NO");
                if (logicresult.NeedRetest)
                {
                    logicresult.ValueCollect.Add("ReTest ?", "YES");
                }
                else
                {
                    logicresult.ValueCollect.Add("ReTest ?", "NO");
                }
            }
            
            logicresult.ValueCollect.Add("fail unit info", failunitinfo);
            logicresult.ValueCollect.Add("failcount", failcount.ToString());
            logicresult.ValueCollect.Add("fail coupon string", failstring);
            logicresult.ValueCollect.Add("fail mode", failmodestr);
            logicresult.ValueCollect.Add("ProbeCount", probecount.ToString());
            logicresult.ValueCollect.Add("readcount", readcount.ToString());

            logicresult.DataTables.Add(watpassfailcoupondata);
            logicresult.DataTables.Add(failmodes);

            if (string.IsNullOrEmpty(logicresult.AppErrorMsg) 
                && logicresult.TestPass
                && (CouponGroup.Contains("E08") || CouponGroup.Contains("R08") || CouponGroup.Contains("E01"))
                && string.Compare(CurrentStepName.Replace(" ","").ToUpper(), "POSTHTOL2JUDGEMENT") == 0
                && string.IsNullOrEmpty(AnalyzeParam)
                && AllowToMoveMapFile)
            {
                try
                {
                    using (NativeMethods cv = new NativeMethods("brad.qiu", "china", cfg["SHAREFOLDERPWD"]))
                    {
                        MoveOriginalMapFile(containerinfo.wafer, cfg["DIESORTFOLDER"], cfg["DIESORT100PCT"]);
                    }
                }
                catch (Exception ex) { }
            }
            return logicresult;
        }

        private void CollectAnalyzeDeltaValue(List<WXWATProbeTestDataFiltered> filterdata)
        {
            var orginalname = AnalyzeParam.Split(new string[] { "_AD_", "_DB_", "_RD_" }, StringSplitOptions.RemoveEmptyEntries)[0].ToUpper();
            foreach (var srcdata in filterdata)
            {
                if (string.Compare(srcdata.CommonTestName, orginalname, true) == 0)
                {
                    if (AnalyzeParam.Contains("_AD_"))
                    {
                        if (AnalyzeParam.Contains("REF0") && AnalyzeParam.Contains("RP01"))
                        {
                            if (!string.IsNullOrEmpty(srcdata.DeltaList[0].absolutedeltaref))
                            { AnalyzeParamData.Add(new XYVAL(srcdata.X, srcdata.Y, srcdata.UnitNum, UT.O2D(srcdata.DeltaList[0].absolutedeltaref))); }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(srcdata.DeltaList[1].absolutedeltaref))
                            { AnalyzeParamData.Add(new XYVAL(srcdata.X, srcdata.Y, srcdata.UnitNum, UT.O2D(srcdata.DeltaList[1].absolutedeltaref))); }
                        }
                    }

                    if (AnalyzeParam.Contains("_DB_"))
                    {
                        if (AnalyzeParam.Contains("REF0") && AnalyzeParam.Contains("RP01"))
                        {
                            if (!string.IsNullOrEmpty(srcdata.DeltaList[0].dBdeltaref))
                            { AnalyzeParamData.Add(new XYVAL(srcdata.X, srcdata.Y, srcdata.UnitNum, UT.O2D(srcdata.DeltaList[0].dBdeltaref))); }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(srcdata.DeltaList[1].dBdeltaref))
                            { AnalyzeParamData.Add(new XYVAL(srcdata.X, srcdata.Y, srcdata.UnitNum, UT.O2D(srcdata.DeltaList[1].dBdeltaref))); }
                        }
                    }

                    if (AnalyzeParam.Contains("_RD_"))
                    {
                        if (AnalyzeParam.Contains("REF0") && AnalyzeParam.Contains("RP01"))
                        {
                            if (!string.IsNullOrEmpty(srcdata.DeltaList[0].ratiodeltaref))
                            { AnalyzeParamData.Add(new XYVAL(srcdata.X, srcdata.Y, srcdata.UnitNum, UT.O2D(srcdata.DeltaList[0].ratiodeltaref))); }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(srcdata.DeltaList[1].ratiodeltaref))
                            { AnalyzeParamData.Add(new XYVAL(srcdata.X, srcdata.Y, srcdata.UnitNum, UT.O2D(srcdata.DeltaList[1].ratiodeltaref))); }
                        }
                    }


                }
            }
        }

        private void CollectAnalyzePOLDSummary(List<WXWATCouponStats> couponstat)
        {
            var orginalname = AnalyzeParam.Split(new string[] { "_RP"}, StringSplitOptions.RemoveEmptyEntries)[0].ToUpper();
            foreach (var cp in couponstat)
            {
                foreach (var kv in cp.CPValDict)
                {
                    if (kv.Key.ToUpper().Contains(orginalname) && !string.IsNullOrEmpty(kv.Value))
                    {
                        AnalyzeParamData.Add(new XYVAL(cp.X, cp.Y,cp.UnitNum, UT.O2D(kv.Value)));
                    }
                }
            }
        }

        private void CollectAnalyzeCPKValue(List<WXWATCPK> cpktab)
        {
            var orginalname = AnalyzeParam.Split(new string[] { "_REF" }, StringSplitOptions.RemoveEmptyEntries)[0].ToUpper() + "_REF";
            foreach (var c in cpktab)
            {
                if (c.CommonTestName.ToUpper().Contains(orginalname) && !string.IsNullOrEmpty(c.TestValue))
                {
                    AnalyzeParamData.Add(new XYVAL(c.X, c.Y,c.UnitNum, UT.O2D(c.TestValue)));
                }
            }
        }

        private void CollectAnalyzeC_PValue(List<WXWATPassFailUnit> pfunit)
        {
            var orginalname = AnalyzeParam.Split(new string[] { "_RP" }, StringSplitOptions.RemoveEmptyEntries)[0].ToUpper();
            foreach (var pfu in pfunit)
            {
                if (pfu.CommonTestName.ToUpper().Contains(orginalname) && !string.IsNullOrEmpty(pfu.TVAL))
                {
                    AnalyzeParamData.Add(new XYVAL(pfu.X, pfu.Y,pfu.UnitNum, UT.O2D(pfu.TVAL)));
                }
            }
        }

        private static void MoveOriginalMapFile(string wafer, string orgfolder, string PCT100folder)
        {
            try
            {
                var filedict = new Dictionary<string, string>();
                var nofolderfiles = Directory.GetFiles(orgfolder);
                foreach (var f in nofolderfiles)
                {
                    var uf = f.ToUpper();
                    if (!filedict.ContainsKey(uf))
                    { filedict.Add(uf, f); }
                }

                var folders = Directory.GetDirectories(orgfolder);
                foreach (var fd in folders)
                {
                    var fs = Directory.GetFiles(fd);
                    foreach (var f in fs)
                    {
                        var uf = f.ToUpper();
                        if (!filedict.ContainsKey(uf))
                        { filedict.Add(uf, f); }
                    }
                }

                foreach (var kv in filedict)
                {
                    if (kv.Key.Contains(wafer.ToUpper()))
                    {
                        var desfile = Path.Combine(PCT100folder, Path.GetFileName(kv.Value));
                        File.Copy(kv.Value, desfile, true);
                        return;
                    }
                }
            }
            catch (Exception ex) { }
        }

        private static WXWATLogic RetestLogic(WXContainerInfo container, string DCDName, int rp, int shippable, int probeCount
    , int readCount, int dutMinQty, int failcount, string failstring, int couponCount, int couponDutCount, int couponSumFails, double BIYield,double BIYieldSpec)
        {
            var ret = new WXWATLogic();
            if (couponCount > 0)
            {
                if (rp == 1)
                {
                    if (BIYield < BIYieldSpec)
                    {
                        ret.TestPass = false;
                        ret.NeedRetest = true;
                        ret.ResultReason = "Fail. BI Yield "+ BIYield.ToString()+"%";
                        return ret;
                    }
                }

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


            if (rp > 0  && failcount > 0 && bitemp >= 0
                && (string.Compare(containerinfo.lottype, "n", true) == 0
                || string.Compare(containerinfo.lottype, "q", true) == 0
                || string.Compare(containerinfo.lottype, "w", true) == 0
                || string.Compare(containerinfo.lottype, "r", true) == 0)
                && bfailmode && scrapspec.Count > 0
                && (containerinfo.containername.ToUpper().Contains("E01")
                || containerinfo.containername.ToUpper().Contains("E06")
                || containerinfo.containername.ToUpper().Contains("E08")
                || containerinfo.containername.ToUpper().Contains("R01")
                || containerinfo.containername.ToUpper().Contains("R06")
                || containerinfo.containername.ToUpper().Contains("R08")
                || containerinfo.containername.ToUpper().Contains("T01")
                || containerinfo.containername.ToUpper().Contains("T06")
                || containerinfo.containername.ToUpper().Contains("T08"))
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
                if (string.Compare(stepnametrim, "PREBIJUDGEMENT") == 0)
                {
                    return "PRLL_VCSEL_Pre_Burn_in_Test";
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
            var rpstr = "rp" + (100 + UT.O2I(rp)).ToString().Substring(1);

            var charlist = new List<string>();
            charlist.Add("E");
            charlist.Add("R");
            charlist.Add("T");

            foreach (var c in charlist)
            {
                if (cp.Contains(c+"08")
                    || cp.Contains(c + "01")
                    || cp.Contains(c + "06"))
                {
                    return "Eval_50up_" + rpstr;
                }
                else if (cp.Contains(c + "07")
                    || cp.Contains(c + "10")
                    || cp.Contains(c + "09")
                    || cp.Contains(c + "03"))
                {
                    return "Eval_COB_" + rpstr;
                }
            }

            return "";
        }


        public void Usage()
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

        public string ForTest()
        {
            return "Congratulate! You have linked to the dll sucessfully!";
        }

        public WXWATLogic WATPassFail100(string coupongroup1, string CurrentStepName)
        {
            var ret = new WXWATLogic();

            var cfg = WXCfg.GetSysCfg();
            var sharedatatoallen = true;
            if (cfg.ContainsKey("SHARETOALLEN") && cfg["SHARETOALLEN"].Contains("FALSE"))
            { sharedatatoallen = false; }

            if (!string.IsNullOrEmpty(AnalyzeParam))
            { sharedatatoallen = false; }

            var CouponGroup = GetCouponGroup(coupongroup1.ToUpper());
            if (string.IsNullOrEmpty(CouponGroup))
            {
                ret.AppErrorMsg = "Get an illege couponid: " + coupongroup1;
                return ret;
            }

            var bitemp = 100;
            if (CouponGroup.Contains("E06") && CouponGroup.Contains("R06"))
            { bitemp = 25; }

            var shippable = 1;

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

            if (watprobeval.Count > 100)
            {
                var unitdict = new Dictionary<string, bool>();
                foreach (var item in watprobeval)
                {
                    if (!unitdict.ContainsKey(item.UnitNum))
                    { unitdict.Add(item.UnitNum, true); }
                }

                var allunitlist = unitdict.Keys.ToList();
                allunitlist.Sort(delegate(string obj1,string obj2) {
                    return UT.O2I(obj1).CompareTo(UT.O2I(obj2));
                });

                if (allunitlist.Count > 100)
                {
                    var wantedunitdict = new Dictionary<string, bool>();
                    var max = allunitlist.Count-1;
                    var ridxdict = new Dictionary<int, bool>();
                    var rad = new Random(DateTime.Now.Millisecond);
                    while (ridxdict.Count < 100)
                    {
                        var r = rad.Next(max);
                        if (r >= 0 && !ridxdict.ContainsKey(r))
                        { ridxdict.Add(r, true); }
                    }

                    foreach (var kv in ridxdict)
                    { wantedunitdict.Add(allunitlist[kv.Key], true); }

                    var newwatprobeval = new List<WXWATProbeTestData>();
                    foreach (var item in watprobeval)
                    {
                        if (wantedunitdict.ContainsKey(item.UnitNum))
                        {
                            newwatprobeval.Add(item);
                        }
                    }
                    watprobeval = newwatprobeval;
                }
            }


            //var waferarray = WATSampleXY.GetArrayFromAllenSherman(containerinfo.wafer);
            //if (!string.IsNullOrEmpty(waferarray) && CouponGroup.Contains("E08") && string.IsNullOrEmpty(AnalyzeParam))
            //{
            //    var couponcount = WXOriginalWATData.GetCurrentRPTestedCoupon(CouponGroup, UT.O2I(RP));

            //    var necessarynum = 0;
            //    var arraynum = UT.O2I(waferarray);
            //    if (arraynum == 1)
            //    { necessarynum = 4; }
            //    else if (arraynum == 4)
            //    { necessarynum = 10; }
            //    else if (arraynum == 12)
            //    { necessarynum = 30; }

            //    if (couponcount < necessarynum)
            //    {
            //        ret.AppErrorMsg = "For 1x" + waferarray + " wafer, the necessary coupon count of E08 WAT logic check should be great than " + necessarynum.ToString();
            //        return ret;
            //    }
            //}


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
            var failmodestr = WXWATFailureMode.GetFailModeString(failmodes);

            if ((AnalyzeParam.Contains("_AD_")
                || AnalyzeParam.Contains("_DB_")
                || AnalyzeParam.Contains("_RD_"))
                && !AnalyzeParam.Contains("_CPK_"))
            {
                CollectAnalyzeDeltaValue(watprobevalfiltered);
                return ret;
            }

            //Coupon Stat Data
            var binpndict = WXSpecBinPassFail.RetrieveBinDict(containerinfo.ProductName, allspec);
            var couponstatdata = WXWATCouponStats.GetCouponData(watprobevalfiltered, binpndict);

            if ((AnalyzeParam.Contains("_MDD_")
                || AnalyzeParam.Contains("_MXDP_")))
            {
                CollectAnalyzePOLDSummary(couponstatdata);
                return ret;
            }

            //CPK
            var cpkspec = WXSpecBinPassFail.GetCPKSpec(containerinfo.ProductName, DCDName, allspec);
            var cpktab = WXWATCPK.GetCPK(RP, couponstatdata, watprobevalfiltered, cpkspec);

            if (AnalyzeParam.Contains("_CPK_"))
            {
                CollectAnalyzeCPKValue(cpktab);
                return ret;
            }

            //TTF
            var fitspec = WXSpecBinPassFail.GetFitSpec(containerinfo.ProductName, DCDName, allspec);
            var ttfdata = WXWATTTF.GetTTFData(containerinfo.ProductName, UT.O2I(RP), fitspec, watprobevalfiltered, failmodes);

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
            var ttfunitdata = WXWATTTFUnit.GetUnitData(watprobevalfiltered, RP, ttfuse, ttfdatasorted, ttfunitspec);

            var ttfmu = WXWATTTFmu.GetmuData(fitspec, RP, watprobevalfiltered, ttfuse, ttffit);


            //Pass Fail Unit
            var passfailunitspec = WXSpecBinPassFail.GetPassFailUnitSpec(containerinfo.ProductName, DCDName, allspec);
            var passfailunitdata = WXWATPassFailUnit.GetPFUnitData(RP, DCDName, passfailunitspec
                , watprobevalfiltered, couponstatdata, cpktab, ttfmu, ttfunitdata);

            if (AnalyzeParam.Contains("_C-P"))
            {
                CollectAnalyzeC_PValue(passfailunitdata);
                return ret;
            }

            var failcount = WXWATPassFailUnit.GetFailCount(passfailunitdata);
            var failunitinfo = WXWATPassFailUnit.GetFailUnitWithInfo(passfailunitdata);

            //Pass Fail Coupon
            var watpassfailcoupondata = WXWATPassFailCoupon.GetPFCouponData(passfailunitdata, dutminitem[0],false,null,null,null);

            var failstring = WXWATPassFailCoupon.GetFailString(watpassfailcoupondata);
            var couponDutCount = WXWATPassFailCoupon.GetDutCount(watpassfailcoupondata);
            var couponSumFails = WXWATPassFailCoupon.GetSumFails(watpassfailcoupondata);

            var BIYield = 100.0;
            var BIYieldSpec = 0;

            var logicresult = RetestLogic(containerinfo, DCDName, UT.O2I(RP), shippable, probecount, readcount
               , dutminitem[0].minDUT, failcount, failstring, watpassfailcoupondata.Count(), couponDutCount, couponSumFails,BIYield,BIYieldSpec);

            if (!string.IsNullOrEmpty(AnalyzeParam))
            { logicresult.AnalyzeParamData.AddRange(AnalyzeParamData); }

            var scrapspec = WXSpecBinPassFail.GetScrapSpec(containerinfo.ProductName, DCDName, allspec);
            logicresult.ScrapIt = ScrapLogic(containerinfo, scrapspec, UT.O2I(RP), readcount, failcount, bitemp, failmodes);
            if (logicresult.ScrapIt)
            {
                logicresult.ResultReason = failmodestr;
                logicresult.ValueCollect.Add("Scrap ?", "YES");
            }
            else
            {
                logicresult.ValueCollect.Add("Scrap ?", "NO");
                if (logicresult.NeedRetest)
                {
                    logicresult.ValueCollect.Add("ReTest ?", "YES");
                }
                else
                {
                    logicresult.ValueCollect.Add("ReTest ?", "NO");
                }
            }

            logicresult.ValueCollect.Add("fail unit info", failunitinfo);
            logicresult.ValueCollect.Add("failcount", failcount.ToString());
            logicresult.ValueCollect.Add("fail coupon string", failstring);
            logicresult.ValueCollect.Add("fail mode", failmodestr);
            logicresult.ValueCollect.Add("ProbeCount", probecount.ToString());
            logicresult.ValueCollect.Add("readcount", readcount.ToString());

            logicresult.DataTables.Add(watpassfailcoupondata);
            logicresult.DataTables.Add(failmodes);

            if (string.IsNullOrEmpty(logicresult.AppErrorMsg)
                && logicresult.TestPass
                && (CouponGroup.Contains("E08") || CouponGroup.Contains("E01"))
                && string.Compare(CurrentStepName.Replace(" ", "").ToUpper(), "POSTHTOL2JUDGEMENT") == 0
                && string.IsNullOrEmpty(AnalyzeParam)
                && AllowToMoveMapFile)
            {
                try
                {
                    using (NativeMethods cv = new NativeMethods("brad.qiu", "china", cfg["SHAREFOLDERPWD"]))
                    {
                        MoveOriginalMapFile(containerinfo.wafer, cfg["DIESORTFOLDER"], cfg["DIESORT100PCT"]);
                    }
                }
                catch (Exception ex) { }
            }
            return logicresult;
        }

        public WXWATLogic()
        {
            TestPass = false;
            NeedRetest = false;
            ScrapIt = false;
            ResultReason = "";
            AppErrorMsg = "";

            ValueCollect = new Dictionary<string, string>();
            DataTables = new List<object>();

            AnalyzeParam = "";
            AnalyzeParamData = new List<XYVAL>();
            AllowToMoveMapFile = true;
        }

        public bool TestPass { set; get; } //test pass
        public bool NeedRetest { set; get; } //whether need to retest
        public bool ScrapIt { set; get; } //whether scrap
        public string ResultReason { set; get; } //retest/scrap reason
        public string AppErrorMsg { set; get; } //for app logic error

        public Dictionary<string, string> ValueCollect { set; get; }
        public List<object> DataTables { set; get; }

        public string AnalyzeParam { set; get; }
        public List<XYVAL> AnalyzeParamData { set; get; }
        public bool AllowToMoveMapFile { set; get; }
    }
}

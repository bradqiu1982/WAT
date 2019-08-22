using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Windows;

namespace WAT.Models
{
    public class AllenWATLogic
    {

        public static AllenWATLogic PassFail(string containername,string dcdname_,bool noexclusion,bool storeres=false)
        {
            var ret = new AllenWATLogic();

            var dcdname = dcdname_.Replace("_dp", "_rp").Replace("_BurnInTest","");
            var rp = Convert.ToInt32(dcdname.Split(new string[] { "_rp" }, StringSplitOptions.RemoveEmptyEntries)[1]);

            //Container Info
            var containerinfo = ContainerInfo.GetInfo(containername);
            if (string.IsNullOrEmpty(containerinfo.containername))
            {
                //System.Windows.MessageBox.Show("Fail to get container info.....");
                ret.AppErrorMsg = "Fail to get container info.....";
                return ret;
            }

            //BITemp
            var bitemp = WATBITemp.GetBITemp(containerinfo.ProductName, rp.ToString());

            //SPEC
            var allspec = SpecBinPassFail.GetAllSpec();
            var dutminitem = SpecBinPassFail.GetMinDUT(containerinfo.ProductName, dcdname, allspec);
            if (dutminitem.Count == 0)
            {
                //System.Windows.MessageBox.Show("Fail to get min DUT count.....");
                ret.AppErrorMsg = "Fail to get min DUT count.....";
                return ret;
            }

            var shippable = WaferShippable.WFShippable(containerinfo.wafer);

            //PROBE COUNT
            var probecount = ProbeDataQty.GetCount(containerinfo.containername);

            //WAT PROB
            var watprobeval = WATProbeTestData.GetData(containerinfo.containername,rp);
            if (watprobeval.Count == 0)
            {
                //System.Windows.MessageBox.Show("Fail to get wat prob data.....");
                ret.AppErrorMsg = "Fail to get wat prob data.....";
                return ret;
            }
            var readcount = WATProbeTestData.GetReadCount(watprobeval, rp.ToString());

            //sample XY
            var samplexy = SampleCoordinate.GetCoordinate(containerinfo.containername, watprobeval);
            var unitdict = SampleCoordinate.GetNonExclusionUnitDict(samplexy, noexclusion);
            ret.ExclusionInfo = SampleCoordinate.GetExclusionComment(samplexy);


            //filter bin
            var filterbindict = WATACFilterBin.GetFilterBin(containerinfo.ProductName);

            //WAT PROB FILTER
            var watprobevalfiltered = WATProbeTestDataFiltered.GetFilteredData(watprobeval, rp.ToString(), unitdict, filterbindict);
            if (watprobevalfiltered.Count == 0)
            {
                //System.Windows.MessageBox.Show("Fail to get wat prob filtered data.....");
                ret.AppErrorMsg = "Fail to get wat prob filtered data.....";
                return ret;
            }

            //FAIL MODE
            var spec4fmode = SpecBinPassFail.GetParam4FailMode(containerinfo.ProductName, rp.ToString(), allspec);
            var failmodes = WATProbeTestDataFiltered.GetWATFailureModes(watprobevalfiltered, spec4fmode, bitemp);

            //Coupon Stat Data
            var binpndict = SpecBinPassFail.RetrieveBinDict(containerinfo.ProductName, allspec);
            var couponstatdata = WATCouponStats.GetCouponData(watprobevalfiltered, binpndict);
            //if (couponstatdata.Count == 0)
            //{
            //    //System.Windows.MessageBox.Show("Fail to get wat coupon data.....");
            //    ret.AppErrorMsg = "FFail to get wat coupon stat data.....";
            //    return ret;
            //}

            //CPK
            var cpkspec = SpecBinPassFail.GetCPKSpec(containerinfo.ProductName, dcdname, allspec);
            var cpktab = WATCPK.GetCPK(rp.ToString(), couponstatdata, watprobevalfiltered, cpkspec);

            //TTF
            var fitspec = SpecBinPassFail.GetFitSpec(containerinfo.ProductName, dcdname, allspec);
            var ttfdata = WATTTF.GetTTFData(containerinfo.ProductName, rp, fitspec, watprobevalfiltered,failmodes);

            //TTFSorted
            var ttfdatasorted = WATTTFSorted.GetSortedTTFData(ttfdata);

            //TTFTerms
            var ttftermdata = WATTTFTerms.GetTTRTermsData(ttfdatasorted);

            //TTFfit
            var ttffit = WATTTFfit.GetFitData(ttftermdata);

            //TTFuse
            var ttpspec = SpecBinPassFail.GetFitTTPSpec(containerinfo.ProductName, allspec);
            var ttfuse = WATTTFuse.GetTTFuseData(ttpspec, ttffit);

            //TTFUnit
            var ttfunitspec = SpecBinPassFail.GetTTFUnitSpec(containerinfo.ProductName, allspec);
            var ttfunitdata = WATTTFUnit.GetUnitData(watprobevalfiltered, rp.ToString(), ttfuse, ttfdatasorted,ttfunitspec);

            var ttfmu = WATTTFmu.GetmuData(fitspec, rp.ToString(), watprobevalfiltered, ttfuse, ttffit);
            //Pass Fail Unit
            var passfailunitspec = SpecBinPassFail.GetPassFailUnitSpec(containerinfo.ProductName, dcdname, allspec);
            var passfailunitdata = WATPassFailUnit.GetPFUnitData(rp.ToString(), dcdname, passfailunitspec
                , watprobevalfiltered, couponstatdata,cpktab,ttfmu,ttfunitdata);

            //if (passfailunitdata.Count == 0)
            //{
            //    System.Windows.MessageBox.Show("Fail to get wat passfailunit data .....");
            //    ret.ProgramMsg = "Fail to get wat passfailunit data .....";
            //    return ret;
            //}

            var failcount = WATPassFailUnit.GetFailCount(passfailunitdata);
            var failunit = WATPassFailUnit.GetFailUnit(passfailunitdata);
            var failunittab = WATPassFailUnit.GetFailUnitWithFailure(passfailunitdata);

            //Pass Fail Coupon
            var watpassfailcoupondata = WATPassFailCoupon.GetPFCouponData(passfailunitdata, dutminitem[0]);
            //if (watpassfailcoupondata.Count == 0)
            //{
            //    System.Windows.MessageBox.Show("Fail to get wat passfailcoupon data .....");
            //    ret.ProgramMsg = "Fail to get wat passfailcoupon data .....";
            //    return ret;
            //}

            var failstring = WATPassFailCoupon.GetFailString(watpassfailcoupondata);

            var couponDutCount = WATPassFailCoupon.GetDutCount(watpassfailcoupondata);
            var couponSumFails = WATPassFailCoupon.GetSumFails(watpassfailcoupondata);

            //all unit exclusion
            var allunitexclusion = false;
            if (samplexy.Count > 0 && unitdict.Count == 0)
            { allunitexclusion = true; }


            ret.ValueCollect.Add("failunit",failunit);
            ret.ValueCollect.Add("failcount", failcount.ToString());
            ret.ValueCollect.Add("failstring", failstring);
            ret.ValueCollect.Add("ProbeCount", probecount.ProbeCount.ToString());
            ret.ValueCollect.Add("readcount", readcount.ToString());


            //retest logic
            var logicresult = RetestLogic(containerinfo, dcdname, rp, shippable, probecount.ProbeCount, readcount
                , dutminitem[0].minDUT, failcount, failstring, watpassfailcoupondata.Count(), couponDutCount, couponSumFails,allunitexclusion);

            var scrapspec = SpecBinPassFail.GetScrapSpec(containerinfo.ProductName, dcdname,allspec);

            logicresult.ScrapIt = ScrapLogic(containerinfo, scrapspec, rp, readcount, failcount, bitemp, failmodes);

            logicresult.ValueCollect = ret.ValueCollect;
            logicresult.ExclusionInfo = ret.ExclusionInfo;
            logicresult.DataTables.Add(watpassfailcoupondata);
            logicresult.DataTables.Add(failmodes);
            logicresult.DataTables.Add(failunittab);
            logicresult.DataTables.Add(watprobeval);

            if (logicresult.ScrapIt)
            {
                var fmstr = "";
                foreach (var fm in failmodes)
                {
                    if (string.Compare(fm.Failure, "WEAROUT", true) == 0
                    || string.Compare(fm.Failure, "DELAM", true) == 0
                    || string.Compare(fm.Failure, "DVF", true) == 0
                    || string.Compare(fm.Failure, "LOWPOWERLOWLEAKAGE", true) == 0)
                    {
                        fmstr += fm.UnitNum+":"+fm.Failure+"//";
                    }
                }

                logicresult.ResultMsg = fmstr;
            }

            if (storeres)
            {
                StoreOperateInstruction(containerinfo.containertype, containername, containerinfo.ProductName, dcdname, logicresult.SummaryRes, failunit);
            }

            return logicresult;
        }

        private static AllenWATLogic RetestLogic(ContainerInfo container,string DCDName,int rp,int shippable,int probeCount
            ,int readCount,int dutMinQty,int failcount,string failstring,int couponCount,int couponDutCount,int couponSumFails,bool allunitexclusion)
        {
            var ret = new AllenWATLogic();
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
                            ret.ResultMsg = "Proceed";
                        }
                        else
                        {
                            if (DCDName.ToUpper().Contains("50UP"))
                            {
                                if (readCount < 4 && failcount <= 10 && failstring.ToUpper().Contains("C-P"))
                                {
                                    ret.TestPass = false;
                                    ret.NeedRetest = true;
                                    ret.ResultMsg = "Inspect, Check XY coord and fix if necessary, Check orientation and Reseat failing units and retest.";
                                }
                                else if (readCount == 1 && failcount <= 10 && failstring.ToUpper().Contains("VF_LD_V_AD_REF"))
                                {
                                    ret.TestPass = false;
                                    ret.NeedRetest = true;
                                    ret.ResultMsg = "Reseat failing units and retest";
                                }
                                else if (readCount == 1 && failcount <= 10 && failstring.ToUpper().Contains("RD_REF"))
                                {
                                    ret.TestPass = false;
                                    ret.NeedRetest = true;
                                    ret.ResultMsg = "Inspect, ReInsert board and retest";
                                }
                                else if (readCount == 1 && failcount <= 10 && failstring.ToUpper().Contains("AD_REF"))
                                {
                                    ret.TestPass = false;
                                    ret.NeedRetest = true;
                                    ret.ResultMsg = "Inspect, ReInsert board and retest";
                                }
                                else if (readCount == 1 && failcount <= 10 && failstring.ToUpper().Contains("DB_REF"))
                                {
                                    ret.TestPass = false;
                                    ret.NeedRetest = true;
                                    ret.ResultMsg = "Inspect, ReInsert board and retest";
                                }
                                else
                                {
                                    ret.TestPass = false;
                                    ret.NeedRetest = false;
                                    ret.ResultMsg = "Fails. Submit and check that the container goes on hold";
                                }
                            }
                            else if (DCDName.ToUpper().Contains("COB"))
                            {
                                if (readCount == 1 && failcount <= 10 && failstring.ToUpper().Contains("C-P"))
                                {
                                    ret.TestPass = false;
                                    ret.NeedRetest = true;
                                    ret.ResultMsg = "Inspect and retest";
                                }
                                else if (readCount == 1 && failstring.ToUpper().Contains("VF_RP"))
                                {
                                    ret.TestPass = false;
                                    ret.NeedRetest = true;
                                    ret.ResultMsg = "Inspect, ReInsert board and retest";
                                }
                                else if (readCount == 1 && failcount <= 10 && failstring.ToUpper().Contains("RD_REF"))
                                {
                                    ret.TestPass = false;
                                    ret.NeedRetest = true;
                                    ret.ResultMsg = "Inspect, ReInsert board and retest";
                                }
                                else if (readCount == 1 && failcount <= 10 && failstring.ToUpper().Contains("AD_REF"))
                                {
                                    ret.TestPass = false;
                                    ret.NeedRetest = true;
                                    ret.ResultMsg = "Inspect, ReInsert board and retest";
                                }
                                else if (readCount == 1 && failcount <= 10 && failstring.ToUpper().Contains("DB_REF"))
                                {
                                    ret.TestPass = false;
                                    ret.NeedRetest = true;
                                    ret.ResultMsg = "Inspect, ReInsert board and retest";
                                }
                                else
                                {
                                    ret.TestPass = false;
                                    ret.NeedRetest = false;
                                    ret.ResultMsg = "Fails. Submit and check that the container goes on hold";
                                }
                            }
                            else if (DCDName.ToUpper().Contains("TO_RP"))
                            {
                                if (readCount == 1 && failcount <= 10 && failstring.ToUpper().Contains("C-P"))
                                {
                                    ret.TestPass = false;
                                    ret.NeedRetest = true;
                                    ret.ResultMsg = "Inspect and retest";
                                }
                                else if (readCount < 2 && failstring.ToUpper().Contains("C-P") && (couponDutCount-failcount) >= dutMinQty)
                                {
                                    ret.TestPass = false;
                                    ret.NeedRetest = false;
                                    ret.ResultMsg = "Exclude failing parts and redo data collection";
                                }
                                else if (readCount == 1 && failcount <= 10 && failstring.ToUpper().Contains("YIELD"))
                                {
                                    ret.TestPass = false;
                                    ret.NeedRetest = true;
                                    ret.ResultMsg = "Inspect, retest";
                                }
                                else
                                {
                                    ret.TestPass = false;
                                    ret.NeedRetest = false;
                                    ret.ResultMsg = "Fails. Submit and check that the container goes on hold";
                                }
                            }
                            else if (DCDName.ToUpper().Contains("OSA_RP"))
                            {
                                if (readCount == 1 && failcount <= 10 && failstring.ToUpper().Contains("C-P"))
                                {
                                    ret.TestPass = false;
                                    ret.NeedRetest = true;
                                    ret.ResultMsg = "Inspect, Check orientation and Reseat failing units and retest";
                                }
                                else if (readCount == 1 && failstring.ToUpper().Contains("PO_PCT_YIELD"))
                                {
                                    ret.TestPass = false;
                                    ret.NeedRetest = true;
                                    ret.ResultMsg = "Clean fiber, Blow out parts, Retest entire job";
                                }
                                else if (readCount == 1 && failcount <= 10 && failstring.ToUpper().Contains("YIELD"))
                                {
                                    ret.TestPass = false;
                                    ret.NeedRetest = true;
                                    ret.ResultMsg = "Reseat failing units and retest";
                                }
                                else
                                {
                                    ret.TestPass = false;
                                    ret.NeedRetest = false;
                                    ret.ResultMsg = "Fails. Submit and check that the container goes on hold";
                                }
                            }
                            else if (DCDName.ToUpper().Contains("SBW_RP"))
                            {
                                if (readCount == 1 && failcount <= 10 && failstring.ToUpper().Contains("C-P"))
                                {
                                    ret.TestPass = false;
                                    ret.NeedRetest = true;
                                    ret.ResultMsg = "Inspect, Check orientation and Reseat failing units and retest";
                                }
                                else if (readCount == 1 && failcount <= 10 && failstring.ToUpper().Contains("YIELD"))
                                {
                                    ret.TestPass = false;
                                    ret.NeedRetest = true;
                                    ret.ResultMsg = "Reseat failing units and retest";
                                }
                                else
                                {
                                    ret.TestPass = false;
                                    ret.NeedRetest = false;
                                    ret.ResultMsg = "Fails. Submit and check that the container goes on hold";
                                }
                            }
                            else if (DCDName.ToUpper().Contains("LARGE_SIGNAL_RP"))
                            {
                                if (readCount == 1 && failcount <= 10 && failstring.ToUpper().Contains("C-P"))
                                {
                                    ret.TestPass = false;
                                    ret.NeedRetest = true;
                                    ret.ResultMsg = "Inspect, Check orientation and Reseat failing units and retest";
                                }
                                else if (readCount < 2 && failstring.ToUpper().Contains("CPK"))
                                {
                                    ret.TestPass = false;
                                    ret.NeedRetest = false;
                                    ret.ResultMsg = "Clean fiber, Blow out parts, Retest entire job";
                                }
                                else if (readCount == 1 && failcount <= 10 && failstring.ToUpper().Contains("YIELD"))
                                {
                                    ret.TestPass = false;
                                    ret.NeedRetest = true;
                                    ret.ResultMsg = "Reseat failing units and retest";
                                }
                                else
                                {
                                    ret.TestPass = false;
                                    ret.NeedRetest = false;
                                    ret.ResultMsg = "Fails. Submit and check that the container goes on hold";
                                }
                            }
                            else
                            {
                                ret.TestPass = false;
                                ret.NeedRetest = false;
                                ret.ResultMsg = "Fails. Submit and check that the container goes on hold";

                            }
                        }
                    }
                    else
                    {
                        if (readCount == 1 && probeCount < dutMinQty)
                        {
                            ret.TestPass = false;
                            ret.NeedRetest = true;
                            ret.ResultMsg = "Not enough DUTs have xy coordinates recorded or not enough DUTs have probe data recorded.  Make sure container goes on hold and contact engineering.";
                        }
                        else if (readCount == 1)
                        {
                            ret.TestPass = false;
                            ret.NeedRetest = true;
                            ret.ResultMsg = "Not enough DUTs measured. Retest container ensuring container is fully inserted into connector. Redo Check Test Data.";
                        }
                        else if (readCount > 1
                            && string.Compare(DCDName, "Eval_50up_rp00", true) == 0
                            && container.containername.ToUpper().Contains("R"))
                        {
                            ret.TestPass = false;
                            ret.NeedRetest = true;
                            ret.ResultMsg = "Scrap and repick container. Notify Test Engineering that container had too many missing DUTs.";
                        }
                        else
                        {
                            ret.TestPass = false;
                            ret.NeedRetest = false;
                            ret.ResultMsg = "Fails. Submit and check that the container goes on hold 88.";
                        }
                    }
                }
                else
                {
                    ret.TestPass = false;
                    ret.NeedRetest = false;
                    ret.ResultMsg = "Not Production WAT.  Proceed to next step (note that in some cases the next step may be an ENG HOLD step)";
                }
            }
            else if(allunitexclusion)
            {
                ret.TestPass = false;
                ret.NeedRetest = false;
                ret.ResultMsg = "All units now excluded. Hold for engineering.";
            }
            else
            {
                if (string.Compare(container.containertype, "T", true) == 0
                    || string.Compare(container.containertype, "E", true) == 0
                    || string.Compare(container.containertype, "D", true) == 0)
                {
                    ret.TestPass = false;
                    ret.NeedRetest = true;
                    ret.ResultMsg = "Data missing or product or specs not set up or WAT Engineering mode used.  Proceed per Engineering instruction.";
                }
                else
                {
                    ret.TestPass = false;
                    ret.NeedRetest = true;
                    ret.ResultMsg = "Retest Required! There is no data associated with this read point. Check that the correct containername, test program and read point were used.";
                }
            }

            return ret;
        }


        private static bool ScrapLogic(ContainerInfo containerinfo, List<SpecBinPassFail> scrapspec
            ,int rp,int readcount,int failcount,Double bitemp,List<WATFailureMode> failmodes)
        {
            var requiredvehicle = RequiredVehicles.GetData(containerinfo.ProductName);
            var samplevehicle = SampleVehicles.GetData(requiredvehicle, containerinfo.containername);
            var requiredcontainer = RequiredContainers.GetContainers(requiredvehicle, containerinfo.containername, samplevehicle);

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

            var brequiredcontainer = false;
            foreach (var item in requiredcontainer)
            {
                if (string.Compare(containerinfo.containername, item.ContainerName, true) == 0)
                {
                    brequiredcontainer = true;
                    break;
                }
            }

            if (rp > 0 && readcount > 1 && failcount > 0 && bitemp >= 0
                && (string.Compare(containerinfo.lottype, "n", true) == 0
                || string.Compare(containerinfo.lottype, "q", true) == 0
                || string.Compare(containerinfo.lottype, "w", true) == 0
                || string.Compare(containerinfo.lottype, "r", true) == 0)
                && bfailmode && brequiredcontainer && scrapspec.Count > 0
                && (containerinfo.containername.ToUpper().Contains("E01")
                || containerinfo.containername.ToUpper().Contains("E06"))
                    )
            {
                return true;
            }

            return false;
        }

        public AllenWATLogic()
        {
            NeedRetest = false;
            TestPass = false;
            ScrapIt = false;
            ResultMsg = "";
            AppErrorMsg = "";

            ValueCollect = new Dictionary<string, string>();
            DataTables = new List<object>();
        }


        //public static AllenWATLogic ForVerify(string containername, string dcdname_, bool noexclusion = false)
        //{
        //    var ret = new AllenWATLogic();

        //    var dcdname = dcdname_.Replace("_dp", "_rp").Replace("_BurnInTest", "");
        //    var rp = Convert.ToInt32(dcdname.Split(new string[] { "_rp" }, StringSplitOptions.RemoveEmptyEntries)[1]);

        //    //Container Info
        //    var containerinfo = ContainerInfo.GetInfo(containername);
        //    if (string.IsNullOrEmpty(containerinfo.containername))
        //    {
        //        //System.Windows.MessageBox.Show("Fail to get container info.....");
        //        ret.AppErrorMsg = "Fail to get container info.....";
        //        return ret;
        //    }

        //    //BITemp
        //    var bitemp = WATBITemp.GetBITemp(containerinfo.ProductName, rp.ToString());

        //    //SPEC
        //    var allspec = SpecBinPassFail.GetAllSpec();
        //    var dutminitem = SpecBinPassFail.GetMinDUT(containerinfo.ProductName, dcdname, allspec);
        //    if (dutminitem.Count == 0)
        //    {
        //        //System.Windows.MessageBox.Show("Fail to get min DUT count.....");
        //        ret.AppErrorMsg = "Fail to get min DUT count.....";
        //        return ret;
        //    }

        //    var shippable = WaferShippable.WFShippable(containerinfo.wafer);

        //    //PROBE COUNT
        //    var probecount = ProbeDataQty.GetCount(containerinfo.containername);

        //    //WAT PROB
        //    var watprobeval = WATProbeTestData.GetData(containerinfo.containername, rp);
        //    if (watprobeval.Count == 0)
        //    {
        //        //System.Windows.MessageBox.Show("Fail to get wat prob data.....");
        //        ret.AppErrorMsg = "Fail to get wat prob data.....";
        //        return ret;
        //    }
        //    var readcount = WATProbeTestData.GetReadCount(watprobeval, rp.ToString());

        //    //sample XY
        //    var samplexy = SampleCoordinate.GetCoordinate(containerinfo.containername, watprobeval);
        //    var unitdict = SampleCoordinate.GetNonExclusionUnitDict(samplexy, noexclusion);
        //    ret.ExclusionInfo = SampleCoordinate.GetExclusionComment(samplexy);


        //    //filter bin
        //    var filterbindict = WATACFilterBin.GetFilterBin(containerinfo.ProductName);

        //    //WAT PROB FILTER
        //    var watprobevalfiltered = WATProbeTestDataFiltered.GetFilteredData(watprobeval, rp.ToString(), unitdict, filterbindict);
        //    if (watprobevalfiltered.Count == 0)
        //    {
        //        //System.Windows.MessageBox.Show("Fail to get wat prob filtered data.....");
        //        ret.AppErrorMsg = "Fail to get wat prob filtered data.....";
        //        return ret;
        //    }

        //    //FAIL MODE
        //    var spec4fmode = SpecBinPassFail.GetParam4FailMode(containerinfo.ProductName, rp.ToString(), allspec);
        //    var failmodes = WATProbeTestDataFiltered.GetWATFailureModes(watprobevalfiltered, spec4fmode, bitemp);

        //    //Coupon Stat Data
        //    var binpndict = SpecBinPassFail.RetrieveBinDict(containerinfo.ProductName, allspec);
        //    var couponstatdata = WATCouponStats.GetCouponData(watprobevalfiltered, binpndict);
        //    //if (couponstatdata.Count == 0)
        //    //{
        //    //    //System.Windows.MessageBox.Show("Fail to get wat coupon data.....");
        //    //    ret.AppErrorMsg = "FFail to get wat coupon stat data.....";
        //    //    return ret;
        //    //}

        //    //CPK
        //    var cpkspec = SpecBinPassFail.GetCPKSpec(containerinfo.ProductName, dcdname, allspec);
        //    var cpktab = WATCPK.GetCPK(rp.ToString(), couponstatdata, watprobevalfiltered, cpkspec);

        //    //TTF
        //    var fitspec = SpecBinPassFail.GetFitSpec(containerinfo.ProductName, dcdname, allspec);
        //    var ttfdata = WATTTF.GetTTFData(containerinfo.ProductName, rp, fitspec, watprobevalfiltered, failmodes);

        //    //TTFSorted
        //    var ttfdatasorted = WATTTFSorted.GetSortedTTFData(ttfdata);

        //    //TTFTerms
        //    var ttftermdata = WATTTFTerms.GetTTRTermsData(ttfdatasorted);

        //    //TTFfit
        //    var ttffit = WATTTFfit.GetFitData(ttftermdata);

        //    //TTFuse
        //    var ttpspec = SpecBinPassFail.GetFitTTPSpec(containerinfo.ProductName, allspec);
        //    var ttfuse = WATTTFuse.GetTTFuseData(ttpspec, ttffit);

        //    //TTFUnit
        //    var ttfunitspec = SpecBinPassFail.GetTTFUnitSpec(containerinfo.ProductName, allspec);
        //    var ttfunitdata = WATTTFUnit.GetUnitData(watprobevalfiltered, rp.ToString(), ttfuse, ttfdatasorted, ttfunitspec);

        //    var ttfmu = WATTTFmu.GetmuData(fitspec, rp.ToString(), watprobevalfiltered, ttfuse, ttffit);
        //    //Pass Fail Unit
        //    var passfailunitspec = SpecBinPassFail.GetPassFailUnitSpec(containerinfo.ProductName, dcdname, allspec);
        //    var passfailunitdata = WATPassFailUnit.GetPFUnitData(rp.ToString(), dcdname, passfailunitspec
        //        , watprobevalfiltered, couponstatdata, cpktab, ttfmu, ttfunitdata);

        //    //if (passfailunitdata.Count == 0)
        //    //{
        //    //    System.Windows.MessageBox.Show("Fail to get wat passfailunit data .....");
        //    //    ret.ProgramMsg = "Fail to get wat passfailunit data .....";
        //    //    return ret;
        //    //}

        //    var failcount = WATPassFailUnit.GetFailCount(passfailunitdata);
        //    var failunit = WATPassFailUnit.GetFailUnit(passfailunitdata);

        //    //Pass Fail Coupon
        //    var watpassfailcoupondata = WATPassFailCoupon.GetPFCouponData(passfailunitdata, dutminitem[0]);
        //    var findisposedata = PassFailCoupon4Comparing.GetData(containername, dcdname, watpassfailcoupondata);
        //    if (findisposedata.Count > 0)
        //    { PassFailCoupon4Comparing.StoreComparingData(containername, dcdname, findisposedata); }

        //    return ret;
        //}

        public static void StoreOperateInstruction(string containertype, string containername, string productname, string dcdname, string instruct, string failunit)
        {
            var dict = new Dictionary<string, string>();
            dict.Add("@containertype", containertype);
            dict.Add("@containername", containername);
            dict.Add("@productname", productname);
            dict.Add("@ParameterSetName", dcdname);
            dict.Add("@OperatorInstruction", instruct);
            dict.Add("@FailingUnits", failunit);

            var sql = @"insert into [WAT].[dbo].[OpInstruct4Comparing4](containertype,containername,productname,ParameterSetName,OperatorInstruction,FailingUnits)  
                            values(@containertype,@containername,@productname,@ParameterSetName,@OperatorInstruction,@FailingUnits)";
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public bool TestPass { set; get; }
        public bool NeedRetest { set; get; }
        public bool ScrapIt { set; get; }
        public string ResultMsg { set; get; }
        public string AppErrorMsg { set; get; }

        public List<SampleCoordinate> ExclusionInfo { set; get; }
        public Dictionary<string, string> ValueCollect { set; get; }
        public List<object> DataTables { set; get; }


        public string SummaryRes
        {
            get
            {
                if (!string.IsNullOrEmpty(AppErrorMsg))
                {
                    return "APPError:" + AppErrorMsg;
                }
                else
                {
                    if (ScrapIt)
                    {
                        return "SCRAP WAFER:" + ResultMsg;
                    }
                    else
                    {
                        if (TestPass)
                        {
                            return ResultMsg;
                        }
                        else
                        {
                            if (NeedRetest)
                            {
                                return ResultMsg;
                            }
                            else
                            {
                                return ResultMsg;
                            }
                        }
                    }
                }
            }
        }




    }
}
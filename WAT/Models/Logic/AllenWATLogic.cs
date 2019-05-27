using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Windows;

namespace WAT.Models
{
    public class AllenWATLogic
    {

        public static AllenWATLogic PassFaile(string containername,string dcdname_)
        {

            var ret = new AllenWATLogic();

            var dcdname = dcdname_.Replace("_dp", "_rp").Replace("_BurnInTest","");
            var rp = Convert.ToInt32(dcdname.Split(new string[] { "_rp" }, StringSplitOptions.RemoveEmptyEntries)[1]);

            //Container Info
            var containerinfo = ContainerInfo.GetInfo(containername);
            if (string.IsNullOrEmpty(containerinfo.containername))
            {
                System.Windows.MessageBox.Show("Fail to get container info.....");
                ret.ProgramMsg = "Fail to get container info.....";
                return ret;
            }

            //BITemp
            var bitemp = WATBITemp.GetBITemp(containerinfo.ProductName, rp.ToString());

            //SPEC
            var allspec = SpecBinPassFail.GetAllSpec();
            var dutminitem = SpecBinPassFail.GetMinDUT(containerinfo.ProductName, dcdname, allspec);
            if (dutminitem.Count == 0)
            {
                System.Windows.MessageBox.Show("Fail to get min DUT count.....");
                ret.ProgramMsg = "Fail to get min DUT count.....";
                return ret;
            }

            var shippable = WaferShippable.WFShippable(containerinfo.wafer);

            //PROBE COUNT
            var probecount = ProbeDataQty.GetCount(containerinfo.containername);

            //WAT PROB
            var watprobeval = WATProbeTestData.GetData(containerinfo.containername,rp);
            if (watprobeval.Count == 0)
            {
                System.Windows.MessageBox.Show("Fail to get wat prob data.....");
                ret.ProgramMsg = "Fail to get wat prob data.....";
                return ret;
            }
            var readcount = WATProbeTestData.GetReadCount(watprobeval, rp.ToString());

            //sample XY
            var samplexy = SampleCoordinate.GetCoordinate(containerinfo.containername, watprobeval);
            var unitdict = SampleCoordinate.GetNonExclusionUnitDict(samplexy);

            //filter bin
            var filterbindict = WATACFilterBin.GetFilterBin(containerinfo.ProductName);

            //WAT PROB FILTER
            var watprobevalfiltered = WATProbeTestDataFiltered.GetFilteredData(watprobeval, rp.ToString(), unitdict, filterbindict);
            if (watprobevalfiltered.Count == 0)
            {
                System.Windows.MessageBox.Show("Fail to get wat prob filtered data.....");
                ret.ProgramMsg = "Fail to get wat prob filtered data.....";
                return ret;
            }

            //FAIL MODE
            var spec4fmode = SpecBinPassFail.GetParam4FailMode(containerinfo.ProductName, rp.ToString(), allspec);
            var failmodes = WATProbeTestDataFiltered.GetWATFailureModes(watprobevalfiltered, spec4fmode, bitemp);

            //Coupon Stat Data
            var binpndict = SpecBinPassFail.RetrieveBinDict(containerinfo.ProductName, allspec);
            var couponstatdata = WATCouponStats.GetCouponData(watprobevalfiltered, binpndict);
            if (couponstatdata.Count == 0)
            {
                System.Windows.MessageBox.Show("Fail to get wat coupon data.....");
                ret.ProgramMsg = "FFail to get wat coupon data.....";
                return ret;
            }

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

            //Pass Fail Unit
            var passfailunitspec = SpecBinPassFail.GetSpecByPNDCDName(containerinfo.ProductName, dcdname, allspec);
            var passfailunitdata = WATPassFailUnit.GetPFUnitData(rp.ToString(), dcdname, passfailunitspec, watprobevalfiltered, couponstatdata,cpktab);

            //if (passfailunitdata.Count == 0)
            //{
            //    System.Windows.MessageBox.Show("Fail to get wat passfailunit data .....");
            //    ret.ProgramMsg = "Fail to get wat passfailunit data .....";
            //    return ret;
            //}

            var failcount = WATPassFailUnit.GetFailCount(passfailunitdata);

           
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

            //retest logic
            var retestlogic = RetestLogic(containerinfo, dcdname, rp, shippable, probecount.ProbeCount, readcount
                , dutminitem[0].minDUT, failcount, failstring, watpassfailcoupondata.Count(), couponDutCount, couponSumFails,allunitexclusion);

            return retestlogic;
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

        public AllenWATLogic()
        {
            NeedRetest = false;
            TestPass = false;
            ResultMsg = "";
            ProgramMsg = "";
        }

        public bool TestPass { set; get; }
        public bool NeedRetest { set; get; }
        public string ResultMsg { set; get; }
        public string ProgramMsg { set; get; }
    }
}
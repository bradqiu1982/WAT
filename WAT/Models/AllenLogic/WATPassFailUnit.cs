﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class WATPassFailUnit : WATProbeTestData
    {
        public static List<WATPassFailUnit> GetPFUnitData(string RP, string DCDName,List<SpecBinPassFail> speclist
            ,List<WATProbeTestDataFiltered> filterdata,List<WATCouponStats> coupondata, List<WATCPK> cpktab
            ,List<WATTTFmu> ttfmu,List<WATTTFUnit> ttfunit)
        {
            var ret = new List<WATPassFailUnit>();

            var RPStr = "_rp"+(100 + Convert.ToInt32(RP)).ToString().Substring(1);

            var specparamdict = new Dictionary<string, List<SpecBinPassFail>>();
            foreach (var spec in speclist)
            {
                if (specparamdict.ContainsKey(spec.ParamName))
                {
                    specparamdict[spec.ParamName].Add(spec);
                }
                else
                {
                    var templist = new List<SpecBinPassFail>();
                    templist.Add(spec);
                    specparamdict.Add(spec.ParamName, templist);
                }
            }

            foreach (var fitem in filterdata)
            {
                var paramname = fitem.CommonTestName + RPStr;
                if (specparamdict.ContainsKey(paramname))
                {
                    foreach (var spec in specparamdict[paramname])
                    {
                        ret.Add(new WATPassFailUnit(fitem, spec, paramname, fitem.TestValue.ToString()));
                    }
                }

                paramname = fitem.CommonTestName + "_rd_ref0" + RPStr;
                if (specparamdict.ContainsKey(paramname))
                {
                    foreach (var spec in specparamdict[paramname])
                    {
                        ret.Add(new WATPassFailUnit(fitem, spec, paramname, fitem.DeltaList[0].ratiodeltaref));
                    }
                }

                paramname = fitem.CommonTestName + "_rd_ref1" + RPStr;
                if (specparamdict.ContainsKey(paramname))
                {
                    foreach (var spec in specparamdict[paramname])
                    {
                        ret.Add(new WATPassFailUnit(fitem, spec, paramname, fitem.DeltaList[1].ratiodeltaref));
                    }
                }

                paramname = fitem.CommonTestName + "_ad_ref0" + RPStr;
                if (specparamdict.ContainsKey(paramname))
                {
                    foreach (var spec in specparamdict[paramname])
                    {
                        ret.Add(new WATPassFailUnit(fitem, spec, paramname, fitem.DeltaList[0].absolutedeltaref));
                    }
                }

                paramname = fitem.CommonTestName + "_ad_ref1" + RPStr;
                if (specparamdict.ContainsKey(paramname))
                {
                    foreach (var spec in specparamdict[paramname])
                    {
                        ret.Add(new WATPassFailUnit(fitem, spec, paramname, fitem.DeltaList[1].absolutedeltaref));
                    }
                }

                paramname = fitem.CommonTestName + "_ad_ref2" + RPStr;
                if (specparamdict.ContainsKey(paramname))
                {
                    foreach (var spec in specparamdict[paramname])
                    {
                        ret.Add(new WATPassFailUnit(fitem, spec, paramname, fitem.DeltaList[2].absolutedeltaref));
                    }
                }

                paramname = fitem.CommonTestName + "_ad_ref3" + RPStr;
                if (specparamdict.ContainsKey(paramname))
                {
                    foreach (var spec in specparamdict[paramname])
                    {
                        ret.Add(new WATPassFailUnit(fitem, spec, paramname, fitem.DeltaList[3].absolutedeltaref));
                    }
                }

                paramname = fitem.CommonTestName + "_dB_ref0" + RPStr;
                if (specparamdict.ContainsKey(paramname))
                {
                    foreach (var spec in specparamdict[paramname])
                    {
                        ret.Add(new WATPassFailUnit(fitem, spec, paramname, fitem.DeltaList[0].dBdeltaref));
                    }
                }

                paramname = fitem.CommonTestName + "_dB_ref1" + RPStr;
                if (specparamdict.ContainsKey(paramname))
                {
                    foreach (var spec in specparamdict[paramname])
                    {
                        ret.Add(new WATPassFailUnit(fitem, spec, paramname, fitem.DeltaList[1].dBdeltaref));
                    }
                }

                paramname = fitem.CommonTestName + "_c-p" + RPStr;
                if (specparamdict.ContainsKey(paramname))
                {
                    foreach (var spec in specparamdict[paramname])
                    {
                        ret.Add(new WATPassFailUnit(fitem, spec, paramname, fitem.couponminusprobe));
                    }
                }

                paramname = fitem.CommonTestName + "_c-pPCT" + RPStr;
                if (specparamdict.ContainsKey(paramname))
                {
                    foreach (var spec in specparamdict[paramname])
                    {
                        ret.Add(new WATPassFailUnit(fitem, spec, paramname, fitem.couponminusprobePCT));
                    }
                }
            }//end foreach

            foreach (var citem in coupondata)
            {
                foreach (var ckv in citem.CPValDict)
                {
                    var paramname = ckv.Key + RPStr;
                    if (specparamdict.ContainsKey(paramname))
                    {
                        foreach (var spec in specparamdict[paramname])
                        {
                            ret.Add(new WATPassFailUnit(citem, spec, paramname, ckv.Value));
                        }
                    }
                }//end foreach
            }

            foreach (var ck in cpktab)
            {
                var paramname = ck.CommonTestName + RPStr;
                if (specparamdict.ContainsKey(paramname))
                {
                    foreach (var spec in specparamdict[paramname])
                    {
                        if (string.Compare(spec.Bin_PN, ck.Bin_PN, true) == 0)
                        {
                            ret.Add(new WATPassFailUnit(ck, paramname, ck.TestValue));
                        }//end if
                    }//end foreach
                }//end if
            }//end foreach

            foreach (var mu in ttfmu)
            {
                var paramname = mu.CommonTestName + RPStr;
                if (specparamdict.ContainsKey(paramname))
                {
                    foreach (var spec in specparamdict[paramname])
                    {
                        if (string.Compare(spec.Bin_PN, mu.Bin_PN, true) == 0)
                        {
                            ret.Add(new WATPassFailUnit(mu,spec,paramname,mu.TestValue.ToString()));
                        }//end if
                    }//end foreach
                }//end if
            }//end foreach


            foreach (var ut in ttfunit)
            {
                var paramname = ut.CommonTestName + RPStr;
                if (specparamdict.ContainsKey(paramname))
                {
                    foreach (var spec in specparamdict[paramname])
                    {
                        if (string.Compare(spec.Bin_PN, ut.Bin_PN, true) == 0)
                        {
                            ret.Add(new WATPassFailUnit(ut, spec, paramname, ut.TestValue.ToString()));
                        }//end if
                    }//end foreach
                }//end if
            }//end foreach

            return ret;
        }

        public static int GetFailCount(List<WATPassFailUnit> srcdata)
        {
            var unitdict = new Dictionary<string, bool>();
            foreach (var item in srcdata)
            {
                if (!string.IsNullOrEmpty(item.FailType))
                {
                    if (!unitdict.ContainsKey(item.UnitNum))
                    { unitdict.Add(item.UnitNum,true); }
                }//end if
            }//end foreach
            return unitdict.Count;
        }

        public static string GetFailUnit(List<WATPassFailUnit> srcdata)
        {
            var unitdict = new Dictionary<string, bool>();
            foreach (var item in srcdata)
            {
                if (!string.IsNullOrEmpty(item.FailType))
                {
                    if (!unitdict.ContainsKey(item.UnitNum))
                    { unitdict.Add(item.UnitNum, true); }
                }//end if
            }//end foreach
            if (unitdict.Count > 0)
            {
                var unitlist = unitdict.Keys.ToList();
                unitlist.Sort(delegate(string obj1,string obj2) {
                    return UT.O2I(obj1).CompareTo(UT.O2I(obj2));
                });
                return "Fails: " + string.Join(",",unitlist);
            }
            else
            { return ""; }
        }

        public static List<WATPassFailUnit> GetFailUnitWithFailure(List<WATPassFailUnit> srcdata)
        {
            var ret = new List<WATPassFailUnit>();
            foreach (var item in srcdata)
            {
                if (!string.IsNullOrEmpty(item.FailType))
                {
                    ret.Add(item);
                }//end if
            }//end foreach
            return ret;
        }

        private WATPassFailUnit(WATProbeTestDataFiltered data, SpecBinPassFail spec,string testname,string val)
        {
            ParamName = spec.ParamName;
            Eval_PN = spec.Eval_PN;
            Bin_PN = spec.Bin_PN;
            DCDName = spec.DCDName;
            UpperLimit = spec.WUL;
            LowLimit = spec.WLL;

            TimeStamp = data.TimeStamp;
            ContainerNum = data.ContainerNum;
            ToolName = data.ToolName;
            RP = data.RP;
            UnitNum = data.UnitNum;
            X = data.X;
            Y = data.Y;

            CommonTestName = testname;
            TVAL = val;
        }

        private WATPassFailUnit(WATCouponStats data, SpecBinPassFail spec, string testname, string val)
        {
            ParamName = spec.ParamName;
            Eval_PN = spec.Eval_PN;
            Bin_PN = spec.Bin_PN;
            DCDName = spec.DCDName;
            UpperLimit = spec.WUL;
            LowLimit = spec.WLL;

            TimeStamp = data.TimeStamp;
            ContainerNum = data.ContainerNum;
            ToolName = data.ToolName;
            RP = data.RP;
            UnitNum = data.UnitNum;
            X = data.X;
            Y = data.Y;

            CommonTestName = testname;
            TVAL = val;
        }

        private WATPassFailUnit(WATCPK data, string testname, string val)
        {
            ParamName = data.ParamName;
            Eval_PN = data.Eval_PN;
            Bin_PN = data.Bin_PN;
            DCDName = data.DCDName;
            UpperLimit = data.WUL;
            LowLimit = data.WLL;

            TimeStamp = data.TimeStamp;
            ContainerNum = data.ContainerNum;
            ToolName = data.ToolName;
            RP = data.RP;
            UnitNum = data.UnitNum;
            X = data.X;
            Y = data.Y;

            CommonTestName = testname;
            TVAL = val;
        }

        private WATPassFailUnit(WATTTFmu data, SpecBinPassFail spec, string testname, string val)
        {
            ParamName = spec.ParamName;
            Eval_PN = spec.Eval_PN;
            Bin_PN = spec.Bin_PN;
            DCDName = spec.DCDName;
            UpperLimit = spec.WUL;
            LowLimit = spec.WLL;

            TimeStamp = data.TimeStamp;
            ContainerNum = data.ContainerNum;
            ToolName = data.ToolName;
            RP = data.RP;
            UnitNum = data.UnitNum;
            X = data.X;
            Y = data.Y;

            CommonTestName = testname;
            TVAL = val;
        }

        private WATPassFailUnit(WATTTFUnit data, SpecBinPassFail spec, string testname, string val)
        {
            ParamName = spec.ParamName;
            Eval_PN = spec.Eval_PN;
            Bin_PN = spec.Bin_PN;
            DCDName = spec.DCDName;
            UpperLimit = spec.WUL;
            LowLimit = spec.WLL;

            TimeStamp = data.TimeStamp;
            ContainerNum = data.ContainerNum;
            ToolName = data.ToolName;
            RP = data.RP;
            UnitNum = data.UnitNum;
            X = data.X;
            Y = data.Y;

            CommonTestName = testname;
            TVAL = val;
        }

        public string ParamName { set; get; }
        public string Eval_PN { set; get; }
        public string Bin_PN { set; get; }
        public string DCDName { set; get; }
        public string UpperLimit { set; get; }
        public string LowLimit { set; get; }
        public string FailType { get {
                if (string.IsNullOrEmpty(TVAL))
                {
                    return ParamName + "(No Meas)";
                }
                else
                {
                    if (!string.IsNullOrEmpty(UpperLimit) || !string.IsNullOrEmpty(LowLimit))
                    {
                        if (!string.IsNullOrEmpty(UpperLimit))
                        {
                            var v = UT.O2D(TVAL);
                            var u = UT.O2D(UpperLimit);
                            if (v > u)
                            { return ParamName; }
                        }

                        if (!string.IsNullOrEmpty(LowLimit))
                        {
                            var v = UT.O2D(TVAL);
                            var l = UT.O2D(LowLimit);
                            if (v < l)
                            { return ParamName; }
                        }

                        return string.Empty;
                    }
                    else
                    { return string.Empty; }
                }
            } }
        public string TVAL { set; get; }

        public List<double> ValList { set; get; }
    }
}
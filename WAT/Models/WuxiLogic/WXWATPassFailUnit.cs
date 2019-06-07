using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class WXWATPassFailUnit: WXWATProbeTestData
    {
        public static List<WXWATPassFailUnit> GetPFUnitData(string RP, string DCDName, List<WXSpecBinPassFail> speclist
            , List<WXWATProbeTestDataFiltered> filterdata, List<WXWATCouponStats> coupondata, List<WXWATCPK> cpktab
            , List<WXWATTTFmu> ttfmu, List<WXWATTTFUnit> ttfunit)
        {
            var ret = new List<WXWATPassFailUnit>();

            var RPStr = "_rp" + (100 + Convert.ToInt32(RP)).ToString().Substring(1);

            var specparamdict = new Dictionary<string, List<WXSpecBinPassFail>>();
            foreach (var spec in speclist)
            {
                if (specparamdict.ContainsKey(spec.ParamName))
                {
                    specparamdict[spec.ParamName].Add(spec);
                }
                else
                {
                    var templist = new List<WXSpecBinPassFail>();
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
                        ret.Add(new WXWATPassFailUnit(fitem, spec, paramname, fitem.TestValue.ToString()));
                    }
                }

                paramname = fitem.CommonTestName + "_rd_ref0" + RPStr;
                if (specparamdict.ContainsKey(paramname))
                {
                    foreach (var spec in specparamdict[paramname])
                    {
                        ret.Add(new WXWATPassFailUnit(fitem, spec, paramname, fitem.DeltaList[0].ratiodeltaref));
                    }
                }

                paramname = fitem.CommonTestName + "_rd_ref1" + RPStr;
                if (specparamdict.ContainsKey(paramname))
                {
                    foreach (var spec in specparamdict[paramname])
                    {
                        ret.Add(new WXWATPassFailUnit(fitem, spec, paramname, fitem.DeltaList[1].ratiodeltaref));
                    }
                }

                paramname = fitem.CommonTestName + "_ad_ref0" + RPStr;
                if (specparamdict.ContainsKey(paramname))
                {
                    foreach (var spec in specparamdict[paramname])
                    {
                        ret.Add(new WXWATPassFailUnit(fitem, spec, paramname, fitem.DeltaList[0].absolutedeltaref));
                    }
                }

                paramname = fitem.CommonTestName + "_ad_ref1" + RPStr;
                if (specparamdict.ContainsKey(paramname))
                {
                    foreach (var spec in specparamdict[paramname])
                    {
                        ret.Add(new WXWATPassFailUnit(fitem, spec, paramname, fitem.DeltaList[1].absolutedeltaref));
                    }
                }

                paramname = fitem.CommonTestName + "_ad_ref2" + RPStr;
                if (specparamdict.ContainsKey(paramname))
                {
                    foreach (var spec in specparamdict[paramname])
                    {
                        ret.Add(new WXWATPassFailUnit(fitem, spec, paramname, fitem.DeltaList[2].absolutedeltaref));
                    }
                }

                paramname = fitem.CommonTestName + "_ad_ref3" + RPStr;
                if (specparamdict.ContainsKey(paramname))
                {
                    foreach (var spec in specparamdict[paramname])
                    {
                        ret.Add(new WXWATPassFailUnit(fitem, spec, paramname, fitem.DeltaList[3].absolutedeltaref));
                    }
                }

                paramname = fitem.CommonTestName + "_dB_ref0" + RPStr;
                if (specparamdict.ContainsKey(paramname))
                {
                    foreach (var spec in specparamdict[paramname])
                    {
                        ret.Add(new WXWATPassFailUnit(fitem, spec, paramname, fitem.DeltaList[0].dBdeltaref));
                    }
                }

                paramname = fitem.CommonTestName + "_dB_ref1" + RPStr;
                if (specparamdict.ContainsKey(paramname))
                {
                    foreach (var spec in specparamdict[paramname])
                    {
                        ret.Add(new WXWATPassFailUnit(fitem, spec, paramname, fitem.DeltaList[1].dBdeltaref));
                    }
                }

                paramname = fitem.CommonTestName + "_c-p" + RPStr;
                if (specparamdict.ContainsKey(paramname))
                {
                    foreach (var spec in specparamdict[paramname])
                    {
                        ret.Add(new WXWATPassFailUnit(fitem, spec, paramname, fitem.couponminusprobe));
                    }
                }

                paramname = fitem.CommonTestName + "_c-pPCT" + RPStr;
                if (specparamdict.ContainsKey(paramname))
                {
                    foreach (var spec in specparamdict[paramname])
                    {
                        ret.Add(new WXWATPassFailUnit(fitem, spec, paramname, fitem.couponminusprobePCT));
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
                            ret.Add(new WXWATPassFailUnit(citem, spec, paramname, ckv.Value));
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
                            ret.Add(new WXWATPassFailUnit(ck, paramname, ck.TestValue));
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
                            ret.Add(new WXWATPassFailUnit(mu, spec, paramname, mu.TestValue.ToString()));
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
                            ret.Add(new WXWATPassFailUnit(ut, spec, paramname, ut.TestValue.ToString()));
                        }//end if
                    }//end foreach
                }//end if
            }//end foreach

            return ret;
        }

        public static int GetFailCount(List<WXWATPassFailUnit> srcdata)
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
            return unitdict.Count;
        }

        public static string GetFailUnit(List<WXWATPassFailUnit> srcdata)
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
                return string.Join(",", unitdict.Keys.ToList());
            }
            else
            { return ""; }
        }

        private WXWATPassFailUnit(WXWATProbeTestDataFiltered data, WXSpecBinPassFail spec, string testname, string val)
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

        private WXWATPassFailUnit(WXWATCouponStats data, WXSpecBinPassFail spec, string testname, string val)
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

        private WXWATPassFailUnit(WXWATCPK data, string testname, string val)
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

        private WXWATPassFailUnit(WXWATTTFmu data, WXSpecBinPassFail spec, string testname, string val)
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

        private WXWATPassFailUnit(WXWATTTFUnit data, WXSpecBinPassFail spec, string testname, string val)
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
        public string FailType
        {
            get
            {
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
            }
        }
        public string TVAL { set; get; }

        public List<double> ValList { set; get; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class WXWATCPK
    {
        public static List<WXWATCPK> GetCPK(string RP, List<WXWATCouponStats> coupondata, List<WXWATProbeTestDataFiltered> filterdata, List<WXSpecBinPassFail> cpkspec)
        {
            var RPStr = "_RP" + (100 + Convert.ToInt32(RP)).ToString().Substring(1);

            var meandict = new Dictionary<string, WXWATCPK>();
            var stddevdict = new Dictionary<string, WXWATCPK>();

            foreach (var citem in coupondata)
            {
                foreach (var binkv in citem.BinPNDict)
                {
                    foreach (var dkv in citem.CPValDict)
                    {
                        var key = dkv.Key + ":" + binkv.Key + ":" + dkv.Value;
                        if (dkv.Key.ToUpper().Contains("MEAN"))
                        {
                            if (!meandict.ContainsKey(key))
                            {
                                var tempvm = new WXWATCPK();
                                tempvm.CommonTestName = dkv.Key;
                                tempvm.Bin_PN = binkv.Key;
                                tempvm.TestValue = dkv.Value;
                                meandict.Add(key, tempvm);
                            }
                        }

                        if (dkv.Key.ToUpper().Contains("STDDEV"))
                        {
                            if (!stddevdict.ContainsKey(key))
                            {
                                var tempvm = new WXWATCPK();
                                tempvm.CommonTestName = dkv.Key;
                                tempvm.Bin_PN = binkv.Key;
                                tempvm.TestValue = dkv.Value;
                                stddevdict.Add(key, tempvm);
                            }
                        }
                    }//end foreach
                }//end foreach
            }//end foreach

            var groupfilterdata = new Dictionary<string, WXWATProbeTestDataFiltered>();
            foreach (var fitem in filterdata)
            {
                var key = fitem.ContainerNum + ":" + fitem.ToolName + ":" + fitem.RP + ":" + fitem.UnitNum + ":" + fitem.X + ":" + fitem.Y;
                if (!groupfilterdata.ContainsKey(key))
                {
                    var tempvm = new WXWATProbeTestDataFiltered();
                    tempvm.TimeStamp = fitem.TimeStamp;
                    tempvm.ContainerNum = fitem.ContainerNum;
                    tempvm.ToolName = fitem.ToolName;
                    tempvm.RP = fitem.RP;
                    tempvm.UnitNum = fitem.UnitNum;
                    tempvm.X = fitem.X;
                    tempvm.Y = fitem.Y;
                    groupfilterdata.Add(key, tempvm);
                }
            }

            var mlist = meandict.Values.ToList();
            var slist = stddevdict.Values.ToList();

            var cpklist = new List<WXWATCPK>();
            foreach (var spec in cpkspec)
            {
                var mval = "";
                foreach (var mitem in mlist)
                {
                    if (string.Compare(spec.Bin_PN, mitem.Bin_PN, true) == 0
                        && string.Compare(mitem.CommonTestName + RPStr, spec.ParamName.ToUpper().Replace("CPK", "MEAN"), true) == 0)
                    {
                        mval = mitem.TestValue;
                    }
                }
                var sval = "";
                foreach (var sitem in slist)
                {
                    if (string.Compare(spec.Bin_PN, sitem.Bin_PN, true) == 0
                        && string.Compare(sitem.CommonTestName + RPStr, spec.ParamName.ToUpper().Replace("CPK", "STDDEV"), true) == 0)
                    {
                        sval = sitem.TestValue;
                    }
                }

                if (!string.IsNullOrEmpty(spec.DTLL)
                    && !string.IsNullOrEmpty(spec.DTUL)
                    && !string.IsNullOrEmpty(mval)
                    && !string.IsNullOrEmpty(sval))
                {
                    var dll = UT.O2D(spec.DTLL);
                    var dul = UT.O2D(spec.DTUL);
                    var mv = UT.O2D(mval);
                    var sv = UT.O2D(sval);

                    var cpkval = 0.0;
                    if (sv != 0)
                    {
                        if ((dul - mv) < (mv - dll))
                        {
                            cpkval = (dul - mv) / 3.0 / sv;
                        }
                        else
                        {
                            cpkval = (mv - dll) / 3.0 / sv;
                        }
                    }
                    else if (mv >= dll && mv <= dul)
                    {
                        cpkval = 9.99;
                    }
                    else
                    {
                        cpkval = 0.0;
                    }

                    var tempvm = new WXWATCPK();
                    tempvm.Bin_PN = spec.Bin_PN;
                    tempvm.TestValue = cpkval.ToString();
                    tempvm.CommonTestName = spec.ParamName.Replace(RPStr.ToLower(), "");

                    tempvm.Eval_PN = spec.Eval_PN;
                    tempvm.ParamName = spec.ParamName;
                    tempvm.DCDName = spec.DCDName;
                    tempvm.WLL = spec.WLL;
                    tempvm.WUL = spec.WUL;

                    cpklist.Add(tempvm);
                }//end if
            }//end foreach

            var gdlist = groupfilterdata.Values.ToList();
            var ret = new List<WXWATCPK>();
            foreach (var fitem in gdlist)
            {
                foreach (var kitem in cpklist)
                {
                    var tempvm = new WXWATCPK();
                    tempvm.TimeStamp = fitem.TimeStamp;
                    tempvm.ContainerNum = fitem.ContainerNum;
                    tempvm.ToolName = fitem.ToolName;
                    tempvm.RP = fitem.RP;
                    tempvm.UnitNum = fitem.UnitNum;
                    tempvm.X = fitem.X;
                    tempvm.Y = fitem.Y;

                    tempvm.CommonTestName = kitem.CommonTestName;
                    tempvm.TestValue = kitem.TestValue;
                    tempvm.Bin_PN = kitem.Bin_PN;
                    tempvm.Eval_PN = kitem.Eval_PN;
                    tempvm.ParamName = kitem.ParamName;
                    tempvm.DCDName = kitem.DCDName;
                    tempvm.WLL = kitem.WLL;
                    tempvm.WUL = kitem.WUL;

                    ret.Add(tempvm);
                }
            }

            return ret;
        }

        public DateTime TimeStamp { set; get; }
        public string ContainerNum { set; get; }
        public string ToolName { set; get; }
        public string RP { set; get; }
        public string UnitNum { set; get; }
        public string X { set; get; }
        public string Y { set; get; }
        public string CommonTestName { set; get; }
        public string TestValue { set; get; }
        public string Bin_PN { set; get; }


        public string ParamName { set; get; }
        public string Eval_PN { set; get; }
        public string DCDName { set; get; }
        public string WUL { set; get; }
        public string WLL { set; get; }
    }
}
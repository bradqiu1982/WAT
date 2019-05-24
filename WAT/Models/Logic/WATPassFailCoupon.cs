using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class WATPassFailCoupon
    {

        public static List<WATPassFailCoupon> GetPFCouponData(List<WATPassFailUnit> srcdatalist,SpecBinPassFail minDutSpec)
        {
            var ret = new List<WATPassFailCoupon>();
            var unitdict = new Dictionary<string, WATPassFailUnit>();
            foreach (var item in srcdatalist)
            {
                var key = item.Bin_PN + ":" + item.ParamName + ":" + item.Eval_PN + ":" 
                    + item.DCDName + ":" + item.UpperLimit + ":" + item.LowLimit+":"+item.CommonTestName;
                if (unitdict.ContainsKey(key))
                {
                    if (!string.IsNullOrEmpty(item.TVAL))
                    { unitdict[key].ValList.Add(UT.O2D(item.TVAL)); }
                }
                else
                {
                    item.ValList = new List<double>();
                    if (!string.IsNullOrEmpty(item.TVAL))
                    { item.ValList.Add(UT.O2D(item.TVAL)); }
                    unitdict.Add(key, item);
                }
            }

            foreach (var kv in unitdict)
            {
                var tempvm = new WATPassFailCoupon();
                tempvm.Bin_PN = kv.Value.Bin_PN;
                tempvm.ParamName = kv.Value.ParamName;
                tempvm.Eval_PN = kv.Value.Eval_PN;
                tempvm.DCDName = kv.Value.DCDName;
                tempvm.UpperLimit = kv.Value.UpperLimit;
                tempvm.LowLimit = kv.Value.LowLimit;
                tempvm.CommonTestName = kv.Value.CommonTestName;
                tempvm.minDut = minDutSpec.minDUT;
                if (kv.Value.ValList.Count > 0)
                {
                    tempvm.MinVal = kv.Value.ValList.Min().ToString();
                    tempvm.MaxVal = kv.Value.ValList.Max().ToString();
                    tempvm.DUTCount = kv.Value.ValList.Count;
                }
                else
                {
                    tempvm.MinVal = "";
                    tempvm.MaxVal = "";
                    tempvm.DUTCount = 0;
                }

                ret.Add(tempvm);
            }

            return ret;
        }

        public static string GetFailString(List<WATPassFailCoupon> srcdata)
        {
            var fdict = new Dictionary<string, bool>();
            foreach (var item in srcdata)
            {
                if (!string.IsNullOrEmpty(item.failtype) && !fdict.ContainsKey(item.failtype))
                {
                    fdict.Add(item.failtype, true);
                }
            }

            if (fdict.Count == 0)
            {
                return "Fails:";
            }
            else
            {
                var keylist = fdict.Keys.ToList();
                return "Fails:"+string.Join(",",keylist);
            }
        }

        public static int GetDutCount(List<WATPassFailCoupon> srcdata)
        {
            if (srcdata.Count == 0)
            { return 0; }

            var dutlist = new List<int>();
            foreach (var item in srcdata)
            {
                dutlist.Add(item.DUTCount);
            }
            return dutlist.Min();
        }

        public static int GetSumFails(List<WATPassFailCoupon> srcdata)
        {
            if (srcdata.Count == 0)
            { return 0; }

            var ret = 0;
            var dutlist = new List<int>();
            foreach (var item in srcdata)
            {
                ret += item.fails;
            }
            return ret;
        }

        public string Bin_PN { set; get; }
        public string ParamName { set; get; }
        public string Eval_PN { set; get; }
        public string DCDName{set;get;}
        public string UpperLimit { set; get; }
        public string LowLimit { set; get; }
        public string CommonTestName { set; get; }
        public string MinVal { set; get; }
        public string MaxVal { set; get; }
        public int DUTCount { set; get; }
        public int minDut { set; get; }
        public int fails { get {
                if (string.IsNullOrEmpty(MinVal) || DUTCount < minDut)
                {
                    return 1;
                }
                else
                {
                    if (!string.IsNullOrEmpty(UpperLimit))
                    {
                        var v = UT.O2D(MaxVal);
                        var u = UT.O2D(UpperLimit);
                        if (v > u)
                        { return 1; }
                    }

                    if (!string.IsNullOrEmpty(LowLimit))
                    {
                        var v = UT.O2D(MinVal);
                        var l = UT.O2D(LowLimit);
                        if (v < l)
                        { return 1; }
                    }
                    return 0;
                }
            } }



        public string failtype { get {
                if (DUTCount >= minDut)
                {
                    if (string.IsNullOrEmpty(MinVal))
                    { return ParamName + "(No Meas)"; }

                    if (!string.IsNullOrEmpty(UpperLimit))
                    {
                        var v = UT.O2D(MaxVal);
                        var u = UT.O2D(UpperLimit);
                        if (v > u)
                        { return ParamName + "(Limits)"; }
                    }

                    if (!string.IsNullOrEmpty(LowLimit))
                    {
                        var v = UT.O2D(MinVal);
                        var l = UT.O2D(LowLimit);
                        if (v < l)
                        { return ParamName + "(Limits)"; }
                    }

                    return "";
                }
                else
                {
                    if (string.IsNullOrEmpty(MinVal))
                    { return ParamName + "(No Meas)"; }

                    if (!string.IsNullOrEmpty(UpperLimit))
                    {
                        var v = UT.O2D(MaxVal);
                        var u = UT.O2D(UpperLimit);
                        if (v > u)
                        { return ParamName + "(Limits TooFew)"; }
                    }

                    if (!string.IsNullOrEmpty(LowLimit))
                    {
                        var v = UT.O2D(MinVal);
                        var l = UT.O2D(LowLimit);
                        if (v < l)
                        { return ParamName + "(Limits TooFew)"; }
                    }

                    return ParamName + "(TooFew)";
                }


            } }

    }
}
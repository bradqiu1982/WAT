using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class WATTTFSorted : WATTTF
    {
        public static List<WATTTFSorted> GetSortedTTFData(List<WATTTF> srcdata)
        {
            var ttfdict = new Dictionary<string, List<WATTTF>>();
            foreach (var item in srcdata)
            {
                if (ttfdict.ContainsKey(item.Bin_PN))
                {
                    ttfdict[item.Bin_PN].Add(item);
                }
                else
                {
                    var templist = new List<WATTTF>();
                    templist.Add(item);
                    ttfdict.Add(item.Bin_PN, templist);
                }
            }//end foreach

            var ret = new List<WATTTFSorted>();
            foreach (var kv in ttfdict)
            {
                var pnttflist = kv.Value.ToList();
                pnttflist.Sort(delegate (WATTTF obj1, WATTTF obj2)
                {
                    return UT.O2D(obj1.TTF_dB_ref1).CompareTo(UT.O2D(obj2.TTF_dB_ref1));
                });

                var idx = 0.0;
                foreach (var item in pnttflist)
                {
                    var cumft = (double)(idx + 0.5) /(double) item.DUT_COUNT;

                    var log0 = "";
                    var log1 = "";
                    if (!string.IsNullOrEmpty(item.TTF_dB_ref0))
                    {
                        if (UT.O2D(item.TTF_dB_ref0) > 0)
                        {
                            log0 = Math.Log(UT.O2D(item.TTF_dB_ref0)).ToString();
                        }
                    }
                    if (!string.IsNullOrEmpty(item.TTF_dB_ref1))
                    {
                        if (UT.O2D(item.TTF_dB_ref1) > 0)
                        {
                            log1 = Math.Log(UT.O2D(item.TTF_dB_ref1)).ToString();
                        }
                    }
                    var z = GetNormalSinV(cumft);
                    ret.Add(new WATTTFSorted(item,cumft,log0,log1,z));

                    idx += 1.0;
                }
            }

            return ret;
        }

        public static double GetNormalSinV(double p)
        {
            var a1 = -39.6968302866538;
            var a2 = 220.946098424521;
            var a3 = -275.928510446969;
            var a4 = 138.357751867269;
            var a5 = -30.6647980661472;
            var a6 = 2.50662827745924;
            var b1 = -54.4760987982241;
            var b2 = 161.585836858041;
            var b3 = -155.698979859887;
            var b4 = 66.8013118877197;
            var b5 = -13.2806815528857;
            var c1 = -7.78489400243029E-03;
            var c2 = -0.322396458041136;
            var c3 = -2.40075827716184;
            var c4 = -2.54973253934373;
            var c5 = 4.37466414146497;
            var c6 = 2.93816398269878;
            var d1 = 7.78469570904146E-03;
            var d2 = 0.32246712907004;
            var d3 = 2.445134137143;
            var d4 = 3.75440866190742;
            var plow = 0.02425;
            var phigh = 1.0 - plow;

            if (p < plow)
            {
                var q = Math.Sqrt(-2 * Math.Log(p));
                var result = (((((c1 * q + c2) * q + c3) * q + c4) * q + c5) * q + c6) / ((((d1 * q + d2) * q + d3) * q + d4) * q + 1);
                return result;
            }
            else
            {
                if (p < phigh)
                {
                    var q = p - 0.5;
                    var r = q * q;
                    var result = (((((a1 * r + a2) * r + a3) * r + a4) * r + a5) * r + a6) * q / (((((b1 * r + b2) * r + b3) * r + b4) * r + b5) * r + 1);
                    return result;
                }
                else
                {
                    var q = Math.Sqrt(-2 * Math.Log(1 - p));
                    var result = -(((((c1 * q + c2) * q + c3) * q + c4) * q + c5) * q + c6) / ((((d1 * q + d2) * q + d3) * q + d4) * q + 1);
                    return result;
                }
            }
        }

        private WATTTFSorted(WATTTF data,double cumft,string log0,string log1,double zz)
        {
            ParamName = data.ParamName;
            Bin_PN = data.Bin_PN;
            DTLL = data.DTLL;
            DTUL = data.DTUL;
            cumulativeBItime = data.cumulativeBItime;

            cumulativeBItime_prev = data.cumulativeBItime_prev;
            UnitNum = data.UnitNum;
            TTF_dB_ref0 = data.TTF_dB_ref0;
            TTF_dB_ref1 = data.TTF_dB_ref1;
            dBdelta_ref0 = data.dBdelta_ref0;

            dBdelta_ref0_prev = data.dBdelta_ref0_prev;
            dBdelta_ref1 = data.dBdelta_ref1;
            dBdelta_ref1_prev = data.dBdelta_ref1_prev;
            CensorType =  data.CensorType;
            CumFailRate = cumft;
            LogTTF_db_ref0 = log0;
            LogTTF_db_ref1 = log1;
            z = zz;
        }

        public double CumFailRate { set; get; }
        public string LogTTF_db_ref0 { set; get; }
        public string LogTTF_db_ref1 { set; get; }
        public double z { set; get; }
    }
}
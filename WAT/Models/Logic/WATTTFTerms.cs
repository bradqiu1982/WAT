using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MathNet.Numerics.Statistics;

namespace WAT.Models
{
    public class WATTTFTerms
    {
        public static List<WATTTFTerms> GetTTRTermsData(List<WATTTFSorted> srcdata)
        {
            var binpndict = new Dictionary<string, List<WATTTFSorted>>();
            var sxlist = new List<double>();
            var sylist = new List<double>();
            var sxxlist = new List<double>();
            var sxylist = new List<double>();
            var syylist = new List<double>();

            foreach (var item in srcdata)
            {
                if (!string.IsNullOrEmpty(item.LogTTF_db_ref1))
                {
                    if (!binpndict.ContainsKey(item.Bin_PN))
                    {
                        var templist = new List<WATTTFSorted>();
                        templist.Add(item);
                        binpndict.Add(item.Bin_PN, templist);
                    }
                    else
                    {
                        binpndict[item.Bin_PN].Add(item);
                    }

                }
            }

            foreach (var kv in binpndict)
            {
                var onelist = kv.Value.ToList();
                foreach (var item in onelist)
                {
                    sxlist.Add(item.z);
                    sylist.Add(UT.O2D(item.LogTTF_db_ref1));
                    sxxlist.Add(Math.Pow(item.z, 2));
                    sxylist.Add(item.z * UT.O2D(item.LogTTF_db_ref1));
                    syylist.Add(Math.Pow(UT.O2D(item.LogTTF_db_ref1), 2));
                }
                break;
            }

            var ret = new List<WATTTFTerms>();
            foreach (var kv in binpndict)
            {
                var tempvm = new WATTTFTerms();
                tempvm.Bin_PN = kv.Key;
                tempvm.N = sxlist.Count;
                tempvm.Sx = sxlist.Sum();
                tempvm.Sy = sylist.Sum();
                tempvm.Sxx = sxxlist.Sum();
                tempvm.Sxy = sxylist.Sum();
                tempvm.Syy = syylist.Sum();
                tempvm.xbar = sxlist.Average();
                tempvm.ybar = sylist.Average();
                tempvm.stddev = Statistics.StandardDeviation(sylist);
                ret.Add(tempvm);
            }
            return ret;
        }

        public string Bin_PN { set; get; }
        public int N { set; get; }
        public double Sx { set; get; }
        public double Sy { set; get; }
        public double Sxx { set; get; }
        public double Sxy { set; get; }
        public double Syy { set; get; }
        public double xbar { set; get; }
        public double ybar { set; get; }
        public double stddev { set; get; }
    }
}
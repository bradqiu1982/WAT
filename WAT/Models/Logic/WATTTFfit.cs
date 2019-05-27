using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class WATTTFfit
    {
        public static List<WATTTFfit> GetFitData(List<WATTTFTerms> srcdata)
        {
            var ret = new List<WATTTFfit>();
            foreach (var item in srcdata)
            {
                var tempvm = new WATTTFfit();
                tempvm.Bin_PN = item.Bin_PN;
                if (item.N > 1)
                {
                    tempvm.sigma = (item.N * item.Sxy - item.Sx * item.Sy) / (item.N * item.Sxx - Math.Pow(item.Sx, 2));
                    tempvm.mu = item.ybar - (item.N * item.Sxy - item.Sx * item.Sy) / (item.N * item.Sxx - Math.Pow(item.Sx, 2)) * item.xbar;
                    tempvm.R2 = Math.Pow((item.N * item.Sxy - item.Sx * item.Sy) / (Math.Sqrt((item.N * item.Sxx - Math.Pow(item.Sx, 2)) * (item.N * item.Syy - Math.Pow(item.Sy, 2)))), 2);
                }
                else
                {
                    tempvm.sigma = 0;
                    tempvm.mu = 0;
                    tempvm.R2 = 1;
                }
                ret.Add(tempvm);
            }
            return ret;
        }

        public string Bin_PN { set; get; }
        public double sigma { set; get; }
        public double mu { set; get; }
        public double R2 { set; get; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class WATTTFuse
    {
        public static List<WATTTFuse> GetTTFuseData( List<SpecBinPassFail> ttpspec,List<WATTTFfit> fitsrcdata)
        {
            var ret = new List<WATTTFuse>();
            foreach (var spec in ttpspec)
            {
                foreach (var fitem in fitsrcdata)
                {
                    if (string.Compare(spec.Bin_PN, fitem.Bin_PN, true) == 0)
                    {
                        var tempvm = new WATTTFuse();
                        tempvm.Bin_PN = spec.Bin_PN;
                        tempvm.Fail_Criteria_Use = UT.O2D(spec.DTLL);
                        tempvm.TTF_percent = UT.O2D(spec.Ref1);
                        tempvm.Temp_Use = UT.O2D(spec.Ref2);
                        tempvm.Bias_Use = UT.O2D(spec.Ref3);
                        tempvm.ArraySize = 1;
                        tempvm.sigma = fitem.sigma;
                        tempvm.AF = 1 / 1E4;
                        var ref4 = UT.O2D(spec.Ref4);

                        if (ref4 != 0)
                        {
                            var z = WATTTFSorted.GetNormalSinV(tempvm.TTF_percent/ref4);
                            tempvm.mu_use = Math.Log(Math.Exp(fitem.mu) / tempvm.AF) - Math.Log((1.0 - Math.Round(Math.Pow(10.000, (-1.0 / 10.000)),3)  ) / (1.0 - Math.Round( Math.Pow(10.000, (tempvm.Fail_Criteria_Use / 10.000)),3) ));
                            tempvm.TTF = Math.Exp(z * tempvm.sigma + Math.Log(Math.Exp(fitem.mu) / tempvm.AF) - Math.Log((1.0 - Math.Round(Math.Pow(10.000, (-1 / 10.000)),3)) / (1.0 - Math.Round(Math.Pow(10.000, (tempvm.Fail_Criteria_Use / 10.000)),3)) ));
                            ret.Add(tempvm);
                        }
                    }//end if
                }//end foreach
            }//end foreach

            return ret;
        }




        public string Bin_PN { set; get; }
        public double Fail_Criteria_Use { set; get; }
        public double TTF_percent { set; get; }
        public double Temp_Use { set; get; }
        public double Bias_Use { set; get; }
        public int ArraySize { set; get; }
        public double sigma { set; get; }
        public double AF { set; get; }
        public double mu_use { set; get; }
        public double TTF { set; get; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class WATTTFmu : WATProbeTestData
    {
        public static List<WATTTFmu> GetmuData(List<SpecBinPassFail> fitspec, string rp, List<WATProbeTestDataFiltered> filterdata, List<WATTTFuse> ttfuse, List<WATTTFfit> ttffit)
        {
            var RPStr = "_rp" + (100 + Convert.ToInt32(rp)).ToString().Substring(1);

            var groupfilterdata = new Dictionary<string, WATProbeTestDataFiltered>();
            foreach (var fitem in filterdata)
            {
                var key = fitem.ContainerNum + ":" + fitem.ToolName + ":" + fitem.RP + ":" + fitem.UnitNum + ":" + fitem.X + ":" + fitem.Y + ":" + fitem.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss");
                if (!groupfilterdata.ContainsKey(key))
                {
                    if (string.Compare(rp, fitem.RP, true) == 0)
                    {
                        var tempvm = new WATProbeTestDataFiltered();
                        tempvm.TimeStamp = fitem.TimeStamp;
                        tempvm.ContainerNum = fitem.ContainerNum;
                        tempvm.ToolName = fitem.ToolName;
                        tempvm.RP = fitem.RP;
                        tempvm.UnitNum = fitem.UnitNum;
                        tempvm.X = fitem.X;
                        tempvm.Y = fitem.Y;
                        groupfilterdata.Add(key, tempvm);
                    }
                }//end if
            }//end foreach

            //use unit num to map grouped filter data
            var groupfilterdatalist = groupfilterdata.Values.ToList();

            var mulist = new List<WATTTFmu>();
            foreach (var item in ttffit)
            {
                var tempvm = new WATTTFmu();
                tempvm.Bin_PN = item.Bin_PN;
                tempvm.CommonTestName = "PO_LD_W_FITmu_ref1";
                tempvm.TestValue = item.mu;
                mulist.Add(tempvm);

                tempvm = new WATTTFmu();
                tempvm.Bin_PN = item.Bin_PN;
                tempvm.CommonTestName = "PO_LD_W_FITsigma_ref1";
                tempvm.TestValue = item.sigma;
                mulist.Add(tempvm);

                tempvm = new WATTTFmu();
                tempvm.Bin_PN = item.Bin_PN;
                tempvm.CommonTestName = "PO_LD_W_FITR2_ref1";
                tempvm.TestValue = item.R2;
                mulist.Add(tempvm);
            }

            foreach (var item in ttfuse)
            {
                var tempvm = new WATTTFmu();
                tempvm.Bin_PN = item.Bin_PN;
                tempvm.CommonTestName = "PO_LD_W_FIT_TTPCTF_ref1";
                tempvm.TestValue = item.TTF;
                mulist.Add(tempvm);
            }

            var ret = new List<WATTTFmu>();
            foreach (var spec in fitspec)
            {
                foreach (var muitem in mulist)
                {
                    if (string.Compare(spec.Bin_PN, muitem.Bin_PN, true) == 0
                        && string.Compare(muitem.CommonTestName + RPStr, spec.ParamName, true) == 0)
                    {
                        foreach (var fitem in groupfilterdatalist)
                        {
                            var tempvm = new WATTTFmu();
                            tempvm.TimeStamp = fitem.TimeStamp;
                            tempvm.ContainerNum = fitem.ContainerNum;
                            tempvm.ToolName = fitem.ToolName;
                            tempvm.RP = fitem.RP;
                            tempvm.UnitNum = fitem.UnitNum;
                            tempvm.X = fitem.X;
                            tempvm.Y = fitem.Y;
                            tempvm.CommonTestName = spec.ParamName.Replace(RPStr, "");
                            tempvm.TestValue = muitem.TestValue;
                            tempvm.Bin_PN = spec.Bin_PN;

                            ret.Add(tempvm);
                        }//end foreach
                    }
                }//end foreach
            }//end foreach

            return ret;
        }


        public string Bin_PN { set; get; }
    }
}
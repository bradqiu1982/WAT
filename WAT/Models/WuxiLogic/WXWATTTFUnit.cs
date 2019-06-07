using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class WXWATTTFUnit : WXWATProbeTestData
    {
        public static List<WXWATTTFUnit> GetUnitData(List<WXWATProbeTestDataFiltered> filterdata, string rp, List<WXWATTTFuse> ttfuse, List<WXWATTTFSorted> ttfsort, List<WXSpecBinPassFail> ttfunitspec)
        {
            var ret = new List<WXWATTTFUnit>();


            //group filter data
            var groupfilterdata = new Dictionary<string, WXWATProbeTestDataFiltered>();
            foreach (var fitem in filterdata)
            {
                var key = fitem.ContainerNum + ":" + fitem.ToolName + ":" + fitem.RP + ":" + fitem.UnitNum + ":" + fitem.X + ":" + fitem.Y + ":" + fitem.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss");
                if (!groupfilterdata.ContainsKey(key))
                {
                    if (string.Compare(rp, fitem.RP, true) == 0)
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
                }//end if
            }//end foreach

            //use unit num to map grouped filter data
            var groupfilterdatalist = groupfilterdata.Values.ToList();
            var groupunitdatadict = new Dictionary<string, List<WXWATProbeTestDataFiltered>>();
            foreach (var fitem in groupfilterdatalist)
            {
                if (groupunitdatadict.ContainsKey(fitem.UnitNum))
                {
                    groupunitdatadict[fitem.UnitNum].Add(fitem);
                }
                else
                {
                    var templist = new List<WXWATProbeTestDataFiltered>();
                    templist.Add(fitem);
                    groupunitdatadict.Add(fitem.UnitNum, templist);
                }
            }

            //bin_use
            var binusedict = new Dictionary<string, List<WXWATTTFuse>>();
            foreach (var item in ttfuse)
            {
                if (binusedict.ContainsKey(item.Bin_PN))
                {
                    binusedict[item.Bin_PN].Add(item);
                }
                else
                {
                    var templist = new List<WXWATTTFuse>();
                    templist.Add(item);
                    binusedict.Add(item.Bin_PN, templist);
                }
            }

            //bin_sort
            var binsortdict = new Dictionary<string, List<WXWATTTFSorted>>();
            foreach (var item in ttfsort)
            {
                if (binsortdict.ContainsKey(item.Bin_PN))
                {
                    binsortdict[item.Bin_PN].Add(item);
                }
                else
                {
                    var templist = new List<WXWATTTFSorted>();
                    templist.Add(item);
                    binsortdict.Add(item.Bin_PN, templist);
                }
            }

            //bindict
            var bindict = new Dictionary<string, bool>();
            foreach (var spec in ttfunitspec)
            {
                if (!bindict.ContainsKey(spec.Bin_PN))
                { bindict.Add(spec.Bin_PN, true); }
            }

            foreach (var binkv in bindict)
            {
                if (binusedict.ContainsKey(binkv.Key)
                    && binsortdict.ContainsKey(binkv.Key))
                {
                    foreach (var useitem in binusedict[binkv.Key])
                    {
                        foreach (var sortitem in binsortdict[binkv.Key])
                        {
                            if (groupunitdatadict.ContainsKey(sortitem.UnitNum))
                            {
                                foreach (var fitem in groupunitdatadict[sortitem.UnitNum])
                                {
                                    var tempvm = new WXWATTTFUnit();
                                    tempvm.TimeStamp = fitem.TimeStamp;
                                    tempvm.ContainerNum = fitem.ContainerNum;
                                    tempvm.ToolName = fitem.ToolName;
                                    tempvm.RP = fitem.RP;
                                    tempvm.UnitNum = fitem.UnitNum;
                                    tempvm.X = fitem.X;
                                    tempvm.Y = fitem.Y;
                                    tempvm.CommonTestName = "PO_LD_W_UNIT_TTF_ref1";
                                    tempvm.TestValue = UT.O2D(sortitem.TTF_dB_ref1) / useitem.AF;
                                    tempvm.Bin_PN = sortitem.Bin_PN;
                                    ret.Add(tempvm);
                                }//end foreach
                            }//end if
                        }//end foreach
                    }//end foreach
                }//end if
            }//end foreach bin

            return ret;
        }
        public string Bin_PN { set; get; }
    }
}
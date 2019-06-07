using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MathNet.Numerics.Statistics;


namespace WAT.Models
{
    public class WXWATCouponStats : WXWATProbeTestDataFiltered
    {
        public static List<WXWATCouponStats> GetCouponData(List<WXWATProbeTestDataFiltered> srcdatalist, Dictionary<string, bool> binpndict)
        {
            var ret = new List<WXWATCouponStats>();
            var paramdict = new Dictionary<string, List<WXWATProbeTestDataFiltered>>();
            foreach (var fitem in srcdatalist)
            {
                if (paramdict.ContainsKey(fitem.CommonTestName))
                {
                    paramdict[fitem.CommonTestName].Add(fitem);
                }
                else
                {
                    var templist = new List<WXWATProbeTestDataFiltered>();
                    templist.Add(fitem);
                    paramdict.Add(fitem.CommonTestName, templist);
                }
            }//end foreach

            var paramnewdict = new Dictionary<string, Dictionary<string, string>>();
            foreach (var kv in paramdict)
            {
                paramnewdict.Add(kv.Key, CouponVal(kv.Key, kv.Value));
            }

            foreach (var fitem in srcdatalist)
            {
                if (paramnewdict.ContainsKey(fitem.CommonTestName))
                {
                    ret.Add(new WXWATCouponStats(fitem, binpndict, paramnewdict[fitem.CommonTestName]));
                }
            }

            return ret;
        }

        private static Dictionary<string, string> CouponVal(string param, List<WXWATProbeTestDataFiltered> oneparamvals)
        {
            var ret = new Dictionary<string, string>();
            var tvallist = new List<double>();
            foreach (var item in oneparamvals)
            { tvallist.Add(item.TestValue); }

            var sum = tvallist.Sum();
            ret.Add(param + "_sum", sum.ToString());
            ret.Add(param + "_dppm", ((sum / (double)tvallist.Count) * 1000000.0).ToString());
            var stddev = Statistics.StandardDeviation(tvallist);
            ret.Add(param + "_stddev", stddev.ToString());
            var mean = tvallist.Average();
            ret.Add(param + "_mean", mean.ToString());

            var ratiodeltaref1list = new List<double>();
            foreach (var item in oneparamvals)
            {
                if (!string.IsNullOrEmpty(item.DeltaList[1].ratiodeltaref))
                {
                    ratiodeltaref1list.Add(UT.O2D(item.DeltaList[1].ratiodeltaref));
                }
            }

            if (ratiodeltaref1list.Count > 0)
            {
                var rmean1 = ratiodeltaref1list.Average();
                var rstddev1 = Statistics.StandardDeviation(ratiodeltaref1list);
                ret.Add(param + "_MXDP", (rmean1 - rstddev1).ToString());
                ret.Add(param + "_SDD", rstddev1.ToString());
            }
            //else
            //{
            //    ret.Add(param + "_MXDP", "");
            //    ret.Add(param + "_SDD","");
            //}

            var ratiodeltaref0list = new List<double>();
            var addeltaref0list = new List<double>();
            var dbdeltaref0list = new List<double>();

            foreach (var item in oneparamvals)
            {
                if (!string.IsNullOrEmpty(item.DeltaList[0].ratiodeltaref))
                {
                    ratiodeltaref0list.Add(UT.O2D(item.DeltaList[0].ratiodeltaref));
                }

                if (!string.IsNullOrEmpty(item.DeltaList[0].absolutedeltaref))
                {
                    addeltaref0list.Add(UT.O2D(item.DeltaList[0].absolutedeltaref));
                }

                if (!string.IsNullOrEmpty(item.DeltaList[0].dBdeltaref))
                {
                    dbdeltaref0list.Add(UT.O2D(item.DeltaList[0].dBdeltaref));
                }
            }

            if (ratiodeltaref0list.Count > 0)
            {
                ret.Add(param + "_mean_rd_ref0", ratiodeltaref0list.Average().ToString());
                ret.Add(param + "_stddev_rd_ref0", Statistics.StandardDeviation(ratiodeltaref0list).ToString());
            }
            //else
            //{
            //    ret.Add(param + "_mean_rd_ref0", "");
            //    ret.Add(param + "_stddev_rd_ref0", "");
            //}

            if (addeltaref0list.Count > 0)
            {
                ret.Add(param + "_mean_ad_ref0", addeltaref0list.Average().ToString());
                ret.Add(param + "_stddev_ad_ref0", Statistics.StandardDeviation(addeltaref0list).ToString());
            }
            //else
            //{
            //    ret.Add(param + "_mean_ad_ref0", "");
            //    ret.Add(param + "_stddev_ad_ref0", "");
            //}

            if (dbdeltaref0list.Count > 0)
            {
                ret.Add(param + "_mean_db_ref0", dbdeltaref0list.Average().ToString());
                ret.Add(param + "_stddev_db_ref0", Statistics.StandardDeviation(dbdeltaref0list).ToString());
            }
            //else
            //{
            //    ret.Add(param + "_mean_db_ref0", "");
            //    ret.Add(param + "_stddev_db_ref0", "");
            //}

            if (string.Compare(param, "PO_LD_W", true) == 0)
            {
                if (ratiodeltaref1list.Count > 0)
                {
                    var midx = 0;
                    if (ratiodeltaref1list.Count % 2 == 0)
                    { midx = ratiodeltaref1list.Count / 2; }
                    else
                    { midx = (ratiodeltaref1list.Count - 1) / 2; }

                    ratiodeltaref1list.Sort();
                    var maxlist = new List<double>();
                    maxlist.AddRange(ratiodeltaref1list.GetRange(0, midx));

                    ratiodeltaref1list.Reverse();
                    var minlist = new List<double>();
                    minlist.AddRange(ratiodeltaref1list.GetRange(0, midx));

                    ret.Add(param + "_MDD", ((maxlist.Max() + minlist.Min()) / 2.0).ToString());
                }
                //else
                //{
                //    ret.Add(param + "_MDD","");
                //}
            }

            return ret;
        }


        private WXWATCouponStats(WXWATProbeTestDataFiltered data, Dictionary<string, bool> binpndict, Dictionary<string, string> cpdict)
        {
            TimeStamp = data.TimeStamp;
            ContainerNum = data.ContainerNum;
            ToolName = data.ToolName;
            RP = data.RP;
            UnitNum = data.UnitNum;
            X = data.X;
            Y = data.Y;
            CommonTestName = "";
            TestValue = 0.0;

            BinPNDict = new Dictionary<string, bool>();
            foreach (var kv in binpndict)
            { BinPNDict.Add(kv.Key, kv.Value); }

            CPValDict = new Dictionary<string, string>();
            foreach (var kv in cpdict)
            { CPValDict.Add(kv.Key, kv.Value); }
        }

        public Dictionary<string, bool> BinPNDict { set; get; }
        public Dictionary<string, string> CPValDict { set; get; }
    }
}
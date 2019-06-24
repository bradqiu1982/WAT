using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WXLogic
{
    public class WXWATFailureMode
    {
        public static string GetFailModeString(List<WXWATFailureMode> modes)
        {
            if (modes.Count == 0)
            { return string.Empty; }

            var failunit = new List<WXWATFailureMode>();
            foreach (var fm in modes)
            {
                if (string.Compare(fm.Failure, "WEAROUT", true) == 0
                || string.Compare(fm.Failure, "DELAM", true) == 0
                || string.Compare(fm.Failure, "DVF", true) == 0
                || string.Compare(fm.Failure, "LOWPOWERLOWLEAKAGE", true) == 0)
                {
                    failunit.Add(fm);
                }
            }

            if (failunit.Count == 0)
            { return string.Empty; }

            var containername = modes[0].ContainerName;
            //var samplexy = WATSampleXY.GetSampleXYByCouponGroup(containername);
            //var samplexydict = new Dictionary<string, WATSampleXY>();
            //foreach (var sitem in samplexy)
            //{
            //    var key = sitem.X + "-" + sitem.Y;
            //    if (!samplexydict.ContainsKey(key))
            //    { samplexydict.Add(key, sitem); }
            //}

            var ret = string.Empty;
            foreach (var fu in failunit)
            {
                //if (samplexydict.ContainsKey(fu.UnitNum))
                //{
                //    ret += samplexydict[fu.UnitNum].CouponID + "-" + samplexydict[fu.UnitNum].ChannelInfo + ":" + fu.Failure + ",";
                //}
                //else
                //{
                    ret += fu.ContainerName+":"+fu.UnitNum + ":" + fu.Failure + ",";
                //}
            }

            return ret;
        }


        public string ContainerName { set; get; }
        public string UnitNum { set; get; }
        public string RP { set; get; }
        public string DPO { set; get; }
        public double DPO_rd { set; get; }
        public double DIth { set; get; }
        public double BVR { set; get; }
        public double DVF { set; get; }
        public string PWR { set; get; }
        public double DPOvsDITHcheck
        {
            get
            {
                return -0.44 * (DIth - 2) - 1;
            }
        }
        public string DPO_LL { set; get; }
        public string DVF_UL { set; get; }

        public string Failure
        {
            get
            {
                if (!string.IsNullOrEmpty(DPO_LL) && !string.IsNullOrEmpty(DVF_UL))
                {
                    var dpoll = UT.O2D(DPO_LL);
                    var dvful = UT.O2D(DVF_UL);
                    if (DPO_rd < dpoll)
                    {
                        if (Math.Abs(DIth) > 0.5 && BVR > 20e-9)
                        { return "DISLOCATION"; }
                        if (DPO_rd < -0.9)
                        { return "LOWPOWERLOWLEAKAGE"; }
                        else
                        {
                            if (string.Compare(Temp, "LOW", true) == 0 || DPO_rd < DPOvsDITHcheck)
                            {
                                if (DVF > dvful)
                                { return "DVF"; }
                                else
                                { return "DELAM"; }
                            }
                            else
                            { return "WEAROUT"; }
                        }
                    }
                    else
                    {
                        if (DVF > dvful)
                        { return "DVF"; }
                        else
                        { return "PASS"; }
                    }
                }
                else
                {
                    return "";
                }
            }
        }
        public string Temp { set; get; }
    }
}
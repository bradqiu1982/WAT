using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class WXWATFailureMode
    {
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
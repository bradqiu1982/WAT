﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WXLogic
{
    public class WXWATProbeTestDataFiltered : WXWATProbeTestData
    {

        public static List<WXWATProbeTestDataFiltered> GetFilteredData(List<WXWATProbeTestData> srcdatalist, string rp)
        {
            var rptimedict = new Dictionary<string, DateTime>();
            foreach (var item in srcdatalist)
            {
                var key = item.UnitNum + "::" + item.CommonTestName + "::" + item.RP;
                if (!rptimedict.ContainsKey(key))
                {
                    rptimedict.Add(key, item.TimeStamp);
                }
                else
                {
                    if (item.TimeStamp > rptimedict[key])
                    { rptimedict[key] = item.TimeStamp; }
                }
            }//end foreach

            var filtereddata = new List<WXWATProbeTestDataFiltered>();
            var RP0WATPBData = new Dictionary<string, WXWATProbeTestData>();
            var RP1WATPBData = new Dictionary<string, WXWATProbeTestData>();
            var RP2WATPBData = new Dictionary<string, WXWATProbeTestData>();
            var RP3WATPBData = new Dictionary<string, WXWATProbeTestData>();
            var RP4WATPBData = new Dictionary<string, WXWATProbeTestData>();
            var RP5WATPBData = new Dictionary<string, WXWATProbeTestData>();
            var RP6WATPBData = new Dictionary<string, WXWATProbeTestData>();

            foreach (var item in srcdatalist)
            {

                var key = item.UnitNum + "::" + item.CommonTestName + "::" + item.RP;
                if (item.TimeStamp == rptimedict[key])
                {
                    if (string.Compare(item.RP, rp, true) == 0)
                    { filtereddata.Add(new WXWATProbeTestDataFiltered(item)); }

                    if (string.Compare(item.RP, "0") == 0)
                    { if (!RP0WATPBData.ContainsKey(key)) { RP0WATPBData.Add(key, item); } }
                    if (string.Compare(item.RP, "1") == 0)
                    { if (!RP1WATPBData.ContainsKey(key)) { RP1WATPBData.Add(key, item); } }
                    if (string.Compare(item.RP, "2") == 0)
                    { if (!RP2WATPBData.ContainsKey(key)) { RP2WATPBData.Add(key, item); } }
                    if (string.Compare(item.RP, "3") == 0)
                    { if (!RP3WATPBData.ContainsKey(key)) { RP3WATPBData.Add(key, item); } }
                    if (string.Compare(item.RP, "4") == 0)
                    { if (!RP4WATPBData.ContainsKey(key)) { RP4WATPBData.Add(key, item); } }
                    if (string.Compare(item.RP, "5") == 0)
                    { if (!RP5WATPBData.ContainsKey(key)) { RP5WATPBData.Add(key, item); } }
                    if (string.Compare(item.RP, "6") == 0)
                    { if (!RP6WATPBData.ContainsKey(key)) { RP6WATPBData.Add(key, item); } }
                }
            }//end foreach

            var rplist = new List<string>();
            rplist.Add("0"); rplist.Add("1");
            rplist.Add("2"); rplist.Add("3");
            rplist.Add("4"); rplist.Add("5");
            rplist.Add("6");

            foreach (var fitem in filtereddata)
            {
                foreach (var p in rplist)
                {
                    var key = fitem.UnitNum + "::" + fitem.CommonTestName + "::" + p;
                    var RPWATPBData = new Dictionary<string, WXWATProbeTestData>();
                    if (string.Compare(p, "0") == 0)
                    { RPWATPBData = RP0WATPBData; }
                    if (string.Compare(p, "1") == 0)
                    { RPWATPBData = RP1WATPBData; }
                    if (string.Compare(p, "2") == 0)
                    { RPWATPBData = RP2WATPBData; }
                    if (string.Compare(p, "3") == 0)
                    { RPWATPBData = RP3WATPBData; }
                    if (string.Compare(p, "4") == 0)
                    { RPWATPBData = RP4WATPBData; }
                    if (string.Compare(p, "5") == 0)
                    { RPWATPBData = RP5WATPBData; }
                    if (string.Compare(p, "6") == 0)
                    { RPWATPBData = RP6WATPBData; }

                    if (RPWATPBData.ContainsKey(key))
                    {
                        var dv = new WXWATDeltaVal();
                        dv.TestValue = RPWATPBData[key].TestValue.ToString();
                        dv.absolutedeltaref = (fitem.TestValue - RPWATPBData[key].TestValue).ToString();
                        if (RPWATPBData[key].TestValue != 0)
                        {
                            dv.ratiodeltaref = ((fitem.TestValue - RPWATPBData[key].TestValue) / RPWATPBData[key].TestValue).ToString();
                            if (fitem.TestValue > 0 && RPWATPBData[key].TestValue > 0)
                            { dv.dBdeltaref = (10 * Math.Log10(fitem.TestValue / RPWATPBData[key].TestValue)).ToString(); }
                            else
                            { dv.dBdeltaref = ""; }

                        }
                        else
                        {
                            dv.ratiodeltaref = "";
                            dv.dBdeltaref = "";
                        }
                        fitem.DeltaList.Add(dv);
                    }
                    else
                    {
                        fitem.DeltaList.Add(new WXWATDeltaVal());
                    }
                }//end foreach rp
            }//foreach unit,testname

            rplist = new List<string>();
            rplist.Add("0"); rplist.Add("1");

            foreach (var fitem in filtereddata)
            {
                var prep = Convert.ToInt32(fitem.RP) - 1;
                var PRERPWATPBData = new Dictionary<string, WXWATProbeTestData>();
                if (prep == 0)
                { PRERPWATPBData = RP0WATPBData; }
                if (prep == 1)
                { PRERPWATPBData = RP1WATPBData; }
                if (prep == 2)
                { PRERPWATPBData = RP2WATPBData; }
                if (prep == 3)
                { PRERPWATPBData = RP3WATPBData; }
                if (prep == 4)
                { PRERPWATPBData = RP4WATPBData; }
                if (prep == 5)
                { PRERPWATPBData = RP5WATPBData; }
                if (prep == 6)
                { PRERPWATPBData = RP6WATPBData; }

                var prekey = fitem.UnitNum + "::" + fitem.CommonTestName + "::" + prep.ToString();
                if (!PRERPWATPBData.ContainsKey(prekey))
                {
                    fitem.PrevTestValue = "";
                    fitem.PreDeltaList.Add(new WXWATDeltaVal());
                    fitem.PreDeltaList.Add(new WXWATDeltaVal());
                }
                else
                {
                    fitem.PrevTestValue = PRERPWATPBData[prekey].TestValue.ToString();
                    foreach (var p in rplist)
                    {
                        var key = fitem.UnitNum + "::" + fitem.CommonTestName + "::" + p;
                        var RPWATPBData = new Dictionary<string, WXWATProbeTestData>();
                        if (string.Compare(p, "0") == 0)
                        { RPWATPBData = RP0WATPBData; }
                        if (string.Compare(p, "1") == 0)
                        { RPWATPBData = RP1WATPBData; }

                        if (RPWATPBData.ContainsKey(key))
                        {
                            var dv = new WXWATDeltaVal();
                            dv.TestValue = RPWATPBData[key].TestValue.ToString();
                            dv.absolutedeltaref = (PRERPWATPBData[prekey].TestValue - RPWATPBData[key].TestValue).ToString();
                            if (RPWATPBData[key].TestValue != 0)
                            {
                                dv.ratiodeltaref = ((PRERPWATPBData[prekey].TestValue - RPWATPBData[key].TestValue) / RPWATPBData[key].TestValue).ToString();
                                if (PRERPWATPBData[prekey].TestValue > 0 && RPWATPBData[key].TestValue > 0)
                                { dv.dBdeltaref = (10 * Math.Log10(PRERPWATPBData[prekey].TestValue / RPWATPBData[key].TestValue)).ToString(); }
                                else
                                { dv.dBdeltaref = ""; }

                            }
                            else
                            {
                                dv.ratiodeltaref = "";
                                dv.dBdeltaref = "";
                            }
                            fitem.PreDeltaList.Add(dv);
                        }
                        else
                        {
                            fitem.PreDeltaList.Add(new WXWATDeltaVal());
                        }
                    }//end foreach rp
                }


            }//foreach unit,testname


            return filtereddata;
        }

        public static List<WXWATFailureMode> GetWATFailureModes(List<WXWATProbeTestDataFiltered> srcdata, List<WXSpecBinPassFail> spec, Double bitemp)
        {
            var DPOLL = "";
            var DVFUL = "";
            foreach (var item in spec)
            {
                if (item.ParamName.ToUpper().Contains("PO_LD_W_RD_REF"))
                { DPOLL = item.WLL; }
                if (item.ParamName.ToUpper().Contains("VF_LD_V_AD_REF"))
                { DVFUL = item.WUL; }
            }

            var ret = new List<WXWATFailureMode>();
            if (bitemp == 150 || bitemp == 100)
            {
                ret.AddRange(getfmode(srcdata, DPOLL, DVFUL));
            }
            else
            {
                ret.AddRange(getfmode(srcdata, DPOLL, DVFUL, false));
            }
            return ret;
        }

        private static List<WXWATFailureMode> getfmode(List<WXWATProbeTestDataFiltered> srcdata, string DPOLL, string DVFUL, bool hightemp = true)
        {
            var ret = new List<WXWATFailureMode>();

            var DPODict = new Dictionary<string, double>();
            var DPOrdDict = new Dictionary<string, double>();
            var DIthDict = new Dictionary<string, double>();
            var BVRDict = new Dictionary<string, double>();
            var DVFDict = new Dictionary<string, double>();
            var PWRDict = new Dictionary<string, double>();

            foreach (var fitem in srcdata)
            {
                if (string.Compare(fitem.CommonTestName, "PO_LD_W", true) == 0)
                {
                    if (hightemp)
                    {
                        if (!string.IsNullOrEmpty(fitem.DeltaList[1].dBdeltaref))
                        {
                            if (!DPODict.ContainsKey(fitem.UnitNum))
                            { DPODict.Add(fitem.UnitNum, UT.O2D(fitem.DeltaList[1].dBdeltaref)); }
                        }

                        if (!string.IsNullOrEmpty(fitem.DeltaList[1].ratiodeltaref))
                        {
                            if (!DPOrdDict.ContainsKey(fitem.UnitNum))
                            { DPOrdDict.Add(fitem.UnitNum, UT.O2D(fitem.DeltaList[1].ratiodeltaref)); }
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(fitem.DeltaList[0].dBdeltaref))
                        {
                            if (!DPODict.ContainsKey(fitem.UnitNum))
                            { DPODict.Add(fitem.UnitNum, UT.O2D(fitem.DeltaList[0].dBdeltaref)); }
                        }

                        if (!string.IsNullOrEmpty(fitem.DeltaList[0].ratiodeltaref))
                        {
                            if (!DPOrdDict.ContainsKey(fitem.UnitNum))
                            { DPOrdDict.Add(fitem.UnitNum, UT.O2D(fitem.DeltaList[0].ratiodeltaref)); }
                        }
                    }

                    if (!PWRDict.ContainsKey(fitem.UnitNum))
                    { PWRDict.Add(fitem.UnitNum, fitem.TestValue); }

                }//end if

                if (string.Compare(fitem.CommonTestName, "THOLD_A", true) == 0)
                {
                    if (hightemp)
                    {
                        if (!string.IsNullOrEmpty(fitem.DeltaList[1].ratiodeltaref))
                        {
                            if (!DIthDict.ContainsKey(fitem.UnitNum))
                            { DIthDict.Add(fitem.UnitNum, UT.O2D(fitem.DeltaList[1].ratiodeltaref)); }
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(fitem.DeltaList[0].ratiodeltaref))
                        {
                            if (!DIthDict.ContainsKey(fitem.UnitNum))
                            { DIthDict.Add(fitem.UnitNum, UT.O2D(fitem.DeltaList[0].ratiodeltaref)); }
                        }
                    }
                }

                if (string.Compare(fitem.CommonTestName, "BVR_LD_A", true) == 0)
                {
                    if (!BVRDict.ContainsKey(fitem.UnitNum))
                    { BVRDict.Add(fitem.UnitNum, fitem.TestValue); }
                }


                if (string.Compare(fitem.CommonTestName, "VF_LD_V", true) == 0)
                {
                    if (hightemp)
                    {
                        if (!string.IsNullOrEmpty(fitem.DeltaList[1].absolutedeltaref))
                        {
                            if (!DVFDict.ContainsKey(fitem.UnitNum))
                            { DVFDict.Add(fitem.UnitNum, UT.O2D(fitem.DeltaList[1].absolutedeltaref)); }
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(fitem.DeltaList[0].absolutedeltaref))
                        {
                            if (!DVFDict.ContainsKey(fitem.UnitNum))
                            { DVFDict.Add(fitem.UnitNum, UT.O2D(fitem.DeltaList[0].absolutedeltaref)); }
                        }
                    }
                }



            }//end foreach

            var udict = new Dictionary<string, bool>();
            foreach (var fitem in srcdata)
            {
                if (string.Compare(fitem.CommonTestName, "PO_LD_W", true) == 0 && !udict.ContainsKey(fitem.UnitNum))
                {
                    udict.Add(fitem.UnitNum, true);

                    if (DPOrdDict.ContainsKey(fitem.UnitNum) && DIthDict.ContainsKey(fitem.UnitNum)
                        && BVRDict.ContainsKey(fitem.UnitNum) && DVFDict.ContainsKey(fitem.UnitNum))
                    {
                        var tempvm = new WXWATFailureMode();
                        tempvm.ContainerName = fitem.ContainerNum;
                        tempvm.UnitNum = fitem.UnitNum;
                        tempvm.X = fitem.X;
                        tempvm.Y = fitem.Y;
                        tempvm.RP = fitem.RP;
                        tempvm.DPO_LL = DPOLL;
                        tempvm.DVF_UL = DVFUL;
                        tempvm.Temp = "HIGH";
                        if (!hightemp)
                        { tempvm.Temp = "LOW"; }

                        tempvm.DPO_rd = DPOrdDict[fitem.UnitNum];
                        tempvm.DIth = DIthDict[fitem.UnitNum];
                        tempvm.BVR = BVRDict[fitem.UnitNum];
                        tempvm.DVF = DVFDict[fitem.UnitNum];

                        if (DPODict.ContainsKey(fitem.UnitNum))
                        { tempvm.DPO = DPODict[fitem.UnitNum].ToString(); }
                        else
                        { tempvm.DPO = ""; }

                        if (PWRDict.ContainsKey(fitem.UnitNum))
                        { tempvm.PWR = PWRDict[fitem.UnitNum].ToString(); }
                        else
                        { tempvm.PWR = ""; }

                        ret.Add(tempvm);
                    }//end it
                }//end if
            }//end foreach

            return ret;
        }

        private WXWATProbeTestDataFiltered(WXWATProbeTestData data)
        {
            TimeStamp = data.TimeStamp;
            ContainerNum = data.ContainerNum;
            ToolName = data.ToolName;
            RP = data.RP;
            UnitNum = data.UnitNum;
            X = data.X;
            Y = data.Y;
            CommonTestName = data.CommonTestName;
            TestValue = data.TestValue;
            ProbeValue = data.ProbeValue;
            BinNum = data.BinNum;
            BinName = data.BinName;

            PrevTestValue = "";
            DeltaList = new List<WXWATDeltaVal>();
            PreDeltaList = new List<WXWATDeltaVal>();

            if (!string.IsNullOrEmpty(data.ProbeValue))
            {
                var dprobeval = Convert.ToDouble(data.ProbeValue);
                couponminusprobe = (TestValue - dprobeval).ToString();
                if (dprobeval != 0)
                { couponminusprobePCT = ((TestValue - dprobeval) / dprobeval*100.0).ToString(); }
                else { couponminusprobePCT = ""; }
            }
            else
            {
                couponminusprobe = "";
                couponminusprobePCT = "";
            }

        }

        public WXWATProbeTestDataFiltered()
        {

        }

        public List<WXWATDeltaVal> DeltaList { set; get; }
        public string PrevTestValue { set; get; }
        public List<WXWATDeltaVal> PreDeltaList { set; get; }
        public string couponminusprobe { set; get; }
        public string couponminusprobePCT { set; get; }
    }

    public class WXWATDeltaVal
    {
        public WXWATDeltaVal()
        {
            TestValue = "";
            ratiodeltaref = "";
            absolutedeltaref = "";
            dBdeltaref = "";
        }

        public string TestValue { set; get; }
        public string ratiodeltaref { set; get; }
        public string absolutedeltaref { set; get; }
        public string dBdeltaref { set; get; }
    }
}
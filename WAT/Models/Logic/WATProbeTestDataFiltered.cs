using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class WATProbeTestDataFiltered : WATProbeTestData
    {
        public static List<WATProbeTestDataFiltered> GetFilteredData(List<WATProbeTestData> srcdatalist,string rp)
        {
            var rptimedict = new Dictionary<string, DateTime>();
            foreach (var item in srcdatalist)
            {
                var key =  item.UnitNum + "::" + item.CommonTestName+ "::" +item.RP ;
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

            var filtereddata = new List<WATProbeTestDataFiltered>();
            var RP0WATPBData = new Dictionary<string, WATProbeTestData>();
            var RP1WATPBData = new Dictionary<string, WATProbeTestData>();
            var RP2WATPBData = new Dictionary<string, WATProbeTestData>();
            var RP3WATPBData = new Dictionary<string, WATProbeTestData>();

            foreach (var item in srcdatalist)
            {
                var key =  item.UnitNum + "::" + item.CommonTestName+ "::"+item.RP;
                if (item.TimeStamp == rptimedict[key])
                {
                    if (string.Compare(item.RP, rp, true) == 0)
                    { filtereddata.Add(new WATProbeTestDataFiltered(item)); }

                    if (string.Compare(item.RP, "0") == 0)
                    { if (!RP0WATPBData.ContainsKey(key)) { RP0WATPBData.Add(key, item); } }
                    if (string.Compare(item.RP, "1") == 0)
                    { if (!RP1WATPBData.ContainsKey(key)) { RP1WATPBData.Add(key, item); } }
                    if (string.Compare(item.RP, "2") == 0)
                    { if (!RP2WATPBData.ContainsKey(key)) { RP2WATPBData.Add(key, item); } }
                    if (string.Compare(item.RP, "3") == 0)
                    { if (!RP3WATPBData.ContainsKey(key)) { RP3WATPBData.Add(key, item); } }
                }
            }//end foreach

            var rplist = new List<string>();
            rplist.Add("0"); rplist.Add("1");
            rplist.Add("2"); rplist.Add("3");

            foreach (var fitem in filtereddata)
            {
                foreach (var p in rplist)
                {
                    var key = fitem.UnitNum + "::" + fitem.CommonTestName + "::" + p;
                    var RPWATPBData = new Dictionary<string, WATProbeTestData>();
                    if (string.Compare(p, "0") == 0)
                    { RPWATPBData = RP0WATPBData; }
                    if (string.Compare(p, "1") == 0)
                    { RPWATPBData = RP1WATPBData; }
                    if (string.Compare(p, "2") == 0)
                    { RPWATPBData = RP2WATPBData; }
                    if (string.Compare(p, "3") == 0)
                    { RPWATPBData = RP3WATPBData; }

                    if (RPWATPBData.ContainsKey(key))
                    {
                        var dv = new WATDeltaVal();
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
                        fitem.DeltaList.Add(new WATDeltaVal());
                    }
                }//end foreach rp
            }//foreach unit,testname

            rplist = new List<string>();
            rplist.Add("0"); rplist.Add("1");

            foreach (var fitem in filtereddata)
            {
                var prep = Convert.ToInt32(fitem.RP) - 1;
                var PRERPWATPBData = new Dictionary<string, WATProbeTestData>();
                if (prep == 0)
                { PRERPWATPBData = RP0WATPBData; }
                if (prep == 1)
                { PRERPWATPBData = RP1WATPBData; }
                if (prep == 2)
                { PRERPWATPBData = RP2WATPBData; }
                if (prep == 3)
                { PRERPWATPBData = RP3WATPBData; }

                var prekey = fitem.UnitNum + "::" + fitem.CommonTestName + "::" + prep.ToString();
                if (!PRERPWATPBData.ContainsKey(prekey))
                {
                    fitem.PrevTestValue = "";
                    fitem.PreDeltaList.Add(new WATDeltaVal());
                    fitem.PreDeltaList.Add(new WATDeltaVal());
                }
                else
                {
                    fitem.PrevTestValue = PRERPWATPBData[prekey].TestValue.ToString();
                    foreach (var p in rplist)
                    {
                        var key = fitem.UnitNum + "::" + fitem.CommonTestName + "::" + p;
                        var RPWATPBData = new Dictionary<string, WATProbeTestData>();
                        if (string.Compare(p, "0") == 0)
                        { RPWATPBData = RP0WATPBData; }
                        if (string.Compare(p, "1") == 0)
                        { RPWATPBData = RP1WATPBData; }

                        if (RPWATPBData.ContainsKey(key))
                        {
                            var dv = new WATDeltaVal();
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
                            fitem.PreDeltaList.Add(new WATDeltaVal());
                        }
                    }//end foreach rp
                }


            }//foreach unit,testname


            return filtereddata;
        }

        private WATProbeTestDataFiltered(WATProbeTestData data)
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
            DeltaList = new List<WATDeltaVal>();
            PreDeltaList = new List<WATDeltaVal>();

            if (!string.IsNullOrEmpty(data.ProbeValue))
            {
                var dprobeval = Convert.ToDouble(data.ProbeValue);
                couponminusprobe = (TestValue - dprobeval).ToString();
                if (dprobeval != 0)
                { couponminusprobePCT = ((TestValue - dprobeval)/ dprobeval).ToString(); }
                else { couponminusprobePCT = ""; }
            }
            else
            {
                couponminusprobe = "";
                couponminusprobePCT = "";
            }

        }

        public WATProbeTestDataFiltered()
        {
            
        }

        public List<WATDeltaVal> DeltaList { set; get; }
        public string PrevTestValue { set; get; }
        public List<WATDeltaVal> PreDeltaList { set; get; }
        public string couponminusprobe { set; get; }
        public string couponminusprobePCT { set; get; }
    }

    public class WATDeltaVal
    {
        public WATDeltaVal()
        {
            TestValue = "";
            ratiodeltaref = "";
            absolutedeltaref = "";
            dBdeltaref = "";
        }

        public string TestValue{ set; get; }
        public string ratiodeltaref { set; get; }
        public string absolutedeltaref { set; get; }
        public string dBdeltaref { set; get; }
    }
}
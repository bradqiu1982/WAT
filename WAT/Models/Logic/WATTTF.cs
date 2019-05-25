using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class WATTTF
    {
        private static List<double> GetCumBITime(string evalpn,int rp)
        {

            var precumval = 0.0;
            var cumval = 0.0;
            var sql = @"SELECT condition_value,spec FROM [EngrData].[dbo].[Eval_Conditions_New] where evalpartnumber = @evalpn and condition = 'duration'";

            var dict = new Dictionary<string, string>();
            dict.Add("@evalpn", evalpn);

            var dbret = DataBaseUT.ExeAllenSqlWithRes(sql, dict);
            foreach(var line in dbret)
            {
                var val = UT.O2D(line[0]);
                var spec = UT.O2S(line[1]);
                var srp = 0;
                if (spec.Length > 2)
                { srp = UT.O2I(spec.Substring(spec.Length-2)); }

                if (srp > 1 && srp <= (rp-1))
                { precumval += val; }

                if (srp > 1 && srp <= rp)
                { cumval += val; }
            }

            var ret = new List<double>();
            ret.Add(precumval);ret.Add(cumval);
            return ret;
        }

        public static List<WATTTF> GetTTFData(string evalpn,int rp
            ,List<SpecBinPassFail> fitspec,List<WATProbeTestDataFiltered> filterdata)
        {
            var RPStr = "_rp" + (100 +rp).ToString().Substring(1);

            var ret = new List<WATTTF>();

            var precumval = 0.0;
            var cumval = 0.0;
            var timelist = GetCumBITime(evalpn, rp);
            precumval = timelist[0];
            cumval = timelist[1];


            var sql = @"select top 1 * from engrdata.dbo.Eval_Conditions_New cond where cond.evalpartnumber=@productname AND cast(right(cond.spec,2) as int)=@rp and [Condition] = 'Duration'";
            var dict = new Dictionary<string, string>();
            dict.Add("@productname", evalpn);
            dict.Add("@rp", rp.ToString());
            var dbret = DataBaseUT.ExeAllenSqlWithRes(sql,dict);
            if (dbret.Count > 0)
            {
                var specdict = new Dictionary<string, List<SpecBinPassFail>>();
                foreach (var spec in fitspec)
                {
                    var upparamname = spec.ParamName.ToUpper();
                    if (specdict.ContainsKey(upparamname))
                    { specdict[upparamname].Add(spec); }
                    else
                    {
                        var templist = new List<SpecBinPassFail>();
                        templist.Add(spec);
                        specdict.Add(upparamname, templist);
                    }
                }//end foreach

                foreach (var fitem in filterdata)
                {
                    var fname = fitem.CommonTestName + "_FITmu_ref0" + RPStr;
                    if (specdict.ContainsKey(fname.ToUpper()))
                    {
                        var speclist = specdict[fname.ToUpper()];
                        foreach (var spec in speclist)
                        {
                            var tempvm = new WATTTF();
                            tempvm.ParamName = spec.ParamName;
                            tempvm.Bin_PN = spec.Bin_PN;
                            tempvm.DTUL = spec.DTUL;
                            tempvm.DTLL = spec.DTLL;
                            tempvm.cumulativeBItime = cumval;
                            tempvm.cumulativeBItime_prev = precumval;
                            tempvm.UnitNum = fitem.UnitNum;

                            var ratiodelta_ref0_prev = fitem.PreDeltaList[0].ratiodeltaref;
                            var ratiodelta_ref0 = fitem.DeltaList[0].ratiodeltaref;
                            if (!string.IsNullOrEmpty(ratiodelta_ref0_prev) && !string.IsNullOrEmpty(ratiodelta_ref0))
                            {
                                var ratvalprev = UT.O2D(ratiodelta_ref0_prev);
                                var ratval = UT.O2D(ratiodelta_ref0);
                                var ttfdb = 0.0;
                                try
                                {
                                    ttfdb = precumval + (0.0 - (1 - Math.Round(Math.Pow(10.0,UT.O2D(spec.DTLL) / 10.0), 3)) - ratvalprev) / ((ratval - ratvalprev) / (cumval - precumval));
                                    tempvm.TTF_dB_ref0 = ttfdb.ToString();
                                }
                                catch(Exception ex) { tempvm.TTF_dB_ref0 = ""; }
                            }
                            else
                            { tempvm.TTF_dB_ref0 = ""; }

                            var ratiodelta_ref1_prev = fitem.PreDeltaList[1].ratiodeltaref;
                            var ratiodelta_ref1 = fitem.DeltaList[1].ratiodeltaref;
                            if (!string.IsNullOrEmpty(ratiodelta_ref1_prev) && !string.IsNullOrEmpty(ratiodelta_ref1))
                            {
                                var ratvalprev = UT.O2D(ratiodelta_ref1_prev);
                                var ratval = UT.O2D(ratiodelta_ref1);
                                var ttfdb = 0.0;
                                try
                                {
                                    ttfdb = precumval + (0.0 - (1 - Math.Round(Math.Pow(10.0, UT.O2D(spec.DTLL) / 10.0), 3)) - ratvalprev) / ((ratval - ratvalprev) / (cumval - precumval));
                                    tempvm.TTF_dB_ref1 = ttfdb.ToString();
                                }
                                catch (Exception ex) { tempvm.TTF_dB_ref1 = ""; }
                            }
                            else
                            { tempvm.TTF_dB_ref1 = ""; }

                            tempvm.dBdelta_ref0 = fitem.DeltaList[0].dBdeltaref;
                            tempvm.dBdelta_ref0_prev = fitem.PreDeltaList[0].dBdeltaref;

                            tempvm.dBdelta_ref1 = fitem.DeltaList[1].dBdeltaref;
                            tempvm.dBdelta_ref1_prev = fitem.PreDeltaList[1].dBdeltaref;
                            ret.Add(tempvm);
                        }
                    }//end if

                    fname = fitem.CommonTestName + "_FITmu_ref1" + RPStr;
                    if (specdict.ContainsKey(fname.ToUpper()))
                    {
                        var speclist = specdict[fname.ToUpper()];
                        foreach (var spec in speclist)
                        {
                            var tempvm = new WATTTF();
                            tempvm.ParamName = spec.ParamName;
                            tempvm.Bin_PN = spec.Bin_PN;
                            tempvm.DTUL = spec.DTUL;
                            tempvm.DTLL = spec.DTLL;
                            tempvm.cumulativeBItime = cumval;
                            tempvm.cumulativeBItime_prev = precumval;
                            tempvm.UnitNum = fitem.UnitNum;

                            var ratiodelta_ref0_prev = fitem.PreDeltaList[0].ratiodeltaref;
                            var ratiodelta_ref0 = fitem.DeltaList[0].ratiodeltaref;
                            if (!string.IsNullOrEmpty(ratiodelta_ref0_prev) && !string.IsNullOrEmpty(ratiodelta_ref0))
                            {
                                var ratvalprev = UT.O2D(ratiodelta_ref0_prev);
                                var ratval = UT.O2D(ratiodelta_ref0);
                                var ttfdb = 0.0;
                                try
                                {
                                    ttfdb = precumval + (0.0 - (1 - Math.Round(Math.Pow(10.0, UT.O2D(spec.DTLL) / 10.0), 3)) - ratvalprev) / ((ratval - ratvalprev) / (cumval - precumval));
                                    tempvm.TTF_dB_ref0 = ttfdb.ToString();
                                }
                                catch (Exception ex) { tempvm.TTF_dB_ref0 = ""; }
                            }
                            else
                            { tempvm.TTF_dB_ref0 = ""; }

                            var ratiodelta_ref1_prev = fitem.PreDeltaList[1].ratiodeltaref;
                            var ratiodelta_ref1 = fitem.DeltaList[1].ratiodeltaref;
                            if (!string.IsNullOrEmpty(ratiodelta_ref1_prev) && !string.IsNullOrEmpty(ratiodelta_ref1))
                            {
                                var ratvalprev = UT.O2D(ratiodelta_ref1_prev);
                                var ratval = UT.O2D(ratiodelta_ref1);
                                var ttfdb = 0.0;
                                try
                                {
                                    ttfdb = precumval + (0.0 - (1 - Math.Round(Math.Pow(10.0,UT.O2D(spec.DTLL) / 10.0), 3)) - ratvalprev) / ((ratval - ratvalprev) / (cumval - precumval));
                                    tempvm.TTF_dB_ref1 = ttfdb.ToString();
                                }
                                catch (Exception ex) { tempvm.TTF_dB_ref1 = ""; }
                            }
                            else
                            { tempvm.TTF_dB_ref1 = ""; }

                            tempvm.dBdelta_ref0 = fitem.DeltaList[0].dBdeltaref;
                            tempvm.dBdelta_ref0_prev = fitem.PreDeltaList[0].dBdeltaref;

                            tempvm.dBdelta_ref1 = fitem.DeltaList[1].dBdeltaref;
                            tempvm.dBdelta_ref1_prev = fitem.PreDeltaList[1].dBdeltaref;
                            ret.Add(tempvm);
                        }
                    }//end if
                }//end foreach

            }//end if

            return ret;
        }


        public WATTTF()
        {
            ParamName = "";
            Bin_PN = "";
            DTUL = "";
            DTLL = "";
            cumulativeBItime = 0.0;
            cumulativeBItime_prev = 0.0;
            UnitNum = "";
            TTF_dB_ref0 = "";
            TTF_dB_ref1 = "";
            dBdelta_ref0 = "";
            dBdelta_ref0_prev = "";
            dBdelta_ref1 = "";
            dBdelta_ref1_prev = "";
        }

        public string  ParamName {set;get;}
        public string Bin_PN { set; get; }
        public string DTUL { set; get; }
        public string DTLL { set; get; }
        public double cumulativeBItime { set; get; }
        public double cumulativeBItime_prev { set; get; }
        public string UnitNum { set; get; }
        public string TTF_dB_ref0 { set; get; }
        public string TTF_dB_ref1 { set; get; }
        public string dBdelta_ref0 { set; get; }
        public string dBdelta_ref0_prev { set; get; }
        public string dBdelta_ref1 { set; get; }
        public string dBdelta_ref1_prev { set; get; }

    }
}
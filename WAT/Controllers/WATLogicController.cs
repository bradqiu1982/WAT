using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WAT.Models;
using WXLogic;

namespace WAT.Controllers
{
    public class WATLogicController : Controller
    {
        private static string DetermineCompName(string IP)
        {
            try
            {
                IPAddress myIP = IPAddress.Parse(IP);
                IPHostEntry GetIPHost = Dns.GetHostEntry(myIP);
                List<string> compName = GetIPHost.HostName.ToString().Split('.').ToList();
                return compName.First();
            }
            catch (Exception ex)
            { return string.Empty; }
        }

        public ActionResult VerifyAllenLogic()
        {
            var machine = DetermineCompName(Request.UserHostName);
            if (machine.ToUpper().Contains("IPH"))
            { return RedirectToAction("AllenWaferWAT", "WATLogic"); }

            return View();
        }

        public JsonResult VerifyAllenLogicData()
        {
            var container = Request.Form["container"];
            var dcdname = Request.Form["dcdname"];
            var exclusion = Request.Form["exclusion"];
            var ret = new AllenWATLogic();

            var resname = "";
            if (string.Compare(exclusion, "withex", true) == 0)
            {
                ret = AllenWATLogic.PassFail(container, dcdname,false);
                resname = "Logic Result With Exclusion";
            }
            else
            {
                ret = AllenWATLogic.PassFail(container, dcdname, true);
                resname = "Logic Result Without Exclusion";
            }


            var msglist = new List<object>();
            if (!string.IsNullOrEmpty(ret.AppErrorMsg))
            {
                msglist.Add(new
                {
                    pname = "App Exception",
                    pval = ret.AppErrorMsg
                });
            }
            else
            {
                msglist.Add(new
                {
                    pname = resname,
                    pval = ret.ResultMsg
                });

                foreach (var kv in ret.ValueCollect)
                {
                    msglist.Add(new
                    {
                        pname = kv.Key,
                        pval = kv.Value
                    });
                }
            }

            var jret = new JsonResult();
            jret.MaxJsonLength = Int32.MaxValue;
            jret.Data = new
            {
                msglist = msglist,
                exclusionlist = ret.ExclusionInfo,
                datatables = ret.DataTables
            };
            return jret;

        }

        public JsonResult GetAllenContainerList()
        {
            var containlist = AllenEVALData.GetAllenContainerList();
            var ret = new JsonResult();
            ret.Data = new {
                containlist = containlist
            };
            return ret;
        }

        public JsonResult GetAllen2WXWaferList()
        {
            var containlist = AllenEVALData.GetAllen2WXWaferList();
            var ret = new JsonResult();
            ret.Data = new
            {
                containlist = containlist
            };
            return ret;
        }

        public ActionResult AllenWaferWAT()
        {
            return View();
        }

        public JsonResult AllenWaferWATData()
        {
            var wafernum = Request.Form["wafernum"];
            var rplist = new List<string>();
            rplist.Add("Eval_50up_rp00");
            rplist.Add("Eval_50up_rp01");
            rplist.Add("Eval_50up_rp02");
            rplist.Add("Eval_50up_rp03");

            var clist = new List<string>();
            clist.Add("E01");
            clist.Add("E06AA");
            clist.Add("E06AB");
            clist.Add("E08AA");
            clist.Add("E08AB");

            var msglist = new List<object>();

            foreach (var c in clist)
            {
                foreach (var rp in rplist)
                {
                    var ret = AllenWATLogic.PassFail(wafernum+c, rp, false);
                    if (!string.IsNullOrEmpty(ret.AppErrorMsg))
                    {
                        msglist.Add(new
                        {
                            container = wafernum+c,
                            rp = rp,
                            pname = "App Exception",
                            pval = ret.AppErrorMsg
                        });
                    }
                    else
                    {
                        msglist.Add(new
                        {
                            container = wafernum + c,
                            rp = rp,
                            pname = "WAT Result",
                            pval = ret.ResultMsg
                        });

                        foreach (var kv in ret.ValueCollect)
                        {
                            msglist.Add(new
                            {
                                container = wafernum + c,
                                rp = rp,
                                pname = kv.Key,
                                pval = kv.Value
                            });
                        }
                    }

                    msglist.Add(new
                    {
                        container = "",
                        rp = "",
                        pname = "",
                        pval = ""
                    });
                }//end foreach
            }//end foreach

            rplist = new List<string>();
            rplist.Add("Eval_COB_rp00");
            rplist.Add("Eval_COB_rp01");
            rplist.Add("Eval_COB_rp02");
            rplist.Add("Eval_COB_rp03");

            clist = new List<string>();
            clist.Add("E10");

            foreach (var c in clist)
            {
                foreach (var rp in rplist)
                {
                    var ret = AllenWATLogic.PassFail(wafernum + c, rp, false);
                    if (!string.IsNullOrEmpty(ret.AppErrorMsg))
                    {
                        msglist.Add(new
                        {
                            container = wafernum + c,
                            rp = rp,
                            pname = "App Exception",
                            pval = ret.AppErrorMsg
                        });
                    }
                    else
                    {
                        msglist.Add(new
                        {
                            container = wafernum + c,
                            rp = rp,
                            pname = "WAT Result",
                            pval = ret.ResultMsg
                        });

                        foreach (var kv in ret.ValueCollect)
                        {
                            msglist.Add(new
                            {
                                container = wafernum + c,
                                rp = rp,
                                pname = kv.Key,
                                pval = kv.Value
                            });
                        }
                    }

                    msglist.Add(new
                    {
                        container = "",
                        rp = "",
                        pname = "",
                        pval = ""
                    });
                }//end foreach
            }//end foreach

            var jret = new JsonResult();
            jret.MaxJsonLength = Int32.MaxValue;
            jret.Data = new
            {
                msglist = msglist
            };
            return jret;
        }


        public JsonResult GetAllenDCDName()
        {
            var dcdnamelist = AllenEVALData.GetAllenDCDName();
            var ret = new JsonResult();
            ret.Data = new {
                dcdnamelist = dcdnamelist
            };
            return ret;
        }


        public ActionResult WUXIWATLogic()
        { return View(); }

        public JsonResult WUXIWATLogicData()
        {
            var couponid = Request.Form["couponid"];
            var jstepname = Request.Form["jstepname"];

            var wxlogic = new WXLogic.WXWATLogic();
            var ret = wxlogic.WATPassFail(couponid, jstepname);

            var msglist = new List<object>();
            if (!string.IsNullOrEmpty(ret.AppErrorMsg))
            {
                msglist.Add(new
                {
                    pname = "App Exception",
                    pval = ret.AppErrorMsg
                });
            }
            else
            {
                msglist.Add(new
                {
                    pname = "Logic Result",
                    pval = ret.ResultReason
                });

                foreach (var kv in ret.ValueCollect)
                {
                    msglist.Add(new
                    {
                        pname = kv.Key,
                        pval = kv.Value
                    });
                }
            }

            var jret = new JsonResult();
            jret.MaxJsonLength = Int32.MaxValue;
            jret.Data = new
            {
                msglist = msglist,
                datatables = ret.DataTables
            };
            return jret;

        }

        public JsonResult GetWXCouponID()
        {
            var couponidlist = WXWATTestData.GetAllCouponID();
            var ret = new JsonResult();
            ret.Data = new
            {
                couponidlist = couponidlist
            };
            return ret;
        }

        public JsonResult GetWXJudgementStep()
        {
            var steplist = new List<string>();
            steplist.Add("POSTBIJUDGEMENT");
            steplist.Add("POSTHTOL1JUDGEMENT");
            steplist.Add("POSTHTOL2JUDGEMENT");

            var ret = new JsonResult();
            ret.Data = new
            {
                steplist = steplist
            };
            return ret;
        }

        public ActionResult WUXIWATDataManage()
        {
            return View();
        }

        public JsonResult WUXIWATDataMG()
        {
            var couponid = Request.Form["couponid"];
            var mgdata = WuxiWATData4MG.GetData(couponid,this);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new { mgdata = mgdata };
            return ret;
        }

        public JsonResult IgnoreWATDie()
        {

            var ignoredies = Request.Form["ignoredies"].Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var reason = Request.Form["reason"];
            var username = WXWATIgnoreDie.GetUserName(Request.UserHostName);

            foreach (var die in ignoredies)
            {
                var waferxy = die.Split(new string[] { "E", "e", "_" }, StringSplitOptions.RemoveEmptyEntries);
                if (waferxy.Length == 4)
                {
                    WXWATIgnoreDie.UpdateIgnoreDie(waferxy[0], waferxy[2], waferxy[3], reason, username);
                }
            }

            var ret = new JsonResult();
            ret.Data = new { sucess = true };
            return ret;
        }


        public ActionResult WATOGPData()
        {
            return View();
        }

        public JsonResult LoadOGPWafer()
        {
            var waferlist = DieSortVM.GetAllOGPWafers(this);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                waferlist = waferlist
            };
            return ret;
        }

        public JsonResult LoadOGPData()
        {
            var wafer = Request.Form["wafer"];
            var ogpdatalist = DieSortVM.GetOGPData(wafer, this);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                ogpdatalist = ogpdatalist
            };
            return ret;
        }

        public ActionResult WUXIWaferWAT()
        { return View(); }

        private object GetWuxiWaferWATRest(string coupongroup, string step)
        {
            var wxlogic = new WXLogic.WXWATLogic();
            var ret = wxlogic.WATPassFail(coupongroup, step);

            var stepdict = new Dictionary<string, string>();
            stepdict.Add("POSTBIJUDGEMENT", "(RP01)");
            stepdict.Add("POSTHTOL1JUDGEMENT", "(RP02)");
            stepdict.Add("POSTHTOL2JUDGEMENT", "(RP03)");

            var failcount = "";
            var couponstr = "";
            var failmode = "";
            var testtimes = "";
            var result = "";

            if (!string.IsNullOrEmpty(ret.AppErrorMsg))
            { result = ret.AppErrorMsg; }
            else
            {
                if (ret.TestPass)
                {
                    testtimes = ret.ValueCollect["readcount"];
                    result = "PASS";
                }
                else
                {
                    if (ret.ScrapIt)
                    {
                        failcount = ret.ValueCollect["failcount"];
                        couponstr = ret.ValueCollect["fail coupon string"].Replace("Fails:","");
                        testtimes = ret.ValueCollect["readcount"];
                        result = "SCRAP";
                    }
                    else
                    {
                        if (ret.NeedRetest)
                        {
                            failcount = ret.ValueCollect["failcount"];
                            couponstr = ret.ValueCollect["fail coupon string"].Replace("Fails:", "");
                            testtimes = ret.ValueCollect["readcount"];
                            result = "FAIL/RETESTABLE";
                        }
                        else
                        {
                            failcount = ret.ValueCollect["failcount"];
                            couponstr = ret.ValueCollect["fail coupon string"].Replace("Fails:", "");
                            testtimes = ret.ValueCollect["readcount"];
                            result = "FAIL/NON-RETESTABLE";
                        }
                    }//end else
                }//end else
            }

            if (ret.ValueCollect.ContainsKey("fail mode") && !string.IsNullOrEmpty(ret.ValueCollect["fail mode"]))
            {
                var mddict = new Dictionary<string, bool>();
                var unitstr = ret.ValueCollect["fail mode"].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var u in unitstr)
                {
                    var md = u.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[2];
                    if (!mddict.ContainsKey(md))
                    { mddict.Add(md, true); }
                }
                failmode = string.Join(",", mddict.Keys);
            }

            if (result.Contains("Fail to get wat prob filtered data"))
            { result = "Not Tested"; }
            if (stepdict.ContainsKey(step))
            { step += stepdict[step]; }

            return new
            {
                coupongroup = coupongroup,
                teststep = step,
                result = result,
                testtimes = testtimes,
                failcount = failcount,
                failmode = failmode,
                couponstr = couponstr
            };
        }

        public JsonResult WUXIWaferWATData()
        {
            var wafer = Request.Form["wafer"];
            var cdclist = new List<string>();
            cdclist.Add("E08");
            cdclist.Add("E07");
            cdclist.Add("E10");

            var steplist = new List<string>();
            steplist.Add("POSTBIJUDGEMENT");
            steplist.Add("POSTHTOL1JUDGEMENT");
            steplist.Add("POSTHTOL2JUDGEMENT");

            var reslist = new List<object>();

            foreach (var cdc in cdclist)
            {
                foreach (var step in steplist)
                {
                    reslist.Add(GetWuxiWaferWATRest(wafer+cdc,step));
                }
            }

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                reslist = reslist
            };
            return ret;

        }

        public ActionResult WUXIWATAnalyze()
        {
            return View();
        }

        public JsonResult WATAnalyzeParams()
        {
            var paramlist = new List<string>();
            paramlist.AddRange(WATAnalyzeVM.GetWATRawParamList());
            paramlist.AddRange(WATAnalyzeVM.GetWATLogicParamList());

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                paramlist = paramlist
            };
            return ret;
        }

        private JsonResult WATDataAnalyzeChart(string param, List<string> wflist, string rp, bool cmp
            , double lowlimit, double highlimit, double lowrange,double highrange,int rd,bool raw)
        {

            var xaxisdata = new List<string>();
            var boxlistdata = new List<object>();
            var outlierdata = new List<object>();
            var xidx = 0;

            Dictionary<string, List<XYVAL>> wuxiwatdatadict = new Dictionary<string, List<XYVAL>>();
            if (raw)
            { wuxiwatdatadict = WATAnalyzeVM.GetWUXIWATRawData(param, wflist, rp, lowrange, highrange);}
            else
            { wuxiwatdatadict = WATAnalyzeVM.GetWUXIWATLogicData(param, wflist, rp, lowrange, highrange); }
                
            var allwxwatdata = new List<XYVAL>();
            if (wuxiwatdatadict.Count > 1)
            {
                foreach (var kv in wuxiwatdatadict)
                { allwxwatdata.AddRange(kv.Value); }
                allwxwatdata.Sort(delegate (XYVAL obj1, XYVAL obj2) {
                    return obj1.Val.CompareTo(obj2.Val);
                });
                wuxiwatdatadict.Add("ALLDATA", allwxwatdata);
            }

            foreach (var kv in wuxiwatdatadict)
            {
                xaxisdata.Add("WUXI-" + kv.Key);

                var tmplist = new List<double>();
                foreach (var v in kv.Value)
                { tmplist.Add(v.Val); }

                var box = CBOXData.CBOXFromRaw(tmplist, lowlimit, highlimit);
                var boxrawdata = (CBOXData)box[0];
                var outlier = (List<VXVal>)box[1];

                boxlistdata.Add(new
                {
                    x = xidx,
                    low = Math.Round(boxrawdata.min, rd),
                    q1 = Math.Round(boxrawdata.lower, rd),
                    median = Math.Round(boxrawdata.mean, rd),
                    q3 = Math.Round(boxrawdata.upper, rd),
                    high = Math.Round(boxrawdata.max, rd),
                    color = "#0053a2"
                });

                foreach (var outitem in outlier)
                {
                    outlierdata.Add(new
                    {
                        x = Math.Round(xidx + outitem.x, 4),
                        y = Math.Round(outitem.ival, rd),
                        color = "#0053a2"
                    });
                }


                xidx++;
            }


            Dictionary<string, List<XYVAL>> allenrawdict = new Dictionary<string, List<XYVAL>>(); 
            if (cmp)
            {
                var allallenwatdata = new List<XYVAL>();
                if (raw)
                { allenrawdict = WATAnalyzeVM.GetALLENWATRawData(param, wflist, rp, lowrange, highrange); }
                else
                { allenrawdict = WATAnalyzeVM.GetALLENWATLogicData(param, wflist, rp, lowrange, highrange); }
                
                if (allenrawdict.Count > 0)
                {
                    if (allenrawdict.Count > 1)
                    {
                        foreach (var kv in allenrawdict)
                        { allallenwatdata.AddRange(kv.Value); }
                        allallenwatdata.Sort(delegate (XYVAL obj1, XYVAL obj2) {
                            return obj1.Val.CompareTo(obj2.Val);
                        });
                        allenrawdict.Add("ALLDATA", allallenwatdata);
                    }


                    foreach (var kv in allenrawdict)
                    {
                        xaxisdata.Add("ALLEN-" + kv.Key);

                        var tmplist = new List<double>();
                        foreach (var v in kv.Value)
                        { tmplist.Add(v.Val); }

                        var box = CBOXData.CBOXFromRaw(tmplist, lowlimit, highlimit);
                        var boxrawdata = (CBOXData)box[0];
                        var outlier = (List<VXVal>)box[1];

                        boxlistdata.Add(new
                        {
                            x = xidx,
                            low = Math.Round(boxrawdata.min, rd),
                            q1 = Math.Round(boxrawdata.lower, rd),
                            median = Math.Round(boxrawdata.mean, rd),
                            q3 = Math.Round(boxrawdata.upper, rd),
                            high = Math.Round(boxrawdata.max, rd),
                            color = "#d91919"
                        });

                        foreach (var outitem in outlier)
                        {
                            outlierdata.Add(new
                            {
                                x = Math.Round(xidx + outitem.x, 4),
                                y = Math.Round(outitem.ival, rd),
                                color = "#d91919"
                            });
                        }


                        xidx++;
                    }//end foreach
                }//end allendata
            }//end cmp

            if (wuxiwatdatadict.Count == 0 && allenrawdict.Count == 0)
            {
                var ret1 = new JsonResult();
                ret1.MaxJsonLength = Int32.MaxValue;
                ret1.Data = new
                {
                    sucess = false,
                    msg = "No data for paramter " + param
                };
                return ret1;
            }

            var id = param.Replace(" ", "_") + "_id";
            var title = param + " Distribution by Wafer";
            var xAxis = new
            {
                title = "Wafer#",
                data = xaxisdata
            };
            var yAxis = new { title = "Value" };
            var data = new
            {
                name = param,
                data = boxlistdata,
            };

            var labels = new List<object>();
            labels.Add(new
            {
                format = "<table><tr><td>LL:" + lowlimit + "</td></tr><tr><td>HL:" + highlimit + "</td></tr></table>",
                useHTML = true,
                point = new
                {
                    x = 0,
                    y = 0
                }
            });

            var boxdata = new
            {
                id = id,
                title = title,
                xAxis = xAxis,
                yAxis = yAxis,
                lowlimit = lowlimit,
                highlimit = highlimit,
                labels = labels,
                data = data,
                outlierdata = outlierdata
            };

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                sucess = true,
                boxdata = boxdata
            };
            return ret;
        }


        private JsonResult WATRawDistribution(string param, List<string> wflist, string rp,bool cmp)
        {
            var syscfg = CfgUtility.GetSysConfig(this);
            if (!syscfg.ContainsKey(param))
            {
                var ret1 = new JsonResult();
                ret1.MaxJsonLength = Int32.MaxValue;
                ret1.Data = new
                {
                    sucess = false,
                    msg = "No limit for RAW paramter "+param
                };
                return ret1;
            }

            var rawlimits = syscfg[param].Split(new string[] { ",", ";", ":" }, StringSplitOptions.RemoveEmptyEntries);
            var lowlimit = Models.UT.O2D(rawlimits[0]);
            var highlimit = Models.UT.O2D(rawlimits[1]);

            var rd = 5;
            if (param.Contains("BVR_LD_A"))
            { rd = 10; }


            var lowrange = lowlimit -  3.0* Math.Abs(highlimit -lowlimit);
            var highrange = highlimit + 3.0 * Math.Abs(highlimit - lowlimit);

            return WATDataAnalyzeChart(param, wflist, rp, cmp
            , lowlimit, highlimit, lowrange, highrange, rd, true);
        }

        private JsonResult WATLogicDistribution(string param, List<string> wflist, string rp,bool cmp)
        {
            var paramlimit = WATAnalyzeVM.GetWATSpecLimit(param,wflist);

            if (paramlimit.Count == 0)
            {
                var ret1 = new JsonResult();
                ret1.MaxJsonLength = Int32.MaxValue;
                ret1.Data = new
                {
                    sucess = false,
                    msg = "No limit for LOGIC paramter " + param
                };
                return ret1;
            }

            var lowlimit = paramlimit[0];
            var highlimit = paramlimit[1];

            var rd = 5;
            if (param.Contains("BVR_LD_A"))
            { rd = 10; }

            var lowrange = lowlimit - 3.0 * Math.Abs(highlimit - lowlimit);
            var highrange = highlimit + 3.0 * Math.Abs(highlimit - lowlimit);


            return WATDataAnalyzeChart(param, wflist, rp, cmp
            , lowlimit, highlimit, lowrange, highrange, rd, false);
        }


        private JsonResult WATDataSampleAnalyzeChart(string param, List<string> wxwflist,List<string> allenwflist, string rp
                    , double lowlimit, double highlimit, double lowrange, double highrange, int rd, bool raw)
        {

            var xaxisdata = new List<string>();
            var boxlistdata = new List<object>();
            var outlierdata = new List<object>();
            var xidx = 0;

            Dictionary<string, List<XYVAL>> wuxiwatdatadict = new Dictionary<string, List<XYVAL>>();
            if (raw)
            { wuxiwatdatadict = WATAnalyzeVM.GetWUXIWATRawData(param, wxwflist, rp, lowrange, highrange); }
            else
            { wuxiwatdatadict = WATAnalyzeVM.GetWUXIWATLogicData(param, wxwflist, rp, lowrange, highrange); }

            var allwxwatdata = new List<XYVAL>();
            if (wuxiwatdatadict.Count > 1)
            {
                foreach (var kv in wuxiwatdatadict)
                { allwxwatdata.AddRange(kv.Value); }
                allwxwatdata.Sort(delegate (XYVAL obj1, XYVAL obj2) {
                    return obj1.Val.CompareTo(obj2.Val);
                });
                wuxiwatdatadict.Add("ALLDATA", allwxwatdata);
            }

            foreach (var kv in wuxiwatdatadict)
            {
                xaxisdata.Add("WUXI-" + kv.Key);

                var tmplist = new List<double>();
                foreach (var v in kv.Value)
                { tmplist.Add(v.Val); }

                var box = CBOXData.CBOXFromRaw(tmplist, lowlimit, highlimit);
                var boxrawdata = (CBOXData)box[0];
                var outlier = (List<VXVal>)box[1];

                boxlistdata.Add(new
                {
                    x = xidx,
                    low = Math.Round(boxrawdata.min, rd),
                    q1 = Math.Round(boxrawdata.lower, rd),
                    median = Math.Round(boxrawdata.mean, rd),
                    q3 = Math.Round(boxrawdata.upper, rd),
                    high = Math.Round(boxrawdata.max, rd),
                    color = "#0053a2"
                });

                foreach (var outitem in outlier)
                {
                    outlierdata.Add(new
                    {
                        x = Math.Round(xidx + outitem.x, 4),
                        y = Math.Round(outitem.ival, rd),
                        color = "#0053a2"
                    });
                }


                xidx++;
            }

            var allallenwatdata = new List<XYVAL>();
            Dictionary<string, List<XYVAL>> allenrawdict = new Dictionary<string, List<XYVAL>>();

            {
                if (raw)
                { allenrawdict = WATAnalyzeVM.GetALLENWATRawData(param, allenwflist, rp, lowrange, highrange); }
                else
                { allenrawdict = WATAnalyzeVM.GetALLENWATLogicData(param, allenwflist, rp, lowrange, highrange); }

                if (allenrawdict.Count > 0)
                {
                    if (allenrawdict.Count > 1)
                    {
                        foreach (var kv in allenrawdict)
                        { allallenwatdata.AddRange(kv.Value); }
                        allallenwatdata.Sort(delegate (XYVAL obj1, XYVAL obj2) {
                            return obj1.Val.CompareTo(obj2.Val);
                        });
                        allenrawdict.Add("ALLDATA", allallenwatdata);
                    }

                    foreach (var kv in allenrawdict)
                    {
                        xaxisdata.Add("ALLEN-" + kv.Key);

                        var tmplist = new List<double>();
                        foreach (var v in kv.Value)
                        { tmplist.Add(v.Val); }

                        var box = CBOXData.CBOXFromRaw(tmplist, lowlimit, highlimit);
                        var boxrawdata = (CBOXData)box[0];
                        var outlier = (List<VXVal>)box[1];

                        boxlistdata.Add(new
                        {
                            x = xidx,
                            low = Math.Round(boxrawdata.min, rd),
                            q1 = Math.Round(boxrawdata.lower, rd),
                            median = Math.Round(boxrawdata.mean, rd),
                            q3 = Math.Round(boxrawdata.upper, rd),
                            high = Math.Round(boxrawdata.max, rd),
                            color = "#d91919"
                        });

                        foreach (var outitem in outlier)
                        {
                            outlierdata.Add(new
                            {
                                x = Math.Round(xidx + outitem.x, 4),
                                y = Math.Round(outitem.ival, rd),
                                color = "#d91919"
                            });
                        }


                        xidx++;
                    }//end foreach
                }//end allendata
            }

            if (wuxiwatdatadict.Count == 0 && allenrawdict.Count == 0)
            {
                var ret1 = new JsonResult();
                ret1.MaxJsonLength = Int32.MaxValue;
                ret1.Data = new
                {
                    sucess = false,
                    msg = "No data for paramter " + param
                };
                return ret1;
            }

            var id = param.Replace(" ", "_") + "_id";
            var title = param + " Distribution by Wafer";
            var xAxis = new
            {
                title = "Wafer#",
                data = xaxisdata
            };
            var yAxis = new { title = "Value" };
            var data = new
            {
                name = param,
                data = boxlistdata,
            };

            var labels = new List<object>();
            labels.Add(new
            {
                format = "<table><tr><td>LL:" + lowlimit + "</td></tr><tr><td>HL:" + highlimit + "</td></tr></table>",
                useHTML = true,
                point = new
                {
                    x = 0,
                    y = 0
                }
            });

            var boxdata = new
            {
                id = id,
                title = title,
                xAxis = xAxis,
                yAxis = yAxis,
                lowlimit = lowlimit,
                highlimit = highlimit,
                labels = labels,
                data = data,
                outlierdata = outlierdata
            };

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                sucess = true,
                boxdata = boxdata
            };
            return ret;
        }

        private JsonResult WATRawSampleCmp(string param, List<string> wxwflist, string rp)
        {
            var syscfg = CfgUtility.GetSysConfig(this);
            if (!syscfg.ContainsKey(param))
            {
                var ret1 = new JsonResult();
                ret1.MaxJsonLength = Int32.MaxValue;
                ret1.Data = new
                {
                    sucess = false,
                    msg = "No limit for RAW paramter " + param
                };
                return ret1;
            }

            var rawlimits = syscfg[param].Split(new string[] { ",", ";", ":" }, StringSplitOptions.RemoveEmptyEntries);
            var lowlimit = WAT.Models.UT.O2D(rawlimits[0]);
            var highlimit = WAT.Models.UT.O2D(rawlimits[1]);

            var rd = 5;
            if (param.Contains("BVR_LD_A"))
            { rd = 10; }


            var lowrange = lowlimit - 3.0 * Math.Abs(highlimit - lowlimit);
            var highrange = highlimit + 3.0 * Math.Abs(highlimit - lowlimit);

            var allenwflist = WATAnalyzeVM.GetE08SampleWaferList(wxwflist,16);

            return WATDataSampleAnalyzeChart(param, wxwflist, allenwflist, rp
                    , lowlimit, highlimit, lowrange, highrange, rd, true);
        }

        private JsonResult WATLogicSampleCmp(string param, List<string> wxwflist, string rp)
        {
            var paramlimit = WATAnalyzeVM.GetWATSpecLimit(param, wxwflist);

            if (paramlimit.Count == 0)
            {
                var ret1 = new JsonResult();
                ret1.MaxJsonLength = Int32.MaxValue;
                ret1.Data = new
                {
                    sucess = false,
                    msg = "No limit for LOGIC paramter " + param
                };
                return ret1;
            }

            var lowlimit = paramlimit[0];
            var highlimit = paramlimit[1];

            var rd = 5;
            if (param.Contains("BVR_LD_A"))
            { rd = 10; }

            var lowrange = lowlimit - 3.0 * Math.Abs(highlimit - lowlimit);
            var highrange = highlimit + 3.0 * Math.Abs(highlimit - lowlimit);

            var allenwflist = WATAnalyzeVM.GetE08SampleWaferList(wxwflist,10);

            return WATDataSampleAnalyzeChart(param, wxwflist, allenwflist, rp
                    , lowlimit, highlimit, lowrange, highrange, rd, false);
        }

        public JsonResult WUXIWATAnalyzeData()
        {
            var param = Request.Form["param"].Trim();
            var wafers = Request.Form["wafers"];
            var rp = Request.Form["rp"];
            var act = Request.Form["act"];

            var wfdict = new Dictionary<string, bool>();
            var wfarray = wafers.Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (var item in wfarray)
            {
                if (!wfdict.ContainsKey(item.Trim()))
                {
                    wfdict.Add(item.Trim(), true);
                }
            }
            var wflist = wfdict.Keys.ToList();
            

            var rawlist = WATAnalyzeVM.GetWATRawParamList();
            var logiclist = WATAnalyzeVM.GetWATLogicParamList();

            if (rawlist.Contains(param))
            {
                if (act.Contains("distrib"))
                { return WATRawDistribution(param, wflist, rp, false); }
                else if (act.Contains("cmp"))
                { return WATRawDistribution(param, wflist, rp, true); }
                else
                { return WATRawSampleCmp(param, wflist, rp); }
            }
            else if (logiclist.Contains(param))
            {
                if (act.Contains("distrib"))
                { return WATLogicDistribution(param.Replace("LOGIC", rp), wflist, rp, false); }
                else if (act.Contains("cmp"))
                { return WATLogicDistribution(param.Replace("LOGIC", rp), wflist, rp, true); }
                else
                { return WATLogicSampleCmp(param.Replace("LOGIC", rp), wflist, rp); }
            }
            else
            {
                var ret = new JsonResult();
                ret.MaxJsonLength = Int32.MaxValue;
                ret.Data = new
                {
                    sucess = false,
                    msg = "Parameter "+param+"is not supported!"
                };
                return ret;
            }
        }


    }
}
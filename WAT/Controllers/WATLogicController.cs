using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using WAT.Models;
using WXLogic;
using MathNet.Numerics.Statistics;

namespace WAT.Controllers
{
    public class WATLogicController : Controller
    {

        private ActionResult Jump2Welcome(string url)
        {
            var dict = new RouteValueDictionary();
            dict.Add("url", url);
            return RedirectToAction("Welcome", "Main", dict);
        }

        private bool CheckName(string ip, string url)
        {
            var machinename = MachineUserMap.GetUserMachineName(ip);
            if (machinename.Count == 0)
            { return false; }
            else
            {
                MachineUserMap.LoginLog(machinename[0], machinename[1], url);
                return true;
            }
        }


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

            var url = "/WATLogic/VerifyAllenLogic";
            if (!CheckName(Request.UserHostName, url))
            { return Jump2Welcome(url); }

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
            var url = "/WATLogic/AllenWaferWAT";
            if (!CheckName(Request.UserHostName, url))
            { return Jump2Welcome(url); }

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


        public ActionResult WUXIWATLogic(string wafer,string rp)
        {
            var url = "/WATLogic/WUXIWATLogic";
            if (!CheckName(Request.UserHostName, url))
            { return Jump2Welcome(url); }

            ViewBag.wafer = "";
            ViewBag.jstepname = "";
            if (!string.IsNullOrEmpty(wafer) && !string.IsNullOrEmpty(rp))
            {
                var jstepname = WATAnalyzeVM.ConvertRP2JudgementStep(rp);
                if (!jstepname.Contains("PRE"))
                {
                    ViewBag.wafer = wafer;
                    ViewBag.jstepname = jstepname;
                }
            }
            return View();
        }

        public JsonResult WUXIWATLogicData()
        {
            var couponid = Request.Form["couponid"];
            var jstepname = Request.Form["jstepname"];
            var r100 = Request.Form["r100"];

            var wxlogic = new WXLogic.WXWATLogic();
            
            var ret = new WXLogic.WXWATLogic();
            if(r100.Contains("FALSE"))
            {
                wxlogic.AllowToMoveMapFile = true;
                ret = wxlogic.WATPassFail(couponid, jstepname);
            }
            else
            {
                //wxlogic.AllowToMoveMapFile = false;
                //ret = wxlogic.WATPassFail100(couponid, jstepname);
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
            steplist.Add("POSTHTOL3JUDGEMENT");
            steplist.Add("POSTHTOL4JUDGEMENT");

            var ret = new JsonResult();
            ret.Data = new
            {
                steplist = steplist
            };
            return ret;
        }

        public ActionResult WUXIWATDataManage(string wafer)
        {
            var url = "/WATLogic/WUXIWATDataManage";
            if (!CheckName(Request.UserHostName, url))
            { return Jump2Welcome(url); }

            ViewBag.wafer = "";
            if (!string.IsNullOrEmpty(wafer))
            { ViewBag.wafer = wafer; }
            
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
                var waferxy = die.ToUpper().Split(new string[] { "E","R","T","_" }, StringSplitOptions.RemoveEmptyEntries);
                if (waferxy.Length == 5)
                {
                    var coupon = die.ToUpper().Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries)[0];
                    WXWATIgnoreDie.UpdateIgnoreDie(waferxy[0], waferxy[2], waferxy[3], reason, username, coupon+":::"+ waferxy[4]);
                }
            }

            var ret = new JsonResult();
            ret.Data = new { sucess = true };
            return ret;
        }

        public JsonResult IgnoreWATDiePicture()
        {
            var ignoredies = Request.Form["igid"];
            var waferxy = ignoredies.ToUpper().Split(new string[] { "E", "R", "T", "_" }, StringSplitOptions.RemoveEmptyEntries);
            if (waferxy.Length == 5)
            {
                try
                {
                    foreach (string fl in Request.Files)
                    {
                        if (fl != null && Request.Files[fl].ContentLength > 0)
                        {
                            string fn = Path.GetFileName(Request.Files[fl].FileName).Replace("#", "")
                                .Replace("&", "").Replace("?", "").Replace("%", "").Replace("+", "");
                            if (!fn.ToUpper().Contains(".PNG") && !fn.ToUpper().Contains(".JPG") && !fn.ToUpper().Contains(".BMP"))
                            { continue; }

                            string datestring = DateTime.Now.ToString("yyyyMMdd");
                            string imgdir = Server.MapPath("~/userfiles") + "\\docs\\" + datestring + "\\";

                            if (!Directory.Exists(imgdir))
                            {
                                Directory.CreateDirectory(imgdir);
                            }

                            fn = Path.GetFileNameWithoutExtension(fn) + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(fn);
                            Request.Files[fl].SaveAs(imgdir + fn);
                            var url = "/userfiles/docs/" + datestring + "/" + fn;

                            WXWATIgnoreDie.UpdateIgnoreDiePicture(waferxy[0], waferxy[2], waferxy[3], url);
                        }
                    }

                }
                catch (Exception ex)
                {}
            }

            var ret = new JsonResult();
            ret.Data = new { sucess = true };
            return ret;
        }

        public ActionResult WATOGPData()
        {
            var url = "/WATLogic/WATOGPData";
            if (!CheckName(Request.UserHostName, url))
            { return Jump2Welcome(url); }

            return View();
        }

        public JsonResult LoadOGPWafer()
        {
            var waferlist = new List<string>();

            var dbret = WuxiWATData4MG.GetWUXIWATWaferStepData();
            foreach (var line in dbret)
            {
                var wafer = Models.UT.O2S(line[0]);
                if (wafer.Length > 1 && !waferlist.Contains(wafer))
                {
                    waferlist.Add(wafer);
                }
            }

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
            var ogpdatalist = WATOGPVM.GetOGPData(wafer, this);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                ogpdatalist = ogpdatalist
            };
            return ret;
        }

        public ActionResult WUXIWaferWAT()
        {
            var url = "/WATLogic/WUXIWaferWAT";
            if (!CheckName(Request.UserHostName, url))
            { return Jump2Welcome(url); }

            return View();
        }

        private object GetWuxiWaferWATRest(string coupongroup, string step)
        {
            var wxlogic = new WXLogic.WXWATLogic();
            wxlogic.AllowToMoveMapFile = false;
            var ret = wxlogic.WATPassFail(coupongroup, step);

            var stepdict = new Dictionary<string, string>();
            stepdict.Add("POSTBIJUDGEMENT", "(RP01)");
            stepdict.Add("POSTHTOL1JUDGEMENT", "(RP02)");
            stepdict.Add("POSTHTOL2JUDGEMENT", "(RP03)");
            stepdict.Add("POSTHTOL3JUDGEMENT", "(RP04)");
            stepdict.Add("POSTHTOL4JUDGEMENT", "(RP05)");

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
            var wattype = WuxiWATData4MG.GetWATType(wafer);

            if (string.IsNullOrEmpty(wattype))
            {
                var reslist1 = new List<object>();
                reslist1.Add( new
                {
                    coupongroup = wafer,
                    teststep = "",
                    result = "Your input should be wafernum with E08/E09,eg:192323-80E08",
                    testtimes = "",
                    failcount = "",
                    failmode = "",
                    couponstr = ""
                });

                var ret1 = new JsonResult();
                ret1.MaxJsonLength = Int32.MaxValue;
                ret1.Data = new
                {
                    reslist = reslist1
                };
                return ret1;
            }

            var wf = wafer.Split(new string[] { "E","R","T" }, StringSplitOptions.RemoveEmptyEntries)[0];

            var cdclist = new List<string>();
            cdclist.Add(wattype);
            var E10Str = wattype.Replace("08", "10").Replace("09", "10");
            cdclist.Add(E10Str);

            var steplist = new List<string>();
            if (wattype.Contains("08"))
            {
                steplist.Add("POSTBIJUDGEMENT");
                steplist.Add("POSTHTOL1JUDGEMENT");
                steplist.Add("POSTHTOL2JUDGEMENT");
            }
            else if (wattype.Contains("09"))
            {
                steplist.Add("POSTBIJUDGEMENT");
                steplist.Add("POSTHTOL1JUDGEMENT");
                steplist.Add("POSTHTOL2JUDGEMENT");
                steplist.Add("POSTHTOL3JUDGEMENT");
                steplist.Add("POSTHTOL4JUDGEMENT");
            }

            var reslist = new List<object>();

            foreach (var cdc in cdclist)
            {
                foreach (var step in steplist)
                {
                    if (cdc.Contains(E10Str)
                        && (step.Contains("POSTHTOL3JUDGEMENT") || step.Contains("POSTHTOL4JUDGEMENT")))
                    { continue; }

                    reslist.Add(GetWuxiWaferWATRest(wf+cdc,step));
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
            var url = "/WATLogic/WUXIWATAnalyze";
            if (!CheckName(Request.UserHostName, url))
            { return Jump2Welcome(url); }

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
            var title = param +"_"+rp+ " Distribution by Wafer";
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
            param = WATAnalyzeVM.RealParam(param);
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

        public ActionResult WUXIWATXY(string param,string wafer,string rp)
        {
            var url = "/WATLogic/WUXIWATXY";
            if (!CheckName(Request.UserHostName, url))
            { return Jump2Welcome(url); }

            ViewBag.param = "";
            if (!string.IsNullOrEmpty(param))
            { ViewBag.param = param; }
            ViewBag.wafer = "";
            if (!string.IsNullOrEmpty(wafer))
            { ViewBag.wafer = wafer; }
            ViewBag.rp = "";
            if (!string.IsNullOrEmpty(rp))
            { ViewBag.rp = rp; }

            return View();
        }

        private JsonResult WATDataXYChart(string param, List<string> wflist, string rp
    , double lowlimit, double highlimit, double lowrange, double highrange, int rd, Dictionary<string, bool> vcselxy, bool raw)
        {
            var dvf = false;
            var pold = false;
            if (param.ToUpper().Contains("VF_LD_V_AD_REF"))
            { dvf = true; }
            else if (param.ToUpper().Contains("PO_LD_W"))
            { pold = true; }


            Dictionary<string, List<XYVAL>> wuxiwatdatadict = new Dictionary<string, List<XYVAL>>();
            if (raw)
            { wuxiwatdatadict = WATAnalyzeVM.GetWUXIWATRawData(param, wflist, rp, lowrange, highrange); }
            else
            { wuxiwatdatadict = WATAnalyzeVM.GetWUXIWATLogicData(param, wflist, rp, lowrange, highrange); }

            if (wuxiwatdatadict.Count == 0)
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

            var wuxiwatdata = wuxiwatdatadict.Values.First();
            var valdict = new Dictionary<string, double>();
            foreach (var xyval in wuxiwatdata)
            {
                var k = xyval.X + ":::" + xyval.Y;
                if (!valdict.ContainsKey(k))
                { valdict.Add(k, xyval.Val); }
            }

            var vallist = new List<double>();
            var sample = new List<object>();
            var xlist = new List<int>();
            var ylist = new List<int>();
            var xlistdict = new Dictionary<int, bool>();
            var ylistdict = new Dictionary<int, bool>();

            foreach (var kv in vcselxy)
            {
                var xystr = kv.Key.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                var X = Models.UT.O2I(xystr[0]);
                var Y = Models.UT.O2I(xystr[1]);
                if (valdict.ContainsKey(kv.Key))
                {
                    vallist.Add(valdict[kv.Key]);

                    if (dvf && valdict[kv.Key] > 0.05)
                    {
                        var templist = new List<object>();
                        templist.Add(X);
                        templist.Add(Y);
                        templist.Add(0.8);
                        sample.Add(templist);
                    }
                    else if (pold && valdict[kv.Key] < 0.0008)
                    {
                        var templist = new List<object>();
                        templist.Add(X);
                        templist.Add(Y);
                        templist.Add(-0.01);
                        sample.Add(templist);
                    }
                    else
                    {
                        var templist = new List<object>();
                        templist.Add(X);
                        templist.Add(Y);
                        templist.Add(valdict[kv.Key]);
                        sample.Add(templist);
                    }
                }
                else
                {
                    var templist = new List<object>();
                    templist.Add(X);
                    templist.Add(Y);
                    templist.Add(null);
                    sample.Add(templist);
                }

                if (!xlistdict.ContainsKey(X))
                { xlistdict.Add(X,true); }
                if (!ylistdict.ContainsKey(Y))
                { ylistdict.Add(Y, true); }

            }

            vallist.Sort();
            xlist.AddRange(xlistdict.Keys);
            ylist.AddRange(ylistdict.Keys);
            xlist.Sort();
            ylist.Sort();


           var serial = new List<object>();
            serial.Add(new
            {
                type = "heatmap",
                name = "Sample",
                data = sample,
                boostThreshold = 100,
                borderWidth = 0,
                nullColor = "#e5e5e5",//"#EFEFEF",
                turboThreshold = 100,
                visible = true
            });


            var stops = new List<object>();
            var clist = new string[] { "#0000ff","#00ffff","#00ff00","#ffff00","#ff0000"}.ToList();
            if (pold) { clist.Reverse(); }
            var val = 0.0;
            foreach (var c in clist)
            {
                var colorlist = new List<object>();
                colorlist.Add(val);colorlist.Add(c);
                stops.Add(colorlist);
                val += 0.2;
            }
            
            var id = param.Replace(" ", "_") + "_id";
            var title = param + "_" + rp + " Distribution by Wafer";
            var xydata = new
            {
                id = id,
                title = title,
                serial = serial,
                xmax = xlist[xlist.Count - 1] + 10,
                ymax = ylist[ylist.Count - 1] + 10,
                datamin = vallist[0],
                datamax = vallist[vallist.Count - 1],
                stops = stops
            };


            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                sucess = true,
                xydata = xydata
            };
            return ret;
        }


        private JsonResult WATRawXY(string param, List<string> wflist, string rp,Dictionary<string,bool> vcselxy)
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
            var lowlimit = Models.UT.O2D(rawlimits[0]);
            var highlimit = Models.UT.O2D(rawlimits[1]);

            var rd = 5;
            if (param.Contains("BVR_LD_A"))
            { rd = 10; }


            var lowrange = lowlimit - 3.0 * Math.Abs(highlimit - lowlimit);
            var highrange = highlimit + 3.0 * Math.Abs(highlimit - lowlimit);

            return WATDataXYChart(param, wflist, rp
            , lowlimit, highlimit, lowrange, highrange, rd, vcselxy, true);
        }

        private JsonResult WATLogicXY(string param, List<string> wflist, string rp, Dictionary<string, bool> vcselxy)
        {
            var paramlimit = WATAnalyzeVM.GetWATSpecLimit(param, wflist);

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


            return WATDataXYChart(param, wflist, rp
            , lowlimit, highlimit, lowrange, highrange, rd, vcselxy, false);
        }

        public JsonResult WUXIWATXYDATA()
        {
            var param = Request.Form["param"].Trim();
            param = WATAnalyzeVM.RealParam(param);
            var wafernum = Request.Form["wafernum"].Trim().ToUpper();
            var rp = Request.Form["rp"];

            if ((CCT(wafernum, "08") || CCT(wafernum, "10"))
               && (rp.Contains("RP04") || rp.Contains("RP05")))
            {
                var ret = new JsonResult();
                ret.MaxJsonLength = Int32.MaxValue;
                ret.Data = new
                {
                    sucess = false,
                    msg = "E08/E10 not support RP04/RP05!"
                };
                return ret;
            }

            var vcselxy = WATAnalyzeVM.GetWaferCoordinate(wafernum.Split(new string[] { "E", "R", "T" }, StringSplitOptions.RemoveEmptyEntries)[0], this);
            if (vcselxy.Count == 0)
            {
                var ret = new JsonResult();
                ret.MaxJsonLength = Int32.MaxValue;
                ret.Data = new
                {
                    sucess = false,
                    msg = "Wafer " + wafernum + "has no coordination information!"
                };
                return ret;
            }

            var rawlist = WATAnalyzeVM.GetWATRawParamList();
            var logiclist = WATAnalyzeVM.GetWATLogicParamList();
            var wflist = new List<string>();
            wflist.Add(wafernum);

            if (rawlist.Contains(param))
            {
                return WATRawXY(param, wflist, rp, vcselxy);
            }
            else if (logiclist.Contains(param))
            {
                return WATLogicXY(param.Replace("LOGIC", rp), wflist, rp, vcselxy);
            }
            else
            {
                var ret = new JsonResult();
                ret.MaxJsonLength = Int32.MaxValue;
                ret.Data = new
                {
                    sucess = false,
                    msg = "Parameter " + param + "is not supported!"
                };
                return ret;
            }
        }


        public ActionResult ALLENWATXY()
        {
            return View();
        }

        public JsonResult ALLENWATXYDATA() {

            var wafernum = Request.Form["wafernum"].Trim().ToUpper();
            var vcselxy = WATAnalyzeVM.GetWaferCoordinate(wafernum.Split(new string[] { "E", "R", "T" }, StringSplitOptions.RemoveEmptyEntries)[0], this);

            var allenwatdata = new List<XYVAL>();

            var sql = @"select distinct X,Y from [EngrData].[insite].[Get_EVAL_Data_By_CONTAINER_NUMBER_Link_PROBE_DATA] 
                      where Left(CONTAINER_NUMBER,9) = @wafer and (CONTAINER_NUMBER like '%E01%' 
                      or CONTAINER_NUMBER like '%E06%' or CONTAINER_NUMBER like '%E08%') ";
            var dict = new Dictionary<string, string>();
            dict.Add("@wafer", wafernum);

            var dbret = Models.DBUtility.ExeAllenSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                var tmpvm = new XYVAL();
                tmpvm.Val = 9;
                tmpvm.X = Models.UT.O2I(line[0]).ToString();
                tmpvm.Y = Models.UT.O2I(line[1]).ToString();
                allenwatdata.Add(tmpvm);
            }

            if (allenwatdata.Count == 0)
            {
                var ret1 = new JsonResult();
                ret1.MaxJsonLength = Int32.MaxValue;
                ret1.Data = new
                {
                    sucess = false,
                    msg = "No data for allen wat test "
                };
                return ret1;
            }

            var valdict = new Dictionary<string, double>();
            foreach (var xyval in allenwatdata)
            {
                var k = xyval.X + ":::" + xyval.Y;
                if (!valdict.ContainsKey(k))
                { valdict.Add(k, xyval.Val); }
            }

            var data = new List<List<object>>();
            var xlist = new List<int>();
            var ylist = new List<int>();
            var xlistdict = new Dictionary<int, bool>();
            var ylistdict = new Dictionary<int, bool>();

            foreach (var kv in vcselxy)
            {
                var xystr = kv.Key.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                var X = Models.UT.O2I(xystr[0]);
                var Y = Models.UT.O2I(xystr[1]);
                if (valdict.ContainsKey(kv.Key))
                {
                    var templist = new List<object>();
                    templist.Add(X);
                    templist.Add(Y);
                    templist.Add(valdict[kv.Key]);
                    data.Add(templist);
                }
                else
                {
                    var templist = new List<object>();
                    templist.Add(X);
                    templist.Add(Y);
                    templist.Add(null);
                    data.Add(templist);
                }

                if (!xlistdict.ContainsKey(X))
                { xlistdict.Add(X, true); }
                if (!ylistdict.ContainsKey(Y))
                { ylistdict.Add(Y, true); }

            }

            xlist.AddRange(xlistdict.Keys);
            ylist.AddRange(ylistdict.Keys);
            xlist.Sort();
            ylist.Sort();


            var serial = new List<object>();
            serial.Add(new
            {
                name = "Die Sort",
                data = data,
                boostThreshold = 100,
                borderWidth = 0,
                nullColor = "#acacac",//"#EFEFEF",
                turboThreshold = 100
            });


            var id ="Allen_wat_id";
            var title = "Allen WAT DIE Distribution by Wafer";
            var xydata = new
            {
                id = id,
                title = title,
                serial = serial,
                xmax = xlist[xlist.Count - 1] + 10,
                ymax = ylist[ylist.Count - 1] + 10,
                datamin = 1,
                datamax = 10
            };


            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                sucess = true,
                xydata = xydata
            };
            return ret;
        }


        public JsonResult LoadLocalOGPWafer()
        {
            var waferlist = WATOGPVM.GetLocalOGPXYWafer();
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                waferlist = waferlist
            };
            return ret;
        }

        public ActionResult WUXIWATDistribution()
        {
            return View();
        }

        public JsonResult WUXIWATDistributionDATA()
        {
            var coupongroup = Request.Form["wafernum"].Trim().ToUpper();
            var wafer = coupongroup.Split(new string[] { "E", "R", "T" }, StringSplitOptions.RemoveEmptyEntries)[0];
            var vcselxy = WATAnalyzeVM.GetWaferCoordinate(wafer, this);

            if (vcselxy.Count == 0)
            {
                var ret1 = new JsonResult();
                ret1.MaxJsonLength = Int32.MaxValue;
                ret1.Data = new
                {
                    sucess = false,
                    msg = "WAFER "+wafer+" HAS NO MAP FILE!"
                };
                return ret1;
            }

            var wxwatdata = new List<XYVAL>();

            var array = Models.WXEvalPN.GetLocalWaferArray(wafer);//WXLogic.WATSampleXY.GetArrayFromAllenSherman(wafer);
            var arraysize = Models.UT.O2I(array);
            var xylist = WXLogic.WATSampleXY.LoadOGPFromLocalDBByWafer(coupongroup, arraysize, "");

            foreach (var s in xylist)
            {
                var tmpvm = new XYVAL();
                tmpvm.Val = 9;
                tmpvm.X = Models.UT.O2I(s.X).ToString();
                tmpvm.Y = Models.UT.O2I(s.Y).ToString();
                wxwatdata.Add(tmpvm);
            }

            if (wxwatdata.Count == 0)
            {
                var ret1 = new JsonResult();
                ret1.MaxJsonLength = Int32.MaxValue;
                ret1.Data = new
                {
                    sucess = false,
                    msg = "No data for allen wat test "
                };
                return ret1;
            }

            var valdict = new Dictionary<string, double>();
            foreach (var xyval in wxwatdata)
            {
                var k = xyval.X + ":::" + xyval.Y;
                if (!valdict.ContainsKey(k))
                { valdict.Add(k, xyval.Val); }
            }

            var data = new List<List<object>>();
            var xlist = new List<int>();
            var ylist = new List<int>();
            var xlistdict = new Dictionary<int, bool>();
            var ylistdict = new Dictionary<int, bool>();

            foreach (var kv in vcselxy)
            {
                var xystr = kv.Key.Split(new string[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
                var X = Models.UT.O2I(xystr[0]);
                var Y = Models.UT.O2I(xystr[1]);
                if (valdict.ContainsKey(kv.Key))
                {
                    var templist = new List<object>();
                    templist.Add(X);
                    templist.Add(Y);
                    templist.Add(valdict[kv.Key]);
                    data.Add(templist);
                }
                else
                {
                    var templist = new List<object>();
                    templist.Add(X);
                    templist.Add(Y);
                    templist.Add(null);
                    data.Add(templist);
                }

                if (!xlistdict.ContainsKey(X))
                { xlistdict.Add(X, true); }
                if (!ylistdict.ContainsKey(Y))
                { ylistdict.Add(Y, true); }

            }

            xlist.AddRange(xlistdict.Keys);
            ylist.AddRange(ylistdict.Keys);
            xlist.Sort();
            ylist.Sort();


            var serial = new List<object>();
            serial.Add(new
            {
                name = "Die Sort",
                data = data,
                boostThreshold = 100,
                borderWidth = 0,
                nullColor = "#acacac",//"#EFEFEF",
                turboThreshold = 100
            });


            var id = "Allen_wat_id";
            var title = "WUXI WAT DIE Distribution by Wafer";
            var xydata = new
            {
                id = id,
                title = title,
                serial = serial,
                xmax = xlist[xlist.Count - 1] + 10,
                ymax = ylist[ylist.Count - 1] + 10,
                datamin = 1,
                datamax = 10
            };


            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                sucess = true,
                xydata = xydata
            };
            return ret;
        }


        public ActionResult WUXIWATCoupon(string param, string wafer, string rp)
        {
            var url = "/WATLogic/WUXIWATCoupon";
            if (!CheckName(Request.UserHostName, url))
            { return Jump2Welcome(url); }

            ViewBag.param = "";
            if (!string.IsNullOrEmpty(param))
            { ViewBag.param = param; }
            ViewBag.wafer = "";
            if (!string.IsNullOrEmpty(wafer))
            { ViewBag.wafer = wafer; }
            ViewBag.rp = "";
            if (!string.IsNullOrEmpty(rp))
            { ViewBag.rp = rp; }

            return View();
        }


        private JsonResult WATDataCouponChart(string param, List<string> wflist, string rp
    , double lowlimit, double highlimit, double lowrange, double highrange, int rd , bool raw)
        {
            Dictionary<string, List<XYVAL>> wuxiwatdatadict = new Dictionary<string, List<XYVAL>>();
            if (raw)
            { wuxiwatdatadict = WATAnalyzeVM.GetWUXIWATRawData(param, wflist, rp, lowrange, highrange); }
            else
            { wuxiwatdatadict = WATAnalyzeVM.GetWUXIWATLogicData(param, wflist, rp, lowrange, highrange); }

            if (wuxiwatdatadict.Count == 0)
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

            var waferunitdict = new Dictionary<string, bool>();

            var wafercat = new List<object>();

            var datalist = new List<double>();
            foreach (var kv in wuxiwatdatadict)
            {
                var vallist = kv.Value;
                vallist.Sort(delegate (XYVAL obj1, XYVAL obj2)
                {
                    return obj1.unit.CompareTo(obj2.unit);
                });

                var cplist = new List<string>();
                var cpchdict = new Dictionary<string, List<string>>();

                foreach (var val in vallist)
                {
                    var waferunit = kv.Key + ":" + val.unit;
                    if (!waferunitdict.ContainsKey(waferunit))
                    {
                        waferunitdict.Add(waferunit, true);
                        datalist.Add(val.Val);

                        var cp = (val.unit / 10000).ToString();
                        var ch = (val.unit % 10000).ToString();
                        if (!cplist.Contains(cp))
                        { cplist.Add(cp); }

                        if (cpchdict.ContainsKey(cp))
                        { cpchdict[cp].Add(ch); }
                        else
                        {
                            var templist = new List<string>();
                            templist.Add(ch);
                            cpchdict.Add(cp, templist);
                        }
                    }
                }

                var cpcat = new List<object>();
                foreach (var cpp in cplist)
                {
                    cpcat.Add(new
                    {
                        name = cpp,
                        categories = cpchdict[cpp]
                    });
                }

                wafercat.Add(new
                {
                    name = kv.Key,
                    categories = cpcat
                });
            }

            var id = param.Replace(" ", "_") + "_id";
            var title = param + " Coupon Distribution";

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

            var coupondata = new
            {
                id = id,
                title = title,
                categories = wafercat,
                lowlimit = lowlimit,
                highlimit = highlimit,
                labels = labels,
                datalist = datalist
            };

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                sucess = true,
                coupondata = coupondata
            };
            return ret;
        }


        private JsonResult WATRawCoupon(string param, List<string> wflist, string rp)
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
            var lowlimit = Models.UT.O2D(rawlimits[0]);
            var highlimit = Models.UT.O2D(rawlimits[1]);

            var rd = 5;
            if (param.Contains("BVR_LD_A"))
            { rd = 10; }


            var lowrange = lowlimit - 3.0 * Math.Abs(highlimit - lowlimit);
            var highrange = highlimit + 3.0 * Math.Abs(highlimit - lowlimit);

            return WATDataCouponChart(param, wflist, rp
            , lowlimit, highlimit, lowrange, highrange, rd, true);
        }

        private JsonResult WATLogicCoupon(string param, List<string> wflist, string rp)
        {
            var paramlimit = WATAnalyzeVM.GetWATSpecLimit(param, wflist);

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


            return WATDataCouponChart(param, wflist, rp
            , lowlimit, highlimit, lowrange, highrange, rd, false);
        }

        private bool CCT(string coupon, string type)
        {
            var keylist = new List<string>(new string[] { "E", "R", "T" });
            foreach (var k in keylist)
            {
                if (coupon.Contains(k + type))
                { return true; }
            }
            return false;
        }

        public JsonResult WUXIWATCouponData()
        {
            var param = Request.Form["param"].Trim();
            param = WATAnalyzeVM.RealParam(param);
            var wafers = Request.Form["wafers"];
            var rp = Request.Form["rp"];

            if ((CCT(wafers, "08") || CCT(wafers, "10")) 
                && (rp.Contains("RP04") || rp.Contains("RP05")))
            {
                var ret = new JsonResult();
                ret.MaxJsonLength = Int32.MaxValue;
                ret.Data = new
                {
                    sucess = false,
                    msg = "E08/E10 not support RP04/RP05!"
                };
                return ret;
            }

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
                return WATRawCoupon(param, wflist, rp);
            }
            else if (logiclist.Contains(param))
            {
                return WATLogicCoupon(param.Replace("LOGIC", rp), wflist, rp);
            }
            else
            {
                var ret = new JsonResult();
                ret.MaxJsonLength = Int32.MaxValue;
                ret.Data = new
                {
                    sucess = false,
                    msg = "Parameter " + param + "is not supported!"
                };
                return ret;
            }
        }

        public ActionResult WUXIWATPvsP(string xparam, string yparam, string wafer, string rp)
        {
            var url = "/WATLogic/WUXIWATPvsP";
            if (!CheckName(Request.UserHostName, url))
            { return Jump2Welcome(url); }


            ViewBag.xparam = "";
            if (!string.IsNullOrEmpty(xparam))
            { ViewBag.xparam = xparam; }

            ViewBag.yparam = "";
            if (!string.IsNullOrEmpty(yparam))
            { ViewBag.yparam = yparam; }

            ViewBag.wafer = "";
            if (!string.IsNullOrEmpty(wafer))
            { ViewBag.wafer = wafer; }

            ViewBag.rp = "";
            if (!string.IsNullOrEmpty(rp))
            { ViewBag.rp = rp; }

            return View();
        }

        private List<object> GetWXWATTestData(string param,string rp,List<string> wflist,List<string> rawlist,List<string> logiclist)
        {
            var ret = new List<object>();
            if (rawlist.Contains(param))
            {
                var syscfg = CfgUtility.GetSysConfig(this);
                var rawlimits = syscfg[param].Split(new string[] { ",", ";", ":" }, StringSplitOptions.RemoveEmptyEntries);
                var lowlimit = Models.UT.O2D(rawlimits[0]);
                var highlimit = Models.UT.O2D(rawlimits[1]);
                var lowrange = lowlimit - 3.0 * Math.Abs(highlimit - lowlimit);
                var highrange = highlimit + 3.0 * Math.Abs(highlimit - lowlimit);
                var wuxiwatdatadict = WATAnalyzeVM.GetWUXIWATRawData(param, wflist, rp, lowrange, highrange);
                if (wuxiwatdatadict.Count > 0)
                {
                    ret.Add(wuxiwatdatadict);
                    ret.Add(lowlimit);
                    ret.Add(highlimit);
                }
            }
            else if (logiclist.Contains(param))
            {
                var newparam = param.Replace("LOGIC", rp);
                var paramlimit = WATAnalyzeVM.GetWATSpecLimit(newparam, wflist);
                var lowlimit = paramlimit[0];
                var highlimit = paramlimit[1];
                if (newparam.ToUpper().Contains("PO_LD_W_dB_".ToUpper()))
                {
                    lowlimit = -1.0; highlimit = 1;
                }
                if (newparam.ToUpper().Contains("THOLD_A_rd_".ToUpper()))
                {
                    lowlimit = 0; highlimit = 10;
                }

                var lowrange = lowlimit - 3.0 * Math.Abs(highlimit - lowlimit);
                var highrange = highlimit + 3.0 * Math.Abs(highlimit - lowlimit);
                var wuxiwatdatadict =  WATAnalyzeVM.GetWUXIWATLogicData(newparam, wflist, rp, lowrange, highrange);
                if (wuxiwatdatadict.Count > 0)
                {
                    ret.Add(wuxiwatdatadict);
                    ret.Add(lowlimit);
                    ret.Add(highlimit);
                }
            }

            return ret;
        }

        private JsonResult WUXIWATPvsPChart(string xparam,string yparam,List<string> wflist,List<object> xdatalist, List<object> ydatalist)
        {
            var xdatadict = (Dictionary<string, List<XYVAL>>)xdatalist[0];
            var xlowlimit = (double)xdatalist[1];
            var xhighlimit = (double)xdatalist[2];

            var ydatadict = (Dictionary<string, List<XYVAL>>)ydatalist[0];
            var ylowlimit = (double)ydatalist[1];
            var yhighlimit = (double)ydatalist[2];

            var series = new List<object>();
            foreach (var wf in wflist)
            {
                if (xdatadict.ContainsKey(wf) && ydatadict.ContainsKey(wf))
                {
                    var xvaldict = new Dictionary<string, double>();
                    foreach (var v in xdatadict[wf])
                    {
                        var key = wf + ":::" + v.unit;
                        if (!xvaldict.ContainsKey(key))
                        { xvaldict.Add(key, v.Val); }
                    }

                    var yvaldict = new Dictionary<string, double>();
                    foreach (var v in ydatadict[wf])
                    {
                        var key = wf + ":::" + v.unit;
                        if (!yvaldict.ContainsKey(key))
                        { yvaldict.Add(key, v.Val); }
                    }

                    var rawdata = new List<object>();
                    foreach (var xkv in xvaldict)
                    {
                        if (yvaldict.ContainsKey(xkv.Key))
                        {
                            var tmplist = new List<double>();
                            tmplist.Add(xkv.Value);
                            tmplist.Add(yvaldict[xkv.Key]);
                            rawdata.Add(tmplist);
                        }
                    }

                    series.Add(new{
                        name = wf,
                        type = "scatter",
                        data = rawdata,
                        marker = new {
                        lineWidth = 1,
                        radius = 2.5
                        },
                        tooltip = new {
                            headerFormat= "",
                            pointFormat = "x:{point.x}<br>y:{point.y}"
                        },
                        turboThreshold = 500000
                    });
                }//end if
            }//end foreach


            if (series.Count == 0)
            {
                var ret1 = new JsonResult();
                ret1.MaxJsonLength = Int32.MaxValue;
                ret1.Data = new
                {
                    sucess = false,
                    msg = "XParam,YParam have no common data on same wafer!"
                };
                return ret1;
            }

            var id = xparam.Replace(" ", "_") + "_id";
            var title = xparam + " vs "+ yparam;

            var labels = new List<object>();
            labels.Add(new
            {
                format = "<table><tr><td>XLL:" + xlowlimit + "</td></tr><tr><td>XHL:" + xhighlimit 
                + "</td></tr><tr><td>YLL:" + ylowlimit + "</td></tr><tr><td>YHL:" + yhighlimit + "</td></tr></table>",
                useHTML = true,
                point = new
                {
                    x = 0,
                    y = 0
                }
            });

            var pvpdata = new
            {
                id = id,
                title = title,
                xtitle = xparam,
                xlowlimit = xlowlimit,
                xhighlimit = xhighlimit,
                ytitle = yparam,
                ylowlimit = ylowlimit,
                yhighlimit = yhighlimit,
                labels = labels,
                series = series
            };

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                sucess = true,
                pvpdata = pvpdata
            };
            return ret;

        }

        public JsonResult WUXIWATPvsPData()
        {
            var xparam = Request.Form["xparam"].Trim();
            xparam = WATAnalyzeVM.RealParam(xparam);
            var yparam = Request.Form["yparam"].Trim();
            yparam = WATAnalyzeVM.RealParam(yparam);

            var wafers = Request.Form["wafers"];
            var rp = Request.Form["rp"];

            if ((CCT(wafers, "08") || CCT(wafers, "10"))
                && (rp.Contains("RP04") || rp.Contains("RP05")))
            {
                var ret = new JsonResult();
                ret.MaxJsonLength = Int32.MaxValue;
                ret.Data = new
                {
                    sucess = false,
                    msg = "E08/E10 not support RP04/RP05!"
                };
                return ret;
            }

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

            if (!rawlist.Contains(xparam) && !logiclist.Contains(xparam))
            {
                var ret = new JsonResult();
                ret.MaxJsonLength = Int32.MaxValue;
                ret.Data = new
                {
                    sucess = false,
                    msg = "Parameter " + xparam + "is not supported!"
                };
                return ret;
            }

            if (!rawlist.Contains(yparam) && !logiclist.Contains(yparam))
            {
                var ret = new JsonResult();
                ret.MaxJsonLength = Int32.MaxValue;
                ret.Data = new
                {
                    sucess = false,
                    msg = "Parameter " + yparam + "is not supported!"
                };
                return ret;
            }

            var xdatalist = GetWXWATTestData(xparam,rp,wflist, rawlist, logiclist);
            if (xdatalist.Count == 0)
            {
                var ret = new JsonResult();
                ret.MaxJsonLength = Int32.MaxValue;
                ret.Data = new
                {
                    sucess = false,
                    msg = "No WAT Test Data For Param: " + xparam 
                };
                return ret;
            }

            var ydatalist = GetWXWATTestData(yparam, rp, wflist, rawlist, logiclist);
            if (ydatalist.Count == 0)
            {
                var ret = new JsonResult();
                ret.MaxJsonLength = Int32.MaxValue;
                ret.Data = new
                {
                    sucess = false,
                    msg = "No WAT Test Data For Param: " + yparam
                };
                return ret;
            }

            return WUXIWATPvsPChart(xparam,yparam,wflist, xdatalist, ydatalist);
        }





        private List<SelectListItem> CreateSelectList(List<string> valist, string defVal)
        {
            bool selected = false;
            var pslist = new List<SelectListItem>();
            foreach (var p in valist)
            {
                var pitem = new SelectListItem();
                pitem.Text = p;
                pitem.Value = p;
                if (!string.IsNullOrEmpty(defVal) && string.Compare(defVal, p, true) == 0)
                {
                    pitem.Selected = true;
                    selected = true;
                }
                pslist.Add(pitem);
            }

            if (!selected && pslist.Count > 0)
            {
                pslist[0].Selected = true;
            }

            return pslist;
        }

        public ActionResult WUXIWATGoldSample(string tester)
        {
            var url = "/WATLogic/WUXIWATGoldSample";
            if (!CheckName(Request.UserHostName, url))
            { return Jump2Welcome(url); }

            var goldentesters = WATGoldSample.GetWATTesterList();
            ViewBag.goldentesterlist = CreateSelectList(goldentesters, "");

            ViewBag.tester = "";
            if (!string.IsNullOrEmpty(tester))
            { ViewBag.tester = tester; }

            return View();
        }

        public JsonResult WUXIWATGoldSampleData()
        {
            var syscfg = CfgUtility.GetSysConfig(this);
            var tester = Request.Form["tester"];
            var sdate = Request.Form["sdate"];
            var edate = Request.Form["edate"];
            var startdate = "";
            var enddate = "";
            if (string.IsNullOrEmpty(sdate) || string.IsNullOrEmpty(edate))
            {
                startdate = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd HH:mm:ss");
                enddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                var st = Models.UT.O2T(sdate);
                var et = Models.UT.O2T(edate);
                if (et > st)
                {
                    startdate = st.ToString("yyyy-MM-dd") + " 00:00:00";
                    enddate = et.ToString("yyyy-MM-dd") + " 23:59:59";
                }
                else {
                    startdate = et.ToString("yyyy-MM-dd") + " 00:00:00";
                    enddate = st.ToString("yyyy-MM-dd") + " 23:59:59";
                }
            }

            var colors = new string[] { "#2f7ed8", "#0d233a", "#8bbc21", "#910000", "#1aadce",
                    "#492970", "#f28f43", "#77a1e5", "#c42525", "#a6c96a" };

            var paramlist = syscfg["GOLDSAMPLEPARAM"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var rad = new System.Random(DateTime.Now.Second);
            var chartlist = new List<object>();
            foreach (var param in paramlist)
            {
                var limits = syscfg[param + "_GLD"].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                var ll = Models.UT.O2D(limits[0]);
                var ul = Models.UT.O2D(limits[1]);

                var gsdict = WATGoldSample.GetGoldData(tester, param, startdate, enddate);
                if (gsdict.Count == 0)
                { continue; }

                //get date idx dict
                var datelist = new List<string>();
                foreach (var gskv in gsdict)
                {
                    foreach (var dkv in gskv.Value)
                    {
                        if (!datelist.Contains(dkv.Key))
                        { datelist.Add(dkv.Key); }
                    }
                }
                datelist.Sort(delegate (string obj1, string obj2) {
                    var d1 = Models.UT.O2T(obj1 + " 00:00:00");
                    var d2 = Models.UT.O2T(obj2 + " 00:00:00");
                    return d1.CompareTo(d2);
                });

                var datedict = new Dictionary<string, int>();
                var didx = 0;
                foreach (var d in datelist)
                {
                    datedict.Add(d, didx);
                    didx++;
                }

                var seriallist = new List<object>();
                var xidx = 0;
                var gidx = 0;
                foreach (var gskv in gsdict)
                {
                    var datalist = new List<object>();
                    var color = colors[gidx%colors.Length];
                    gidx++;

                    var datadict = gskv.Value;
                    if (datadict.Count > 0)
                    {
                        foreach (var kv in datadict)
                        {
                            var x = 0.0;
                            if (xidx % 2 == 0)
                            { x = datedict[kv.Key] + rad.NextDouble() / 5.0; }
                            else
                            { x = datedict[kv.Key] - rad.NextDouble() / 5.0; }

                            foreach (var val in kv.Value)
                            {
                                datalist.Add(new
                                {
                                    x = x,
                                    y = val
                                });
                            }
                            xidx++;
                        }

                        seriallist.Add(new
                        {
                            name = gskv.Key,
                            color = color,
                            data = datalist,
                            marker = new {
                                lineWidth = 1,
                                radius = 3
                            },
                            tooltip = new
                            {
                                headerFormat = "{series.name}<br>",
                                pointFormat = "{point.y}"
                            },
                            turboThreshold = 800000
                        });

                    }//end if
                }//end foreach

                var id = param.Replace(" ", "_") + "_id";
                var title = tester + " Golden Sample " + param + " Distribution";
                var labels = new List<object>();
                labels.Add(new
                {
                    format = "<table><tr><td>LL:" + ll + "</td></tr><tr><td>HL:" + ul + "</td></tr></table>",
                    useHTML = true,
                    point = new
                    {
                        x = 0,
                        y = 0
                    }
                });
                chartlist.Add(new
                {
                    id = id,
                    title = title,
                    categories = datelist,
                    lowlimit = ll,
                    highlimit = ul,
                    labels = labels,
                    seriallist = seriallist
                });
            }//end foreach

            if (chartlist.Count > 0)
            {
                var ret = new JsonResult();
                ret.MaxJsonLength = Int32.MaxValue;
                ret.Data = new
                {
                    sucess = true,
                    chartlist = chartlist
                };
                return ret;
            }
            else
            {
                var ret = new JsonResult();
                ret.MaxJsonLength = Int32.MaxValue;
                ret.Data = new
                {
                    sucess = false,
                    msg = "Fail to get any gold sample data for tester:" + tester
                };
                return ret;
            }

        }


        public JsonResult WUXIWATGoldSamplePwrData()
        {
            var syscfg = CfgUtility.GetSysConfig(this);
            var tester = Request.Form["tester"];
            var sdate = Request.Form["sdate"];
            var edate = Request.Form["edate"];
            var startdate = "";
            var enddate = "";
            if (string.IsNullOrEmpty(sdate) || string.IsNullOrEmpty(edate))
            {
                startdate = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd HH:mm:ss");
                enddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                var st = Models.UT.O2T(sdate);
                var et = Models.UT.O2T(edate);
                if (et > st)
                {
                    startdate = st.ToString("yyyy-MM-dd") + " 00:00:00";
                    enddate = et.ToString("yyyy-MM-dd") + " 23:59:59";
                }
                else
                {
                    startdate = et.ToString("yyyy-MM-dd") + " 00:00:00";
                    enddate = st.ToString("yyyy-MM-dd") + " 23:59:59";
                }
            }

            var colors = new string[] { "#2f7ed8", "#0d233a", "#8bbc21", "#910000", "#1aadce",
                    "#492970", "#f28f43", "#77a1e5", "#c42525", "#a6c96a" };

            var paramlist = syscfg["GOLDSAMPLECHPARAM"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();

            var chartlist = new List<object>();
            foreach (var param in paramlist)
            {
                //var limits = syscfg[param + "_GLD"].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                //var ll = Models.UT.O2D(limits[0]);
                //var ul = Models.UT.O2D(limits[1]);

                var gsdict = WATGoldSample.GetGoldPwrData(tester, param, startdate, enddate);
                if (gsdict.Count == 0)
                { continue; }

                var gidx = 0;
                foreach (var gskv in gsdict)
                {
                    if (gskv.Value.Count < 5)
                    { continue; }

                    //get date idx dict
                    var datelist = new List<string>();

                    foreach (var dkv in gskv.Value)
                    {
                        if (!datelist.Contains(dkv.Key))
                        { datelist.Add(dkv.Key); }
                    }

                    datelist.Sort(delegate (string obj1, string obj2) {
                        var d1 = Models.UT.O2T(obj1 + " 00:00:00");
                        var d2 = Models.UT.O2T(obj2 + " 00:00:00");
                        return d1.CompareTo(d2);
                    });

                    var datedict = new Dictionary<string, int>();
                    var didx = 0;
                    foreach (var d in datelist)
                    {
                        datedict.Add(d, didx);
                        didx++;
                    }

                    var seriallist = new List<object>();
                    var valuelist = new List<double>();

                    var datalist = new List<object>();
                    var color = colors[gidx % colors.Length];
                    gidx++;

                    var datadict = gskv.Value;
                    if (datadict.Count > 0)
                    {
                        foreach (var kv in datadict)
                        {
                            var x = datedict[kv.Key];
                            foreach (var val in kv.Value)
                            {
                                valuelist.Add(val);
                                datalist.Add(new
                                {
                                    x = x,
                                    y = val
                                });
                            }
                        }

                        seriallist.Add(new
                        {
                            name = gskv.Key,
                            color = color,
                            data = datalist,
                            marker = new
                            {
                                lineWidth = 2,
                                fillColor = color
                            },
                            tooltip = new
                            {
                                headerFormat = "{series.name}<br>",
                                pointFormat = "{point.y}"
                            },
                            turboThreshold = 800000
                        });

                        if (valuelist.Count > 10)
                        { valuelist.RemoveAt(valuelist.Count -1); }

                        var mean = Statistics.Mean(valuelist);
                        var stddev = Math.Abs(Statistics.StandardDeviation(valuelist));
                        var ll = mean - 3.0 * stddev;
                        var ul = mean + 3.0 * stddev;

                        var min = mean - 4.0 * stddev;
                        var max = mean + 4.0 * stddev;

                        var id = gskv.Key.Replace(" ", "_").Replace("-","_") +"_"+param.Replace(" ","").Replace("-","_").Replace(".","").Replace(":","")+ "_id";
                        var title = tester + " Golden Sample "+gskv.Key+" " + param + " Distribution";

                        var labels = new List<object>();
                        labels.Add(new
                        {
                            format = "<table><tr><td>LL:" + Math.Round(ll,6) + "</td></tr><tr><td>HL:" + Math.Round(ul,6) + "</td></tr></table>",
                            useHTML = true,
                            point = new
                            {
                                x = 0,
                                y = 0
                            }
                        });

                        chartlist.Add(new
                        {
                            id = id,
                            title = title,
                            categories = datelist,
                            lowlimit = ll,
                            highlimit = ul,
                            min = min,
                            max = max,
                            labels = labels,
                            seriallist = seriallist
                        });


                    }//end if
                }//end foreach

                
            }//end foreach

            if (chartlist.Count > 0)
            {
                var ret = new JsonResult();
                ret.MaxJsonLength = Int32.MaxValue;
                ret.Data = new
                {
                    sucess = true,
                    chartlist = chartlist
                };
                return ret;
            }
            else
            {
                var ret = new JsonResult();
                ret.MaxJsonLength = Int32.MaxValue;
                ret.Data = new
                {
                    sucess = false,
                    msg = "Fail to get any gold sample data for tester:" + tester
                };
                return ret;
            }

        }

        //WUXIWATGoldSampleDttData
        public JsonResult WUXIWATGoldSampleDttData()
        {
            var syscfg = CfgUtility.GetSysConfig(this);
            var tester = Request.Form["tester"];
            var sdate = Request.Form["sdate"];
            var edate = Request.Form["edate"];
            var startdate = "";
            var enddate = "";
            if (string.IsNullOrEmpty(sdate) || string.IsNullOrEmpty(edate))
            {
                startdate = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd HH:mm:ss");
                enddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                var st = Models.UT.O2T(sdate);
                var et = Models.UT.O2T(edate);
                if (et > st)
                {
                    startdate = st.ToString("yyyy-MM-dd") + " 00:00:00";
                    enddate = et.ToString("yyyy-MM-dd") + " 23:59:59";
                }
                else
                {
                    startdate = et.ToString("yyyy-MM-dd") + " 00:00:00";
                    enddate = st.ToString("yyyy-MM-dd") + " 23:59:59";
                }
            }

            var colors = new string[] { "#2f7ed8", "#0d233a", "#8bbc21", "#910000", "#1aadce",
                    "#492970", "#f28f43", "#77a1e5", "#c42525", "#a6c96a" };

            var paramlist = syscfg["GOLDSAMPLECHPARAM"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();

            var chartlist = new List<object>();
            foreach (var param in paramlist)
            {
                //var limits = syscfg[param + "_GLD"].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                //var ll = Models.UT.O2D(limits[0]);
                //var ul = Models.UT.O2D(limits[1]);

                var gsdict = WATGoldSample.GetGoldPwrData(tester, param, startdate, enddate);
                if (gsdict.Count == 0)
                { continue; }

                var gidx = 0;
                foreach (var gskv in gsdict)
                {
                    var datadict = gskv.Value;
                    if (datadict.Count < 5)
                    { continue; }

                    //get date idx dict
                    var datelist = new List<string>();

                    foreach (var dkv in datadict)
                    {
                        if (!datelist.Contains(dkv.Key))
                        { datelist.Add(dkv.Key); }
                    }

                    datelist.Sort(delegate (string obj1, string obj2) {
                        var d1 = Models.UT.O2T(obj1 + " 00:00:00");
                        var d2 = Models.UT.O2T(obj2 + " 00:00:00");
                        return d1.CompareTo(d2);
                    });

                    var tenvals = new List<double>();
                    var idx = 0;
                    foreach (var d in datelist)
                    {
                        if (datadict[d].Count > 0)
                        { tenvals.Add(datadict[d][0]); }
                        idx++;
                        if (idx > 9)
                        { break; }
                    }
                    var dtt = tenvals.Average();

                    var datedict = new Dictionary<string, int>();
                    var didx = 0;
                    foreach (var d in datelist)
                    {
                        datedict.Add(d, didx);
                        didx++;
                    }

                    var seriallist = new List<object>();
                    //var valuelist = new List<double>();

                    var datalist = new List<object>();
                    var color = colors[gidx % colors.Length];
                    gidx++;

                    
                    if (datadict.Count > 0)
                    {
                        foreach (var kv in datadict)
                        {
                            var x = datedict[kv.Key];
                            foreach (var val in kv.Value)
                            {
                                //valuelist.Add(val);
                                datalist.Add(new
                                {
                                    x = x,
                                    y = (val-dtt)
                                });
                            }
                        }

                        seriallist.Add(new
                        {
                            name = gskv.Key,
                            color = color,
                            data = datalist,
                            marker = new
                            {
                                lineWidth = 2,
                                fillColor = color
                            },
                            tooltip = new
                            {
                                headerFormat = "{series.name}<br>",
                                pointFormat = "{point.y}"
                            },
                            turboThreshold = 800000
                        });

                        //if (valuelist.Count > 10)
                        //{ valuelist.RemoveAt(valuelist.Count - 1); }

                        //var mean = Statistics.Mean(valuelist);
                        //var stddev = Math.Abs(Statistics.StandardDeviation(valuelist));
                        //var ll = mean - 3.0 * stddev;
                        //var ul = mean + 3.0 * stddev;

                        //var min = mean - 4.0 * stddev;
                        //var max = mean + 4.0 * stddev;

                        var id = gskv.Key.Replace(" ", "_").Replace("-", "_") + "_" + param.Replace(" ", "").Replace("-", "_").Replace(".", "").Replace(":", "") + "_id";
                        var title = tester + " Golden Sample " + gskv.Key + " " + param + " Distribution";

                        //var labels = new List<object>();
                        //labels.Add(new
                        //{
                        //    format = "<table><tr><td>LL:" + Math.Round(ll, 6) + "</td></tr><tr><td>HL:" + Math.Round(ul, 6) + "</td></tr></table>",
                        //    useHTML = true,
                        //    point = new
                        //    {
                        //        x = 0,
                        //        y = 0
                        //    }
                        //});

                        chartlist.Add(new
                        {
                            id = id,
                            title = title,
                            categories = datelist,
                            //lowlimit = ll,
                            //highlimit = ul,
                            //min = min,
                            //max = max,
                            //labels = labels,
                            seriallist = seriallist
                        });


                    }//end if
                }//end foreach


            }//end foreach

            if (chartlist.Count > 0)
            {
                var ret = new JsonResult();
                ret.MaxJsonLength = Int32.MaxValue;
                ret.Data = new
                {
                    sucess = true,
                    chartlist = chartlist
                };
                return ret;
            }
            else
            {
                var ret = new JsonResult();
                ret.MaxJsonLength = Int32.MaxValue;
                ret.Data = new
                {
                    sucess = false,
                    msg = "Fail to get any gold sample data for tester:" + tester
                };
                return ret;
            }

        }

        public ActionResult WUXIWATOven(string tester)
        {
            var url = "/WATLogic/WUXIWATOven";
            if (!CheckName(Request.UserHostName, url))
            { return Jump2Welcome(url); }

            var syscfg = CfgUtility.GetSysConfig(this);

            var goldentesters =syscfg["WATOVEN"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            ViewBag.goldentesterlist = CreateSelectList(goldentesters, "");

            ViewBag.tester = "";
            if (!string.IsNullOrEmpty(tester))
            { ViewBag.tester = tester; }

            return View();
        }

        public JsonResult WUXIWATOvenData()
        {
            var syscfg = CfgUtility.GetSysConfig(this);
            var tester = Request.Form["tester"];
            var sdate = Request.Form["sdate"];
            var edate = Request.Form["edate"];
            var startdate = "";
            var enddate = "";
            if (string.IsNullOrEmpty(sdate) || string.IsNullOrEmpty(edate))
            {
                startdate = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd HH:mm:ss");
                enddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                var st = Models.UT.O2T(sdate);
                var et = Models.UT.O2T(edate);
                if (et > st)
                {
                    startdate = st.ToString("yyyy-MM-dd") + " 00:00:00";
                    enddate = et.ToString("yyyy-MM-dd") + " 23:59:59";
                }
                else
                {
                    startdate = et.ToString("yyyy-MM-dd") + " 00:00:00";
                    enddate = st.ToString("yyyy-MM-dd") + " 23:59:59";
                }
            }

            var paramlist = new List<string>();
            paramlist.Add("OVEN_TEMPERATURE");
            paramlist.Add("OVEN_CURRENT");
            
            var chartlist = new List<object>();
            var overdata = WATOven.GetOvenData(tester, startdate, enddate, Models.UT.O2I(syscfg["OVENSAMPPICKFREQ"]));
            if (overdata.Count != 2)
            {
                var ret1 = new JsonResult();
                ret1.MaxJsonLength = Int32.MaxValue;
                ret1.Data = new
                {
                    sucess = false,
                    msg = "Fail to get any WAT OVEN data for tester:" + tester
                };
                return ret1;
            }

            var rad = new System.Random(DateTime.Now.Second);
            var idx = 0;
            foreach (var param in paramlist)
            {
                var datadict = (Dictionary<string, List<double>>)overdata[idx];
                idx += 1;

                if (datadict.Count > 0)
                {
                    var limits = syscfg[param].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                    var ll = Models.UT.O2D(limits[0]);
                    var ul = Models.UT.O2D(limits[1]);

                    var xlist = datadict.Keys.ToList();
                    xlist.Sort(delegate (string obj1, string obj2) {
                        var d1 = Models.UT.O2T(obj1 + " 00:00:00");
                        var d2 = Models.UT.O2T(obj2 + " 00:00:00");
                        return d1.CompareTo(d2);
                    });

                    var datalist = new List<object>();
                    var xidx = 0;
                    var cidx = 0;

                    foreach (var x in xlist)
                    {
                        foreach (var v in datadict[x])
                        {
                            var tempdata = new List<double>();
                            if (cidx % 2 == 0)
                            { tempdata.Add(xidx + rad.NextDouble() / 5.0); }
                            else
                            { tempdata.Add(xidx - rad.NextDouble() / 5.0); }

                            tempdata.Add(v);
                            datalist.Add(tempdata);
                            cidx++;
                        }
                        xidx++;
                    }//end foreach

                    var id = param.Replace(" ", "_") + "_id";
                    var title = tester+ " " + param.Replace("_", " ") + " Distribution";

                    var labels = new List<object>();
                    labels.Add(new
                    {
                        format = "<table><tr><td>LL:" + ll + "</td></tr><tr><td>HL:" + ul + "</td></tr></table>",
                        useHTML = true,
                        point = new
                        {
                            x = 0,
                            y = 0
                        }
                    });

                    chartlist.Add(new
                    {
                        id = id,
                        title = title,
                        categories = xlist,
                        lowlimit = ll,
                        highlimit = ul,
                        labels = labels,
                        datalist = datalist
                    });
                }//end if
            }


            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                sucess = true,
                chartlist = chartlist
            };
            return ret;

        }

        public JsonResult GetWXCouponIndex()
        {
            var idxlist = new List<string>();
            for (var idx = 100; idx < 161; idx++)
            {
                idxlist.Add(idx.ToString().Substring(1));
            }
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                idxlist = idxlist
            };
            return ret;
        }

        public ActionResult WUXIWATCouponOVEN(string couponid)
        {
            var url = "/WATLogic/WUXIWATCouponOVEN";
            if (!CheckName(Request.UserHostName, url))
            { return Jump2Welcome(url); }

            ViewBag.couponid = "";
            if (!string.IsNullOrEmpty(couponid))
            { ViewBag.couponid = couponid; }

            return View();
        }

        public JsonResult WUXIWATCouponOVENData()
        {
            var syscfg = CfgUtility.GetSysConfig(this);
            var couponid = Request.Form["couponid"];
            //var ovenmachines = syscfg["WATOVEN"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();

            var tempdict = new Dictionary<string, List<double>>();
            var currentdict = new Dictionary<string, List<double>>();

            var oneovendata = WATOven.GetOvenData("", "", "", Models.UT.O2I(syscfg["OVENSAMPPICKFREQ"]),couponid);
            if (oneovendata.Count == 2)
            {
                tempdict = (Dictionary<string, List<double>>)oneovendata[0];
                currentdict = (Dictionary<string, List<double>>)oneovendata[1];
            }

            if (tempdict.Count == 0 && currentdict.Count == 0)
            {
                var ret1 = new JsonResult();
                ret1.MaxJsonLength = Int32.MaxValue;
                ret1.Data = new
                {
                    sucess = false,
                    msg = "Fail to get any WAT OVEN data for coupon:" + couponid
                };
                return ret1;
            }

            var overdata = new List<object>();
            overdata.Add(tempdict);
            overdata.Add(currentdict);

            var paramlist = new List<string>();
            paramlist.Add("OVEN_TEMPERATURE");
            paramlist.Add("OVEN_CURRENT");
            var chartlist = new List<object>();
            var rad = new System.Random(DateTime.Now.Second);
            var idx = 0;
            foreach (var param in paramlist)
            {
                var datadict = (Dictionary<string, List<double>>)overdata[idx];
                idx += 1;

                if (datadict.Count > 0)
                {
                    var limits = syscfg[param].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                    var ll = Models.UT.O2D(limits[0]);
                    var ul = Models.UT.O2D(limits[1]);

                    var xlist = datadict.Keys.ToList();
                    xlist.Sort(delegate (string obj1, string obj2) {
                        var d1 = Models.UT.O2T(obj1 + " 00:00:00");
                        var d2 = Models.UT.O2T(obj2 + " 00:00:00");
                        return d1.CompareTo(d2);
                    });

                    var datalist = new List<object>();
                    var xidx = 0;
                    var cidx = 0;

                    foreach (var x in xlist)
                    {
                        foreach (var v in datadict[x])
                        {
                            var tempdata = new List<double>();
                            if (cidx % 2 == 0)
                            { tempdata.Add(xidx + rad.NextDouble() / 5.0); }
                            else
                            { tempdata.Add(xidx - rad.NextDouble() / 5.0); }

                            tempdata.Add(v);
                            datalist.Add(tempdata);
                            cidx++;
                        }
                        xidx++;
                    }//end foreach

                    var id = param.Replace(" ", "_") + "_id";
                    var title = couponid + " " + param.Replace("_", " ") + " Distribution";

                    var labels = new List<object>();
                    labels.Add(new
                    {
                        format = "<table><tr><td>LL:" + ll + "</td></tr><tr><td>HL:" + ul + "</td></tr></table>",
                        useHTML = true,
                        point = new
                        {
                            x = 0,
                            y = 0
                        }
                    });

                    chartlist.Add(new
                    {
                        id = id,
                        title = title,
                        categories = xlist,
                        lowlimit = ll,
                        highlimit = ul,
                        labels = labels,
                        datalist = datalist
                    });
                }//end if
            }


            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                sucess = true,
                chartlist = chartlist
            };
            return ret;
        }



        public JsonResult WUXIWATCouponOVENDownload()
        {
            var couponid = Request.Form["couponid"];
            var now = DateTime.Now;
            string imgdir = Server.MapPath("~/userfiles") + "\\docs\\" + now.ToString("yyyy-MM-dd") + "\\";
            if (!Directory.Exists(imgdir))
            { Directory.CreateDirectory(imgdir); }
            var fn = couponid + "_" + now.ToString("yyyyMMddHHmmss") + ".csv";
            var srcfile = imgdir + fn;
            var url = "/userfiles/docs/"+ now.ToString("yyyy-MM-dd")+"/"+ fn;

            var ovendata = WATOven.GetOvenDataByWafer(couponid);
            if (ovendata.Count == 0)
            {
                System.IO.File.WriteAllText(srcfile, "Fail to get OVEN data from coupuon information:" + couponid);
            }
            else
            {
                var sb = new StringBuilder();
                sb.Append("rid,Coupon,Plan,Board,Seat,Level,Slot,TargetC,WaterSetC,TargetIC,OvenTemperature,ImA,CreateTime\r\n");
                foreach (var line in ovendata)
                {
                    var tmpsb = new StringBuilder();
                    foreach (var item in line)
                    {
                        tmpsb.Append("\"" + item + "\",");
                    }
                    sb.Append(tmpsb.ToString() + "\r\n");
                }
                System.IO.File.WriteAllText(srcfile, sb.ToString());

                try
                {
                    var fzip = new ICSharpCode.SharpZipLib.Zip.FastZip();
                    fzip.CreateZip(imgdir + fn.Replace(".csv", ".zip"), imgdir, false, fn);
                    try { System.IO.File.Delete(srcfile); } catch (Exception ex) { }
                    url = url.Replace(".csv", ".zip");
                }
                catch (Exception ex)
                {
                }
            }

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                url = url
            };
            return ret;
        }


        public JsonResult LoadWATAnalyzeComment()
        {
            var watid = Request.Form["watid"];
            var watcommentdata = WATAnalyzeComment.GetComment(watid);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                watcommentdata = watcommentdata
            };
            return ret;
        }

        public JsonResult AddWATAnalyzeComment()
        {
            var watid = Request.Form["watid"];
            var bcomment = Request.Form["comment"];

            string dummyData = bcomment.Trim().Replace(" ", "+");
            if (dummyData.Length % 4 > 0)
            { dummyData = dummyData.PadRight(dummyData.Length + 4 - dummyData.Length % 4, '='); }
            var bytes = Convert.FromBase64String(dummyData);
            var comment = System.Text.Encoding.UTF8.GetString(bytes);

            WATAnalyzeComment.AddComment(watid, comment);

            var watcommentdata = WATAnalyzeComment.GetComment(watid);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                watcommentdata = watcommentdata
            };
            return ret;
        }


        public JsonResult CheckWATTestDataUniformity()
        {
            var waferlist = new List<object>();
            var syscfg = CfgUtility.GetSysConfig(this);

            var zerolevel = WAT.Models.UT.O2D(syscfg["WATDATAWDOGZEROLEVEL"]);
            var zerocnt = WAT.Models.UT.O2I(syscfg["WDOGZEROCNT"]);

            var filterlevel = syscfg["WATDATAWDOGFILTERLEVEL"];
            var filtercnt = syscfg["WDOGFILTERCNT"];

            var dict = new Dictionary<string, bool>();

            var machine = MachineUserMap.DetermineCompName(Request.UserHostName);
            var starttime = WebLog.GetLatestWATDataWDogTime(machine);
            WebLog.LogWATDataWDog(machine);

            var allcoupon = WuxiWATData4MG.GetRecentWATCouponID(starttime);
            foreach (var cp in allcoupon)
            {
                var key = cp.CouponID.ToUpper() + "_" + cp.TestStep.ToUpper();
                if (!dict.ContainsKey(key))
                {
                    dict.Add(key, true);
                    if (!WuxiWATData4MG.CheckWATDataUniform(cp.CouponID, cp.TestStep, cp.TestTime
                        , zerolevel,zerocnt,filterlevel,filtercnt))
                    {
                        WebLog.Log(cp.CouponID, "WDOGCATCH", cp.TestStep + " " + cp.TestTime);

                        waferlist.Add(new { wafer = cp.CouponID
                            ,teststep = cp.TestStep
                            ,tester = cp.Comment});
                    }
                }//end if
            }//end foreach

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                waferlist = waferlist
            };
            return ret;
        }

        public JsonResult WDogDemo()
        {
            var waferlist = new List<object>();

            waferlist.Add(new { wafer = "62024-261-040E0807", teststep = "Post_Test" });
            waferlist.Add(new { wafer = "62024-261-040E0814", teststep = "Post_HTOL1"});

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                waferlist = waferlist
            };
            return ret;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WAT.Models;

namespace WAT.Controllers
{
    public class WATLogicController : Controller
    {
        public ActionResult VerifyAllenLogic()
        {
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

    }
}
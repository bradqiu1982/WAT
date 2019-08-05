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
    }
}
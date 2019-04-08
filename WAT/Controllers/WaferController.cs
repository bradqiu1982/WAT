using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WAT.Models;

namespace WAT.Controllers
{
    public class WaferController : Controller
    {
        public ActionResult WaferQUAL()
        {
            return View();
        }

        public JsonResult WaferQUALData()
        {
            var ssdate = Request.Form["sdate"];
            var sedate = Request.Form["edate"];
            var startdate = DateTime.Now;
            var enddate = DateTime.Now;

            if (!string.IsNullOrEmpty(ssdate) && !string.IsNullOrEmpty(sedate))
            {
                var sdate = DateTime.Parse(Request.Form["sdate"]);
                var edate = DateTime.Parse(Request.Form["edate"]);
                if (sdate < edate)
                {
                    startdate = DateTime.Parse(sdate.ToString("yyyy-MM") + "-01 00:00:00");
                    enddate = DateTime.Parse(edate.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1);
                }
                else
                {
                    startdate = DateTime.Parse(edate.ToString("yyyy-MM") + "-01 00:00:00");
                    enddate = DateTime.Parse(sdate.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1);
                }
            }
            else
            {
                startdate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(-6);
                enddate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM") + "-01 00:00:00").AddMonths(1).AddSeconds(-1);
            }

            var waferdata = WaferQUALVM.RetrieveWaferData(startdate.ToString("yyyy-MM-dd HH:mm:ss"), enddate.ToString("yyyy-MM-dd HH:mm:ss"));

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                waferdata = waferdata
            };
            return ret;
        }

        public JsonResult WaferQUALReport()
        {
            var wkey = Request.Form["wkey"];
            var wreportlist = WaferReport.RetrieveWaferReport(wkey);
            if (wreportlist.Count == 0)
            {
                var report = new
                {
                    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    reporter = "System",
                    content = "TO BE EDIT"
                };

                var ret = new JsonResult();
                ret.MaxJsonLength = Int32.MaxValue;
                ret.Data = new
                {
                    success = true,
                    report = report
                };
                return ret;
            }
            else
            {
                var report = new
                {
                    time = wreportlist[0].CommentDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    reporter = wreportlist[0].Reporter.ToUpper().Replace("@FINISAR.COM", ""),
                    content = wreportlist[0].Comment
                };
                var ret = new JsonResult();
                ret.MaxJsonLength = Int32.MaxValue;
                ret.Data = new
                {
                    success = true,
                    report = report
                };
                return ret;
            }
        }





    }
}
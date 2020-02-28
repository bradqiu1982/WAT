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
    public class MGTLController : Controller
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

        public ActionResult WATTEST()
        {
            var url = "/MGTL/WATTEST";
            if (!CheckName(Request.UserHostName, url))
            { return Jump2Welcome(url); }

            return RedirectToAction("WUXIWATStatus", "MGTL");
        }

        public ActionResult WUXIWATStatus()
        {
            var url = "/MGTL/WUXIWATStatus";
            if (!CheckName(Request.UserHostName, url))
            { return Jump2Welcome(url); }

            return View();
        }

        public JsonResult WUXIWATStatusData()
        {
            var wipdata = WuxiWATData4MG.GetWATStatus(this);

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                wipdata = wipdata
            };
            return ret;
        }

        public JsonResult RefreshWATWIP()
        {
            try
            {
                WuxiWATData4MG.RefreshWATStatusDaily();
            }
            catch (Exception ex) { }

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                sucess = true
            };
            return ret;
        }

        public ActionResult WATWIP()
        {
            var url = "/MGTL/WATWIP";
            if (!CheckName(Request.UserHostName, url))
            { return Jump2Welcome(url); }

            return View();
        }

        public JsonResult WATWIPDATA()
        {
            var wipdata = WUXIWATWIP.GetWATWIP(this);

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                wipdata = wipdata
            };
            return ret;
        }
    }
}
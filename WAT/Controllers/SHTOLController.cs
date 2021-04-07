using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WAT.Models;

namespace WAT.Controllers
{
    public class SHTOLController : Controller
    {
        public ActionResult SHTOLDash()
        {
            return View();
        }

        public JsonResult GetSHTOLOutputDis()
        {
            var sdate = Request.Form["sdate"];
            if (string.IsNullOrEmpty(sdate))
            { sdate = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd") + " 00:00:00"; }
            else
            { sdate = DateTime.Parse(sdate).ToString("yyyy-MM-dd") + " 00:00:00"; }

            var disdict = SHTOLvm.GetWeeklyDoneDistribution(sdate, DateTime.Now.AddDays(1).ToString("yyyy-MM-dd") + " 00:00:00");
            var datelist = disdict.Keys.ToList();
            datelist.Sort(delegate (string obj1, string obj2) {
                var d1 = UT.O2T(obj1);
                var d2 = UT.O2T(obj2);
                return d1.CompareTo(d2);
            });

            var pdict = new Dictionary<string, bool>();
            foreach (var dkv in disdict)
            {
                foreach (var pkv in dkv.Value)
                {
                    if (!pdict.ContainsKey(pkv.Key))
                    { pdict.Add(pkv.Key, true); }
                }
            }

            var plist = pdict.Keys.ToList();

            var pcntdict = new Dictionary<string, List<int>>();
            foreach (var k in plist)
            { pcntdict.Add(k, new List<int>()); }

            foreach (var d in datelist)
            {
                foreach (var p in plist)
                {
                    if (disdict[d].ContainsKey(p))
                    {
                        pcntdict[p].Add(disdict[d][p]);
                    }
                    else
                    {
                        pcntdict[p].Add(0);
                    }
                }
            }

            var slist = new List<object>();
            foreach (var pkv in pcntdict)
            {
                slist.Add(new
                {
                    name = pkv.Key,
                    data = pkv.Value
                });
            }

            var dtlist = new List<string>();
            foreach (var d in datelist)
            { dtlist.Add(UT.O2T(d).ToString("yyyy-MM-dd")); }

            var chartdata = new
            {
                id = "shtoldist",
                title = "SHTOL WEEKLY OUTPUT",
                xlist = dtlist,
                series = slist
            };

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                chartdata = chartdata
            };
            return ret;
        }


        public JsonResult GetSHTOLWIP()
        {
            var pdict = SHTOLvm.GetSHTOLWIP();
            var plist = pdict.Keys.ToList();
            var dlist = new List<int>();
            foreach (var p in plist)
            { dlist.Add(pdict[p]); }

            var slist = new List<object>();
            slist.Add(new
            {
                name = "WIP",
                data = dlist
            });

            var chartdata = new
            {
                id = "shtolwip",
                title = "SHTOL WIP",
                xlist = plist,
                series = slist
            };

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                chartdata = chartdata
            };
            return ret;
        }

        public ActionResult SHTOLStatus()
        { return View(); }

        public JsonResult LoadSHTOLStatus()
        {
            var shtolstatdata = SHTOLAnalyzer.LoadAllSHTOLStat();
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                shtolstatdata = shtolstatdata
            };
            return ret;
        }

        public JsonResult UpdateSHTOLJudgement()
        {
            var sn = Request.Form["sn"];
            var stat = Request.Form["stat"];
            SHTOLAnalyzer.UpdateSHTOLJudgement(sn, stat);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                OK = true
            };
            return ret;
        }

        public ActionResult SHTOLAnalyze(string SN)
        {
            ViewBag.SN = "";
            if (!string.IsNullOrEmpty(SN))
            { ViewBag.SN = SN; }
            return View();
        }

        public JsonResult SHTOLAnalyzeChart()
        {
            var sn = Request.Form["sn"];
            var sndata = SHTOLAnalyzer.GetSNPWRData(sn);
            var maxpoint = 0;

            var slist = new List<object>();
            foreach (var pkv in sndata)
            {
                if (pkv.Value.Count > maxpoint)
                { maxpoint = pkv.Value.Count; }
                slist.Add(new
                {
                    name = pkv.Key,
                    data = pkv.Value
                });
            }

            var dtlist = new List<string>();
            for (var idx = 0;idx < maxpoint;idx++)
            { dtlist.Add(idx.ToString()); }

            var chartdata = new
            {
                id = "shtoldist",
                title = sn+" SHTOL POWER TREND",
                xlist = dtlist,
                series = slist
            };

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                chartdata = chartdata
            };
            return ret;
        }

    }
}
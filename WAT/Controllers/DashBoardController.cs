using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using WAT.Models;

namespace WAT.Controllers
{
    public class DashBoardController : Controller
    {
        private ActionResult Jump2Welcome(string url)
        {
            var dict = new RouteValueDictionary();
            dict.Add("url", url);
            return RedirectToAction("Welcome", "Main", dict);
        }

        private bool CheckName(string ip, string url)
        {
            if (ip.Contains("127.0.0.1"))
            { return true; }

            var machinename = MachineUserMap.GetUserMachineName(ip);
            if (machinename.Count == 0)
            { return false; }
            else
            {
                MachineUserMap.LoginLog(machinename[0], machinename[1], url);

                if (machinename[0].Trim().Length == 0)
                { return false; }

                return true;
            }
        }

        private string GetUserName(string ip)
        {
            if (ip.Contains("127.0.0.1"))
            { return "BRAD.QIU"; }

            var machinename = MachineUserMap.GetUserMachineName(ip);
            if (machinename.Count == 0)
            { return ""; }
            else
            {
                return machinename[1];
            }
        }

        public ActionResult WeeklyCapacityDS()
        {
            var url = "/DashBoard/WeeklyCapacityDS";
            if (!CheckName(Request.UserHostName, url))
            { return Jump2Welcome(url); }

            return View();
        }

        private List<object> GetCapacityTable(DateTime sdate, List<WATCapacity> capvmlist)
        {
            var ret = new List<object>();

            var weeklist = WATCapacity.GetWeekList(sdate);
            var realweeklist = new List<DateTime>();

            //vtype
            var vtypedict = new Dictionary<string, bool>();
            foreach (var cap in capvmlist)
            {
                if (!vtypedict.ContainsKey(cap.VType))
                { vtypedict.Add(cap.VType, true); }
            }

            //by week
            var weekdata = new Dictionary<DateTime, Dictionary<string, int>>();
            foreach (var cap in capvmlist)
            {
                foreach (var wk in weeklist)
                {
                    if (cap.WFDate >= wk && cap.WFDate < wk.AddDays(7))
                    {
                        if (!realweeklist.Contains(wk))
                        { realweeklist.Add(wk); }

                        if (weekdata.ContainsKey(wk))
                        {
                            weekdata[wk][cap.VType] += 1;
                        }
                        else
                        {
                            var tmpvtypedict = new Dictionary<string, int>();
                            foreach (var kv in vtypedict)
                            { tmpvtypedict.Add(kv.Key, 0); }
                            tmpvtypedict[cap.VType] = 1;
                            weekdata.Add(wk, tmpvtypedict);
                        }
                    }//in the week
                }
            }

            //construct table
            realweeklist.Sort();
            var title = new List<string>();
            title.Add("WAFER INPUT");
            foreach (var wk in realweeklist)
            { title.Add(wk.ToString("yy/MM/dd")); }
            title.Add("SUM");

            var colsum = new List<int>();
            var table = new List<List<string>>();
            foreach (var kv in vtypedict)
            {
                var tmplist = new List<string>();
                tmplist.Add(kv.Key);
                var cntlist = new List<int>();
                foreach (var wk in realweeklist)
                {
                    tmplist.Add(weekdata[wk][kv.Key].ToString());
                    cntlist.Add(weekdata[wk][kv.Key]);
                }
                tmplist.Add(cntlist.Sum().ToString());

                if (colsum.Count == 0)
                {
                    colsum.AddRange(cntlist);
                    colsum.Add(cntlist.Sum());
                }
                else
                {
                    for (var idx = 0; idx < cntlist.Count; idx++)
                    { colsum[idx] += cntlist[idx]; }
                    colsum[cntlist.Count] += cntlist.Sum();
                }

                table.Add(tmplist);
            }

            //get target capacity
            var caplist = new List<int>();
            var capdict = new Dictionary<DateTime, int>();
            var syscfg = CfgUtility.GetSysConfig(this);
            var ckvs = syscfg["WATCAPACITY"].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var ckv in ckvs)
            {
                var dckv = ckv.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                capdict.Add(DateTime.Parse(dckv[0] + " 00:00:00"), Models.UT.O2I(dckv[1]));
            }
            foreach (var wk in realweeklist)
            {
                var matchval = 0;
                foreach (var kv in capdict)
                {
                    if (wk >= kv.Key)
                    { matchval = kv.Value; }
                }
                if (matchval != 0)
                { caplist.Add(matchval); }
            }
            var allcap = caplist.Sum();
            caplist.Add(allcap);

            //get usage
            var tlist = new List<string>();
            tlist.Add("USAGE");
            var ix = 0;
            foreach (var val in colsum)
            {
                tlist.Add(Math.Round((double)val / (double)caplist[ix] * 100.0, 1).ToString() + "%");
                ix++;
            }
            table.Add(tlist);

            tlist = new List<string>();
            tlist.Add("CAPACITY");
            foreach (var v in caplist) { tlist.Add(v.ToString()); }
            table.Insert(0, tlist);

            ret.Add(title);
            ret.Add(table);

            return ret;
        }

        private object GetCapacityChart(List<string> title, List<List<string>> table)
        {
            var cidx = 0;
            var colorlist = new string[] { "#161525", "#0053a2", "#bada55", "#1D2088" ,"#00ff00", "#fca2cf", "#E60012", "#EB6100", "#E4007F"
                , "#CFDB00", "#8FC31F", "#22AC38", "#920783",  "#b5f2b0", "#F39800","#4e92d2" , "#FFF100"
                , "#1bfff5", "#4f4840", "#FCC800", "#0068B7", "#6666ff", "#009B6B", "#16ff9b" }.ToList();

            var id = "wat_input_id";
            var xAxis = new List<string>();
            var idx = 0;
            foreach (var dt in title)
            {
                if (idx != 0 && idx != (title.Count - 1))
                { xAxis.Add(dt); }
                idx++;
            }

            var caplist = new List<int>();
            idx = 0;
            foreach (var dt in table[0])
            {
                if (idx != 0 && idx != (table[0].Count - 1))
                { caplist.Add(Models.UT.O2I(dt)); }
                idx++;
            }

            var ydata = new List<object>();

            idx = 0;
            foreach (var line in table)
            {
                if (idx != 0 && idx != (table.Count - 1))
                {
                    var inputdata = new List<int>();
                    var inputtype = line[0];
                    var jidx = 0;
                    foreach (var val in line)
                    {
                        if (jidx != 0 && jidx != (line.Count - 1))
                        { inputdata.Add(Models.UT.O2I(val)); }
                        jidx++;
                    }

                    ydata.Add(new
                    {
                        name = inputtype,
                        type = "column",
                        data = inputdata,
                        color = colorlist[cidx],
                        yAxis = 0
                    });
                    cidx++;
                }
                idx++;
            }

            ydata.Add(new
            {
                name = "Capacity",
                type = "line",
                data = caplist,
                color = colorlist[cidx],
                yAxis = 0
            });
            cidx++;

            return new
            {
                id = id,
                title = "WUXI WAT INPUT",
                xAxis = xAxis,
                data = ydata
            };
        }

        public JsonResult WeeklyCapacityDSData()
        {
            var starttime = Request.Form["sdate"];
            if (string.IsNullOrEmpty(starttime))
            { starttime = DateTime.Now.AddMonths(-4).ToString("yyyy-MM"); }

            var sdate = DateTime.Parse(starttime + "-01 00:00:00");
            var orgdate = DateTime.Parse("2020-01-02 00:00:00");
            if (sdate < orgdate)
            { sdate = orgdate; }

            var datafrom = sdate.AddDays(-21).ToString("yyyy-MM-dd HH:mm:ss");
            var capvmlist = WATCapacity.GetWATCapacity(datafrom);

            var captable = GetCapacityTable(sdate, capvmlist);
            var title = (List<string>)captable[0];
            var table = (List<List<string>>)captable[1];

            var chartdata = GetCapacityChart(title, table);

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                title = title,
                table = table,
                chartdata = chartdata
            };
            return ret;
        }


        public ActionResult WeeklyYieldDS()
        {
            var url = "/DashBoard/WeeklyYieldDS";
            if (!CheckName(Request.UserHostName, url))
            { return Jump2Welcome(url); }

            return View();
        }

        public ActionResult WeeklyYield4Planning()
        {
            var url = "/DashBoard/WeeklyYield4Planning";
            if (!CheckName(Request.UserHostName, url))
            { return Jump2Welcome(url); }

            return View();
        }

        public ActionResult WATRTYield()
        {
            var url = "/DashBoard/WATRTYield";
            if (!CheckName(Request.UserHostName, url))
            { return Jump2Welcome(url); }

            return View();
        }

        private static string GetPNByWafer(string wf, Dictionary<string, string> wfproddict, Dictionary<string, string> pndict)
        {
            var wafer = wf.ToUpper().Replace("E", "").Replace("R", "").Replace("T", "");

            var binlist = new List<string>(new string[] { "55","57","56","58","59","54","53","52","51","50" });
            if (wfproddict.ContainsKey(wafer))
            {
                var prod = wfproddict[wafer];
                foreach (var bin in binlist)
                {
                    if (pndict.ContainsKey(prod + "-" + bin))
                    { return pndict[prod+"-"+bin]; }
                }
            }
            return string.Empty;
        }

        private List<object> GetYieldTable(DateTime sdate, List<WATCapacity> capvmlist)
        {
            var pndict = CfgUtility.GetProdfamPN(this);
            var wfproddict = WXEvalPN.GetWaferProdfamDict();

            var ret = new List<object>();
            var weekpassfailcap = new List<WATCapacity>();

            var weeklist = WATCapacity.GetWeekList(sdate);
            var realweeklist = new List<DateTime>();

            //vtype
            var vtypedict = new Dictionary<string, bool>();
            foreach (var cap in capvmlist)
            {
                if (!vtypedict.ContainsKey(cap.VType))
                { vtypedict.Add(cap.VType, true); }
            }

            //by week
            var weekdata = new Dictionary<DateTime, Dictionary<string, int>>();
            var weekpassdata = new Dictionary<DateTime, Dictionary<string, int>>();

            foreach (var cap in capvmlist)
            {
                foreach (var wk in weeklist)
                {
                    if (cap.WFDate >= wk && cap.WFDate < wk.AddDays(7))
                    {
                        if (!realweeklist.Contains(wk))
                        { realweeklist.Add(wk); }

                        cap.WKStr = wk.ToString("yy/MM/dd");
                        cap.PN = GetPNByWafer(cap.Wafer, wfproddict, pndict);
                        weekpassfailcap.Add(cap);

                        if (weekdata.ContainsKey(wk))
                        {
                            weekdata[wk][cap.VType] += 1;
                            if (cap.Pass.Contains("PASS"))
                            { weekpassdata[wk][cap.VType] += 1; }
                        }
                        else
                        {
                            var tmpvtypedict = new Dictionary<string, int>();
                            foreach (var kv in vtypedict)
                            { tmpvtypedict.Add(kv.Key, 0); }
                            tmpvtypedict[cap.VType] = 1;
                            weekdata.Add(wk, tmpvtypedict);

                            tmpvtypedict = new Dictionary<string, int>();
                            foreach (var kv in vtypedict)
                            { tmpvtypedict.Add(kv.Key, 0); }
                            if (cap.Pass.Contains("PASS"))
                            { tmpvtypedict[cap.VType] = 1; }
                            weekpassdata.Add(wk, tmpvtypedict);
                        }
                    }//in the week
                }
            }

            //construct table
            realweeklist.Sort();
            var fourwkago = DateTime.Now.AddDays(-28);
            var title = new List<string>();
            title.Add("WAFER YIELD");
            foreach (var wk in realweeklist)
            {
                if (wk > fourwkago)
                { title.Add(wk.ToString("yy/MM/dd")+"(RT)"); }
                else
                { title.Add(wk.ToString("yy/MM/dd")); }
            }
            title.Add("SUM");

            var colsum = new List<int>();
            var colpasssum = new List<int>();

            var table = new List<List<string>>();
            foreach (var kv in vtypedict)
            {
                var tmplist = new List<string>();
                tmplist.Add(kv.Key);
                var cntlist = new List<int>();
                var cntpasslist = new List<int>();

                foreach (var wk in realweeklist)
                {
                    tmplist.Add(weekdata[wk][kv.Key].ToString()+"/"+weekpassdata[wk][kv.Key].ToString());
                    cntlist.Add(weekdata[wk][kv.Key]);
                    cntpasslist.Add(weekpassdata[wk][kv.Key]);
                }
                tmplist.Add(cntlist.Sum().ToString()+"/"+ cntpasslist.Sum().ToString());

                if (colsum.Count == 0)
                {
                    colsum.AddRange(cntlist);
                    colsum.Add(cntlist.Sum());

                    colpasssum.AddRange(cntpasslist);
                    colpasssum.Add(cntpasslist.Sum());
                }
                else
                {
                    for (var idx = 0; idx < cntlist.Count; idx++)
                    {
                        colsum[idx] += cntlist[idx];
                        colpasssum[idx] += cntpasslist[idx];
                    }
                    colsum[cntlist.Count] += cntlist.Sum();
                    colpasssum[cntlist.Count] += cntpasslist.Sum();
                }

                table.Add(tmplist);
            }


            //get usage
            var tlist = new List<string>();
            tlist.Add("SUM INPUT");
            foreach (var val in colsum)
            { tlist.Add( val.ToString()); }
            table.Add(tlist);

            tlist = new List<string>();
            tlist.Add("SUM PASS");
            foreach (var val in colpasssum)
            { tlist.Add(val.ToString()); }
            table.Add(tlist);

            tlist = new List<string>();
            tlist.Add("YIELD");
            var ix = 0;
            foreach (var val in colsum)
            {
                tlist.Add(Math.Round( (double)colpasssum[ix] /(double)val * 100.0, 1).ToString() + "%");
                ix++;
            }
            table.Add(tlist);

            ret.Add(title);
            ret.Add(table);
            ret.Add(weekpassfailcap);

            return ret;
        }

        //private object GetCapacityChart(List<string> title, List<List<string>> table)
        //{
        //    var cidx = 0;
        //    var colorlist = new string[] { "#161525", "#0053a2", "#bada55", "#1D2088" ,"#00ff00", "#fca2cf", "#E60012", "#EB6100", "#E4007F"
        //        , "#CFDB00", "#8FC31F", "#22AC38", "#920783",  "#b5f2b0", "#F39800","#4e92d2" , "#FFF100"
        //        , "#1bfff5", "#4f4840", "#FCC800", "#0068B7", "#6666ff", "#009B6B", "#16ff9b" }.ToList();

        //    var id = "wat_input_id";
        //    var xAxis = new List<string>();
        //    var idx = 0;
        //    foreach (var dt in title)
        //    {
        //        if (idx != 0 && idx != (title.Count - 1))
        //        { xAxis.Add(dt); }
        //        idx++;
        //    }

        //    var caplist = new List<int>();
        //    idx = 0;
        //    foreach (var dt in table[0])
        //    {
        //        if (idx != 0 && idx != (table[0].Count - 1))
        //        { caplist.Add(Models.UT.O2I(dt)); }
        //        idx++;
        //    }

        //    var ydata = new List<object>();

        //    idx = 0;
        //    foreach (var line in table)
        //    {
        //        if (idx != 0 && idx != (table.Count - 1))
        //        {
        //            var inputdata = new List<int>();
        //            var inputtype = line[0];
        //            var jidx = 0;
        //            foreach (var val in line)
        //            {
        //                if (jidx != 0 && jidx != (line.Count - 1))
        //                { inputdata.Add(Models.UT.O2I(val)); }
        //                jidx++;
        //            }

        //            ydata.Add(new
        //            {
        //                name = inputtype,
        //                type = "column",
        //                data = inputdata,
        //                color = colorlist[cidx],
        //                yAxis = 0
        //            });
        //            cidx++;
        //        }
        //        idx++;
        //    }

        //    ydata.Add(new
        //    {
        //        name = "Capacity",
        //        type = "line",
        //        data = caplist,
        //        color = colorlist[cidx],
        //        yAxis = 0
        //    });
        //    cidx++;

        //    return new
        //    {
        //        id = id,
        //        title = "WUXI WAT INPUT",
        //        xAxis = xAxis,
        //        data = ydata
        //    };
        //}

        public JsonResult WeeklyYieldDSData()
        {
            var starttime = Request.Form["sdate"];
            if (string.IsNullOrEmpty(starttime))
            { starttime = DateTime.Now.AddMonths(-4).ToString("yyyy-MM"); }

            var sdate = DateTime.Parse(starttime + "-01 00:00:00");
            var orgdate = DateTime.Parse("2020-01-02 00:00:00");
            if (sdate < orgdate)
            { sdate = orgdate; }

            var datafrom = sdate.AddDays(-21).ToString("yyyy-MM-dd HH:mm:ss");


            var capvmlist = WATCapacity.GetWATCapacity(datafrom);
            var passfailwfdict = WuxiWATData4MG.GetPassFailWaferDict();

            foreach (var capvm in capvmlist)
            {
                if (passfailwfdict.ContainsKey(capvm.Wafer))
                {
                    capvm.Pass = passfailwfdict[capvm.Wafer];
                }
            }

            var yieldtable = GetYieldTable(sdate, capvmlist);
            var title = (List<string>)yieldtable[0];
            var table = (List<List<string>>)yieldtable[1];
            var weekpassfailcap = (List<WATCapacity>)yieldtable[2];

            //var chartdata = GetCapacityChart(title, table);

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                title = title,
                table = table,
                passfailcap = weekpassfailcap
                //,chartdata = chartdata
            };
            return ret;
        }

        public JsonResult WeeklyYield4PlanningData()
        {
            var starttime = Request.Form["sdate"];
            if (string.IsNullOrEmpty(starttime))
            { starttime = DateTime.Now.AddMonths(-4).ToString("yyyy-MM"); }

            var sdate = DateTime.Parse(starttime + "-01 00:00:00");
            var orgdate = DateTime.Parse("2020-01-02 00:00:00");
            if (sdate < orgdate)
            { sdate = orgdate; }

            var datafrom = sdate.AddDays(-21).ToString("yyyy-MM-dd HH:mm:ss");

            var passfaillist = new List<WATCapacity>();
            var capvmlist = WATCapacity.GetWATCapacity(datafrom);
            var passfailwfdict = WuxiWATData4MG.GetPassFailWaferDict();

            foreach (var capvm in capvmlist)
            {
                if (capvm.Wafer.Length > 10)
                { continue; }

                if (passfailwfdict.ContainsKey(capvm.Wafer))
                {
                    capvm.Pass = passfailwfdict[capvm.Wafer];
                    if (!capvm.Pass.Contains("PENDING"))
                    {
                        passfaillist.Add(capvm);
                    }
                }
            }

            var yieldtable = GetYieldTable(sdate, passfaillist);
            var title = (List<string>)yieldtable[0];
            var table = (List<List<string>>)yieldtable[1];
            var weekpassfailcap = (List<WATCapacity>)yieldtable[2];

            //var chartdata = GetCapacityChart(title, table);

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                title = title,
                table = table,
                passfailcap = weekpassfailcap
                //,chartdata = chartdata
            };
            return ret;
        }

        public JsonResult RTYieldDSData()
        {
            var starttime = Request.Form["sdate"];
            if (string.IsNullOrEmpty(starttime))
            { starttime = DateTime.Now.AddMonths(-4).ToString("yyyy-MM"); }

            var sdate = DateTime.Parse(starttime + "-01 00:00:00");
            var orgdate = DateTime.Parse("2020-01-02 00:00:00");
            if (sdate < orgdate)
            { sdate = orgdate; }

            var passfaillist = new List<WATCapacity>();

            var datafrom = sdate.ToString("yyyy-MM-dd HH:mm:ss");
            var capvmlist = WATCapacity.GetWATHTOL2Wafer(datafrom);
            var passfailwfdict = WuxiWATData4MG.GetPassFailWaferDict();
            foreach (var capvm in capvmlist)
            {
                if (passfailwfdict.ContainsKey(capvm.Wafer))
                { capvm.Pass = passfailwfdict[capvm.Wafer]; }

                if (!capvm.Pass.Contains("PENDING"))
                {
                    passfaillist.Add(capvm);
                }
            }


            var yieldtable = GetYieldTable(sdate, passfaillist);
            var title = (List<string>)yieldtable[0];
            var table = (List<List<string>>)yieldtable[1];
            var weekpassfailcap = (List<WATCapacity>)yieldtable[2];

            //var chartdata = GetCapacityChart(title, table);

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                title = title,
                table = table,
                passfailcap = weekpassfailcap
                //,chartdata = chartdata
            };
            return ret;
        }


        public ActionResult WUXIWATWIPDS()
        {
            var url = "/DashBoard/WUXIWATWIPDS";
            if (!CheckName(Request.UserHostName, url))
            { return Jump2Welcome(url); }

            return View();
        }

        public JsonResult WUXIWATWIPDSDATA()
        {
            var pndict = CfgUtility.GetProdfamPN(this);
            var wfproddict = WXEvalPN.GetWaferProdfamDict();

            var datafrom = DateTime.Now.AddDays(-90).ToString("yyyy-MM-dd HH:mm:ss");
            var wfdatedict = new Dictionary<string, DateTime>();
            var wftypedict = new Dictionary<string, string>();
            var capvmlist = WATCapacity.GetWATCapacity(datafrom);
            var passfailwfdict = WuxiWATData4MG.GetPassFailWaferDict();
            foreach (var capvm in capvmlist)
            {
                if (passfailwfdict.ContainsKey(capvm.Wafer))
                { capvm.Pass = passfailwfdict[capvm.Wafer]; }

                if (!wfdatedict.ContainsKey(capvm.Wafer))
                {
                    wfdatedict.Add(capvm.Wafer, capvm.WFDate);
                    wftypedict.Add(capvm.Wafer, capvm.VType);
                }
            }

            var wflist = new List<string>();
            var onemonthago = DateTime.Now.AddDays(-30);
            foreach (var cap in capvmlist)
            {
                if (cap.Pass.Contains("PENDING") && cap.WFDate > onemonthago)
                {
                    wflist.Add(cap.Wafer);
                }
            }

            if (wflist.Count > 0)
            {
                var wiplist = WATCapacity.GetWATWIP(wflist);

                var title = new List<string>();
                title.Add("WAT WIP");
                title.Add("TYPE");
                title.Add("PN");
                title.Add("START DATE");
                title.Add("PRE-BI");
                title.Add("POST-BI");
                title.Add("HTOL1");
                title.Add("HTOL2");

                var table = new List<List<string>>();
                foreach (var wf in wflist)
                {
                    var tmplist = new List<string>();
                    tmplist.Add(wf);

                    if (wftypedict.ContainsKey(wf))
                    { tmplist.Add(wftypedict[wf]); }
                    else
                    { tmplist.Add(""); }

                    tmplist.Add(GetPNByWafer(wf, wfproddict, pndict));

                    if (wfdatedict.ContainsKey(wf))
                    { tmplist.Add(wfdatedict[wf].ToString("yy/MM/dd")); }
                    else
                    { tmplist.Add(""); }

                    tmplist.Add("PENDING");
                    tmplist.Add("PENDING");
                    tmplist.Add("PENDING");
                    tmplist.Add("PENDING");
                    table.Add(tmplist);
                } 
                 
                for (var idx = 0;idx < table.Count;idx++)
                {
                    var line = table[idx];
                    var wf = line[0];
                    foreach (var wp in wiplist)
                    {
                        if (wf.Contains(wp.Wafer))
                        {
                            if (wp.Step.Contains("PRE_BURN_IN_TEST"))
                            { line[3] = "PASS"; }
                            if (wp.Step.Contains("POST_BURN_IN_TEST"))
                            { line[4] = "PASS"; }
                            if (wp.Step.Contains("POST_HTOL1_TEST"))
                            { line[5] = "PASS"; }
                            if (wp.Step.Contains("POST_HTOL2_TEST"))
                            { line[6] = "PASS"; }
                        }//end if
                    }//end foreach
                }//end for

                var ret = new JsonResult();
                ret.MaxJsonLength = Int32.MaxValue;
                ret.Data = new
                {
                    title = title,
                    table = table
                };
                return ret;
            }
            else
            {
                var title = new List<string>();
                title.Add("WAT WIP");
                title.Add("TYPE");
                title.Add("START DATE");
                title.Add("PRE-BI");
                title.Add("POST-BI");
                title.Add("HTOL1");
                title.Add("HTOL2");

                var ret = new JsonResult();
                ret.MaxJsonLength = Int32.MaxValue;
                ret.Data = new
                {
                    title = title,
                    table = new List<object>()
                };
                return ret;
            }
        }

        public ActionResult WATCapPredict()
        {
            var url = "/DashBoard/WATCapPredict";
            if (!CheckName(Request.UserHostName, url))
            { return Jump2Welcome(url); }

            return View();
        }

        public JsonResult WATCapPredictDATA()
        {
            var syscfg = CfgUtility.GetSysConfig(this);
            var ydata = new List<object>();
            var xAxis = new List<string>();

            var daystart = DateTime.Parse(DateTime.Now.AddDays(1).ToString("yyyy-MM-dd") + " 00:00:00");

            var datafrom = DateTime.Now.AddDays(-60).ToString("yyyy-MM-dd HH:mm:ss");
            var capvmlist = WATCapacity.GetWATCapacity(datafrom);
            var precaplist = new List<WATCapacity>();
            foreach (var cap in capvmlist)
            {
                var preday = cap.WFDate.AddDays(13);
                if (preday > daystart)
                {
                    cap.WFDate = preday;
                    var capslot = cap.VType.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)[0]+ "SLOT";
                    if (syscfg.ContainsKey(capslot))
                    {
                        cap.OvenSlot = UT.O2I(syscfg[capslot]);
                    }
                    precaplist.Add(cap);
                }
            }

            var datecap = new Dictionary<string, int>();
            foreach (var cap in precaplist)
            {
                var dk = cap.WFDate.ToString("yyyy-MM-dd");
                if (datecap.ContainsKey(dk))
                { datecap[dk] += cap.OvenSlot; }
                else
                { datecap.Add(dk, cap.OvenSlot); }
            }

            xAxis.AddRange(datecap.Keys.ToList());
            xAxis.Sort(delegate(string obj1,string obj2) {
                var d1 = DateTime.Parse(obj1 + " 00:00:00");
                var d2 = DateTime.Parse(obj2 + " 00:00:00");
                return d1.CompareTo(d2);
            });

            var slotlist = new List<int>();
            foreach (var x in xAxis)
            { slotlist.Add(datecap[x]); }

            ydata.Add(new
            {
                name = "AVAILABLE OVEN SLOT",
                data = slotlist,
                color = "#c31a1e",
                yAxis = 0
            });


            var chartdata  =  new
            {
                id = "wat_cap_id",
                title = "WUXI WAT OVEN RESOURCE PREDICT",
                xAxis = xAxis,
                data = ydata
            };

            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                chartdata = chartdata
            };
            return ret;
        }



        public ActionResult WATJOBCheck()
        {
            var url = "/DashBoard/WATJOBCheck";
            if (!CheckName(Request.UserHostName, url))
            { return Jump2Welcome(url); }
            return View();
        }

        public JsonResult LoadWATJOBCheckData()
        {
            var jobchecklist = WATJobCheckVM.GetWATTodaysJobCheck(this);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                jobchecklist = jobchecklist
            };
            return ret;
        }


        private string CheckWATGoldenSampleData()
        {
            var syscfg = CfgUtility.GetSysConfig(this);
            var testerlist = WATGoldSample.GetTodaysWATTesterList();
            if (testerlist.Count == 0)
            { return "Golden sample have not been tested today!"; }

            if (testerlist.Count < 2)
            { return "Golden sample need to be tested at all tester!"; }

            var startdate = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd HH:mm:ss");
            var enddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            foreach (var tester in testerlist)
            {
                var paramlist = syscfg["GOLDSAMPLECHPARAM"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                var chartlist = new List<object>();
                foreach (var param in paramlist)
                {
                    var gsdict = WATGoldSample.GetGoldPwrData(tester, param, startdate, enddate);
                    if (gsdict.Count == 0)
                    { continue; }

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

                        var lastday = datelist[datelist.Count - 1];

                        var valuelist = new List<double>();
                        var datadict = gskv.Value;
                        if (datadict.Count > 0)
                        {
                            var lastval = datadict[lastday][0];

                            foreach (var kv in datadict)
                            {
                                foreach (var val in kv.Value)
                                {
                                    valuelist.Add(val);
                                }
                            }

                            if (valuelist.Count > 10)
                            { valuelist.Remove(lastval); }

                            var mean = Statistics.Mean(valuelist);
                            var stddev = Math.Abs(Statistics.StandardDeviation(valuelist));
                            var ll = mean - 3.0 * stddev;
                            var ul = mean + 3.0 * stddev;

                            if (lastval < ll && lastval > ul)
                            {
                                var msg = "On tester " + tester + ",golden sample " + gskv.Key + " failed on parameter " + param + ". val is " + lastval.ToString() + " ll is " + ll.ToString() + " ul is " + ul.ToString();
                                var towho = syscfg["WATJOBCHECKLIST"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                                EmailUtility.SendEmail(this, "GOLDEN SAMPLE FAIL WARING", towho, msg);
                                new System.Threading.ManualResetEvent(false).WaitOne(500);
                                return msg;
                            }

                        }//end if
                    }//end foreach
                }//end foreach
            }//end foreach

            return string.Empty;
        }

        public string CheckWATCouponBoardQTY(int qty)
        {
            var syscfg = CfgUtility.GetSysConfig(this);

            var daystart = DateTime.Parse(DateTime.Now.AddDays(1).ToString("yyyy-MM-dd") + " 00:00:00");
            var dayend = daystart.AddDays(7);

            var allslot = 0;
            var datafrom = DateTime.Now.AddDays(-60).ToString("yyyy-MM-dd HH:mm:ss");
            var capvmlist = WATCapacity.GetWATCapacity(datafrom);
            var precaplist = new List<WATCapacity>();
            foreach (var cap in capvmlist)
            {
                var preday = cap.WFDate.AddDays(13);
                if (preday > daystart && preday < dayend)
                {
                    cap.WFDate = preday;
                    var capslot = cap.VType.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)[0] + "SLOT";
                    if (syscfg.ContainsKey(capslot))
                    {
                        allslot += UT.O2I(syscfg[capslot]);
                    }
                    precaplist.Add(cap);
                }
            }

            if (qty > allslot)
            {
                return "In furture 7 days,"+allslot+" oven slots are available. On hand coupon board are "+qty+"pcs. So everything goes well!";
            }
            else
            {
                var msg = "In furture 7 days," + allslot + " oven slots are available. On hand coupon board are " + qty + "pcs. A warning is triggled!";
                var towho = syscfg["WATJOBCHECKLIST"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                EmailUtility.SendEmail(this, "OVEN EMPTY SLOT WARNING", towho, msg);
                new System.Threading.ManualResetEvent(false).WaitOne(500);
                return msg;
            }

        }

        public JsonResult UpdateWATJobData()
        {
            var itemid = Request.Form["itemid"];
            var mark = Request.Form["mark"].Trim();
            var username = GetUserName(Request.UserHostName);
            var status = "DONE";
            var MSG = "";

            if (string.Compare(itemid, "1") == 0)
            {
                MSG = CheckWATGoldenSampleData();
                if (!string.IsNullOrEmpty(MSG))
                { status = "PENDING"; }
            }

            if (string.Compare(itemid, "2") == 0)
            {
                var qty = UT.O2I_Check(mark);
                if (qty == -1)
                {
                    MSG = "Please input the coupon board count on your hand!";
                    status = "PENDING";
                }
                else
                { MSG = CheckWATCouponBoardQTY(qty); }
            }

            WATJobCheckVM.UpdateWATCheckVM(itemid, mark, status,username);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                STATUS=status,
                MSG = MSG
            };
            return ret;
        }

        public ActionResult WATCheckHistory()
        { return View(); }

        public JsonResult WATCheckHistoryData()
        {
            var sdate = Request.Form["sdate"];
            if (string.IsNullOrEmpty(sdate))
            { sdate = DateTime.Now.AddDays(-14).ToString("yyyy-MM-dd") + " 00:00:00"; }
            else
            { sdate = DateTime.Parse(sdate).ToString("yyyy-MM-dd") + " 00:00:00"; }
            
            var jobchecklist = WATJobCheckVM.GetWATCheckVM(sdate);
            var ret = new JsonResult();
            ret.MaxJsonLength = Int32.MaxValue;
            ret.Data = new
            {
                jobchecklist = jobchecklist
            };
            return ret;
        }


    }
    
}